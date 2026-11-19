// ReSharper disable StringLiteralTypo
namespace Immutype.Tests.Integration;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class TestExtensions
{
    private static CSharpCompilation CreateCompilation() =>
        CSharpCompilation
            .Create("Sample")
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(GetSystemAssemblyPathByName("netstandard.dll")),
                MetadataReference.CreateFromFile(GetSystemAssemblyPathByName("System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<object>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ImmutableList<object>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SortedSet<object>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SourceGenerator).Assembly.Location));

    public static IReadOnlyList<string> Run(this string setupCode, out string generatedCode, RunOptions? options = null)
    {
        var curOptions = options ?? new RunOptions();
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(curOptions.LanguageVersion);

        var hostCode = """
                       
                                   using System;
                                   using System.Collections.Generic;
                                   using System.Collections.Immutable;
                                   using System.Linq;
                                   using Immutype;
                                   using Sample;
                       
                                   namespace Sample { public class Program { public static void Main() {
                       """ + curOptions.Statements + "} } }";

        var additionalCode = curOptions.AdditionalCode.Select(code => CSharpSyntaxTree.ParseText(code, parseOptions)).ToArray();

        var compilation = CreateCompilation()
            .WithOptions(
                new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                    .WithNullableContextOptions(options?.NullableContextOptions ?? NullableContextOptions.Disable))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(setupCode, parseOptions))
            .AddSyntaxTrees(additionalCode);

        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(
#if ROSLYN38
            [generator],
#else
            [generator.AsSourceGenerator()],
#endif
            parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

        generatedCode = string.Join(Environment.NewLine, driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(source => source.HintName + Environment.NewLine + Environment.NewLine + source.SourceText));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning);
        compilation = ((CSharpCompilation)updatedCompilation)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(hostCode, parseOptions))
            .Check(generatedCode);

        var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var assemblyPath = Path.ChangeExtension(tempFileName, "exe");
        var configPath = Path.ChangeExtension(tempFileName, "runtimeconfig.json");
        var runtime = RuntimeInformation.FrameworkDescription.Split(" ")[1];
        var dotnetVersion = $"{Environment.Version.Major}.{Environment.Version.Minor}";
        var config = """

                     {
                       "runtimeOptions": {
                                 "tfm": "netV.V",
                                 "framework": {
                                     "name": "Microsoft.NETCore.App",
                                     "version": "RUNTIME"
                                 }
                             }
                     }
                     """.Replace("V.V", dotnetVersion).Replace("RUNTIME", runtime);

        try
        {
            var output = new List<string>();
            File.WriteAllText(configPath, config);
            var result = compilation.Emit(assemblyPath);
            Assert.True(result.Success);

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "dotnet",
                ArgumentList = { assemblyPath }
            };

            try
            {
                process.OutputDataReceived += OnOutputDataReceived;
                process.ErrorDataReceived += OnOutputDataReceived;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            finally
            {
                process.OutputDataReceived -= OnOutputDataReceived;
                process.ErrorDataReceived -= OnOutputDataReceived;
            }

            Assert.True(process.ExitCode == 0, string.Join(Environment.NewLine, output));
            return output;

            void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data == null)
                {
                    return;
                }

                lock (output)
                {
                    output.Add(args.Data);
                }
            }
        }
        finally
        {
            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
        }
    }

    private static CSharpCompilation Check(this CSharpCompilation compilation, string generatedCode)
    {
        var errors = (
                from diagnostic in compilation.GetDiagnostics()
                where diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning
                select GetErrorMessage(diagnostic))
            .ToList();

        Assert.False(errors.Count != 0, string.Join(Environment.NewLine + Environment.NewLine, errors) + Environment.NewLine + Environment.NewLine + generatedCode);
        return compilation;
    }

    private static string GetErrorMessage(Diagnostic diagnostic)
    {
        var description = diagnostic.GetMessage();
        if (!diagnostic.Location.IsInSource)
        {
            return description;
        }

        var source = diagnostic.Location.SourceTree.ToString();
        var span = source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);
        return description
               + Environment.NewLine + Environment.NewLine
               + diagnostic
               + Environment.NewLine + Environment.NewLine
               + span
               + Environment.NewLine + Environment.NewLine
               + "Line " + (diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line + 1)
               + Environment.NewLine
               + Environment.NewLine
               + string.Join(
                   Environment.NewLine,
                   source.Split(Environment.NewLine)
                       .Select(
                           (line, number) => $"/*{number + 1:0000}*/ {line}")
               );
    }

    private static string GetSystemAssemblyPathByName(string assemblyName) =>
        Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty, assemblyName);
}

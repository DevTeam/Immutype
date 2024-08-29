// ReSharper disable StringLiteralTypo
namespace Immutype.Tests.Integration;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Core;
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
                MetadataReference.CreateFromFile(typeof(SourceBuilder).Assembly.Location));

    public static IReadOnlyList<string> Run(this string setupCode, out string generatedCode, RunOptions? options = default)
    {
        var curOptions = options ?? new RunOptions();
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(curOptions.LanguageVersion);

        var hostCode = @"
            using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using Immutype;
            using Sample;

            namespace Sample { public class Program { public static void Main() {" + curOptions.Statements + "} } }";

        var additionalCode = curOptions.AdditionalCode.Select(code => CSharpSyntaxTree.ParseText(code, parseOptions)).ToArray();

        var compilation = CreateCompilation()
            .WithOptions(
                new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                    .WithNullableContextOptions(options?.NullableContextOptions ?? NullableContextOptions.Disable))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(setupCode, parseOptions))
            .AddSyntaxTrees(additionalCode);

        var generatedSources = new List<Source>();
        var composition = new Composition();
        generatedSources.AddRange(composition.ComponentsBuilder.Build(CancellationToken.None));
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var tree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            var context = new GenerationContext<SyntaxNode>(parseOptions, compilation, semanticModel, root, CancellationToken.None, ImmutableDictionary<string, string>.Empty);
            generatedSources.AddRange(composition.SourceBuilder.Build(context));
        }

        generatedCode = string.Join(Environment.NewLine, generatedSources.Select((src, index) => $"Generated {index + 1}" + Environment.NewLine + Environment.NewLine + src.Code));
        compilation = compilation
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(hostCode, parseOptions))
            .AddSyntaxTrees(generatedSources.Select(i => CSharpSyntaxTree.ParseText(i.Code.ToString(), parseOptions)).ToArray())
            .Check(generatedCode);

        var tempFileName = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString()[..4]);
        var assemblyPath = Path.ChangeExtension(tempFileName, "exe");
        var configPath = Path.ChangeExtension(tempFileName, "runtimeconfig.json");
        var runtime = RuntimeInformation.FrameworkDescription.Split(" ")[1];
        var dotnetVersion = $"{Environment.Version.Major}.{Environment.Version.Minor}";
        var config = @"
{
  ""runtimeOptions"": {
            ""tfm"": ""netV.V"",
            ""framework"": {
                ""name"": ""Microsoft.NETCore.App"",
                ""version"": ""RUNTIME""
            }
        }
}".Replace("V.V", dotnetVersion).Replace("RUNTIME", runtime);

        try
        {
            var output = new List<string>();
            File.WriteAllText(configPath, config);
            var result = compilation.Emit(assemblyPath);
            Assert.True(result.Success);

            void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    output.Add(args.Data);
                }
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "dotnet",
                    Arguments = assemblyPath
                }
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

            return output;
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
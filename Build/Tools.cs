using System.IO.Compression;
using HostApi;
using NuGet.Versioning;
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable CheckNamespace

static class Tools
{
    public static bool UnderTeamCity => Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != default;
    
    public static void MergeNuGetPackages(IEnumerable<string> mergingPackages, string targetPackage)
    {
        var targetDir = Path.GetDirectoryName(targetPackage);
        if (!string.IsNullOrWhiteSpace(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        if (File.Exists(targetPackage))
        {
            File.Delete(targetPackage);
        }

        using var outStream = File.Create(targetPackage);
        using var outArchive = new ZipArchive(outStream, ZipArchiveMode.Create);
        var buffer = new byte[4096];
        var paths = new HashSet<string>();
        foreach (var package in mergingPackages)
        {
            Info($"Processing \"{package}\".");
            using var inStream = File.OpenRead(package);
            using var inArchive = new ZipArchive(inStream, ZipArchiveMode.Read);
            foreach (var entry in inArchive.Entries)
            {
                if (entry.Length <= 0 || !paths.Add(entry.FullName))
                {
                    Info($"{entry.FullName,-100} - skipped");
                    continue;
                }

                using var prevStream = entry.Open();
                var newEntry = outArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                using var newStream = newEntry.Open();
                int size;
                do
                {
                    size = prevStream.Read(buffer, 0, buffer.Length);
                    if (size > 0)
                    {
                        newStream.Write(buffer, 0, size);
                    }
                } while (size > 0);
                newStream.Flush();
                Info($"{entry.FullName,-100} - merged");
            }
        }
    }
}

static class Version
{
    public static NuGetVersion GetNext(NuGetRestoreSettings settings, NuGetVersion defaultVersion)
    {
        var floatRange = defaultVersion.Release != string.Empty
            ? new FloatRange(NuGetVersionFloatBehavior.Prerelease, defaultVersion)
            : new FloatRange(NuGetVersionFloatBehavior.Minor, defaultVersion);

        return GetService<INuGet>()
            .Restore(settings.WithHideWarningsAndErrors(true).WithVersionRange(new VersionRange(defaultVersion, floatRange)))
            .Where(i => i.Name == settings.PackageId)
            .Select(i => i.NuGetVersion)
            .Select(i => defaultVersion.Release != string.Empty
                ? new NuGetVersion(i.Major, i.Minor, i.Patch, defaultVersion.Release)
                : new NuGetVersion(i.Major, i.Minor, i.Patch + 1))
            .Max() ?? defaultVersion;
    }
}

static class Property
{
    public static string Get(string name, string defaultProp, bool showWarning = false)
    {
        if (Props.TryGetValue(name, out var prop) && !string.IsNullOrWhiteSpace(prop))
        {
            WriteLine($"{name}: {prop}", Color.Highlighted);
            return prop;
        }

        var message = $"The property \"{name}\" was not defined, the default value \"{defaultProp}\" was used.";
        if (showWarning)
        {
            Warning(message);
        }
        else
        {
            Info(message);
        }

        return defaultProp;
    }
}

static class Assertion
{
    public static bool Succeed(int? exitCode, string shortName)
    {
        if (exitCode == 0)
        {
            return true;
        }

        Error($"{shortName} failed.");
        Exit();
        return false;
    }

    public static async Task<bool> Succeed(Task<int?> exitCodeTask, string shortName) =>
        Succeed(await exitCodeTask, shortName);

    private static bool CheckBuildResult(IBuildResult result)
    {
        if (result.ExitCode == 0)
        {
            return true;
        }

        foreach (var failedTest in
                 from testResult in result.Tests
                 where testResult.State == TestState.Failed
                 select testResult.ToString())
        {
            Error(failedTest);
        }

        Error($"{result.StartInfo.ShortName} failed");
        return false;
    }

    public static void Succeed(IBuildResult result)
    {
        if (!CheckBuildResult(result))
        {
            Exit();
        }
    }

    public static async Task<bool> Succeed(Task<IBuildResult> resultTask)
    {
        if (CheckBuildResult(await resultTask))
        {
            return true;
        }

        Exit();
        return true;
    }

    public static async Task<bool> Succeed(Task<IBuildResult[]> resultsTask)
    {
        if ((await resultsTask).All(CheckBuildResult))
        {
            return true;
        }

        Exit();
        return true;
    }

    private static void Exit()
    {
        if (!Tools.UnderTeamCity)
        {
            var foregroundColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                var timeout = TimeSpan.FromSeconds(10);
                var period = TimeSpan.FromMilliseconds(10);
                while (timeout > period)
                {
                    if (Console.KeyAvailable)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Y)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Thread.Sleep(period);
                    timeout -= period;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Continue this build? {(int)timeout.TotalSeconds:00} y/n");
                }
            }
            finally
            {
                Console.ForegroundColor = foregroundColor;
            }
        }
        
        Environment.Exit(1);
    }
}
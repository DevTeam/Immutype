// Run this from the working directory where the solution or project to build is located.

using HostApi;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NuGet.Versioning;

// ReSharper disable ArrangeTypeModifiers

const string solutionFile = "Immutype.sln";
const string configuration = "Release";
const string packageId = "Immutype";
Settings[] allSettings =
[
    new("3.8", "3.8.0"),
    new("4.0", "4.0.1")
];

var currentDir = Environment.CurrentDirectory;
if (!File.Exists(solutionFile))
{
    Error($"Cannot find the solution \"{solutionFile}\". Current directory is \"{currentDir}\".");
    return 1;
}

var outputDir = Path.Combine("Immutype", "Bin", configuration);

// Which Roslyn builds to build and test: all when empty, a list such as "3.8" or "3.8,4.0",
// or "prebuilt" to only merge the packages built and published by another job.
var roslyn = Props.Get("roslyn", "all");
var isPrebuilt = roslyn.Equals("prebuilt", StringComparison.OrdinalIgnoreCase);
var isAllRoslyn = roslyn.Length == 0 || roslyn.Equals("all", StringComparison.OrdinalIgnoreCase);
var requestedRoslynVersions = isPrebuilt || isAllRoslyn
    ? []
    : roslyn.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var unknownRoslynVersions = requestedRoslynVersions.Where(i => allSettings.All(j => j.AnalyzerRoslynVersion != i)).ToArray();
if (unknownRoslynVersions.Length > 0)
{
    Error($"Unknown Roslyn version(s): {string.Join(", ", unknownRoslynVersions)}. Available: {string.Join(", ", allSettings.Select(i => i.AnalyzerRoslynVersion))}.");
    return 1;
}

var buildSettings = isPrebuilt
    ? []
    : isAllRoslyn
        ? allSettings
        : [.. allSettings.Where(i => requestedRoslynVersions.Contains(i.AnalyzerRoslynVersion, StringComparer.Ordinal))];

if (buildSettings.Length == 0 && !isPrebuilt)
{
    Error($"No Roslyn version has been selected by \"roslyn={roslyn}\".");
    return 1;
}

// A job of a matrix builds and tests a single Roslyn version, and a separate job merges the packages.
var merge = !Props.Get("merge", "true").Equals("false", StringComparison.OrdinalIgnoreCase);

// Packages are published only when an API key is provided.
var publish = Props.Get("publish", "true").Equals("true", StringComparison.OrdinalIgnoreCase);
var apiKey = Props.Get("apiKey", "");

// An explicit version, for example the one taken from a release tag, wins over the computed next version.
var nuGetVersion = GetVersion();
if (Props.Get("versionOnly", "").Equals("true", StringComparison.OrdinalIgnoreCase))
{
    // The only line a caller has to parse to get the version of this build.
    WriteLine($"PackageVersion={nuGetVersion}");
    return 0;
}

var packages = new List<string>();
foreach (var settings in buildSettings)
{
    var props = settings.CreateBuildProps(nuGetVersion);

    new DotNetClean()
        .WithConfiguration(configuration)
        .WithProps(props)
        .Build().EnsureSuccess();

    new DotNetTest()
        .WithConfiguration(configuration)
        .WithProps(props)
        .Build().EnsureSuccess();

    var packagePath = Path.GetFullPath(Path.Combine(outputDir, $"roslyn{settings.AnalyzerRoslynVersion}"));
    new DotNetPack()
        .WithConfiguration(configuration)
        .WithNoBuild(true)
        .WithOutput(packagePath)
        .WithProps(props)
        .Build().EnsureSuccess();

    packages.Add(Path.Combine(packagePath, $"{packageId}.{nuGetVersion}.nupkg"));
}

if (isPrebuilt)
{
    foreach (var settings in allSettings)
    {
        var packagePath = Path.GetFullPath(Path.Combine(outputDir, $"roslyn{settings.AnalyzerRoslynVersion}"));
        var package = Path.Combine(packagePath, $"{packageId}.{nuGetVersion}.nupkg");
        if (!File.Exists(package))
        {
            Error($"Cannot find the prebuilt package \"{package}\".");
            return 1;
        }

        packages.Add(package);
    }
}

if (!merge)
{
    WriteLine("Merging the packages was skipped.", Color.Details);
    WriteLine($"Package version: {nuGetVersion}", Color.Highlighted);
    return 0;
}

var mergedPackage = Path.GetFullPath(Path.Combine(outputDir, $"{packageId}.{nuGetVersion}.nupkg"));
Tools.MergeNuGetPackages(packages, mergedPackage);

Info("Publishing artifacts.");
var teamCityWriter = GetService<ITeamCityWriter>();
teamCityWriter.PublishArtifact($"{mergedPackage} => .");

if (publish && !string.IsNullOrWhiteSpace(apiKey))
{
    new DotNetNuGetPush()
        .WithShortName($"Pushing {Path.GetFileName(mergedPackage)}")
        .WithApiKey(apiKey)
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithPackage(mergedPackage)
        .Run().EnsureSuccess();
}

WriteLine($"Package version: {nuGetVersion}", Color.Highlighted);

return 0;

NuGetVersion GetVersion()
{
    var requestedVersion = Props.Get("version", "").Trim();
    if (requestedVersion.Length == 0)
    {
        return Version.GetNext(new NuGetRestoreSettings(packageId).WithPackageType(NuGetPackageType.Tool), new NuGetVersion(1, 0, 0, "dev"));
    }

    if (!NuGetVersion.TryParse(requestedVersion, out var parsedVersion))
    {
        Error($"Invalid NuGet package version: \"{requestedVersion}\".");
        return new NuGetVersion(1, 0, 0, "dev");
    }

    WriteLine($"The package version has been overridden by {parsedVersion}.", Color.Details);
    return parsedVersion;
}

record Settings(string AnalyzerRoslynVersion, string AnalyzerRoslynPackageVersion)
{
    public (string name, string value)[] CreateBuildProps(NuGetVersion nuGetVersion) =>
    [
        ("AnalyzerRoslynVersion", AnalyzerRoslynVersion),
        ("AnalyzerRoslynPackageVersion", AnalyzerRoslynPackageVersion),
        ("version", nuGetVersion.ToString())
    ];
}

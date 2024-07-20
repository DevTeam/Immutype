// Run this from the working directory where the solution or project to build is located.

using HostApi;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NuGet.Versioning;

// ReSharper disable ArrangeTypeModifiers

const string solutionFile = "Immutype.sln";
const string configuration = "Release";
const string packageId = "Immutype";
Settings[] buildSettings = 
[
    new Settings("3.8", "3.8.0"),
    new Settings("4.0", "4.0.1")
];

var currentDir = Environment.CurrentDirectory;
if (!File.Exists(solutionFile))
{
    Error($"Cannot find the solution \"{solutionFile}\". Current directory is \"{currentDir}\".");
    return 1;
}

var outputDir = Path.Combine("Immutype", "Bin", configuration);
var defaultVersion = NuGetVersion.Parse(Props.Get("version", "1.0.0-dev"));
var nuGetVersion = Version.GetNext(new NuGetRestoreSettings(packageId).WithPackageType(NuGetPackageType.Tool), defaultVersion);
var packages = new List<string>();
foreach (var settings in buildSettings)
{
    var props = settings.CreateBuildProps(nuGetVersion);
    
    new DotNetClean()
        .WithConfiguration(configuration)
        .WithProps(props)
        .Build()
        .EnsureSuccess();

    new DotNetTest()
        .WithConfiguration(configuration)
        .WithProps(props)
        .Build()
        .EnsureSuccess();

    var packagePath = Path.GetFullPath(Path.Combine(outputDir, $"roslyn{settings.AnalyzerRoslynVersion}"));
    new DotNetPack()
        .WithConfiguration(configuration)
        .WithNoBuild(true)
        .WithOutput(packagePath)
        .WithProps(props)
        .Build()
        .EnsureSuccess();
    
    var package = Path.Combine(packagePath, $"Immutype.{nuGetVersion}.nupkg");
    packages.Add(package);
}

var mergedPackage = Path.GetFullPath(Path.Combine(outputDir, $"Immutype.{nuGetVersion}.nupkg"));
Tools.MergeNuGetPackages(packages, mergedPackage);

Info("Publishing artifacts.");
var teamCityWriter = GetService<ITeamCityWriter>();
teamCityWriter.PublishArtifact($"{mergedPackage} => .");

var apiKey = Props.Get("apiKey", "");
if (!string.IsNullOrWhiteSpace(apiKey) && nuGetVersion.Release != "dev")
{
    new DotNetNuGetPush()
        .WithShortName($"Pushing {Path.GetFileName(mergedPackage)}")
        .WithApiKey(apiKey)
        .WithSources("https://api.nuget.org/v3/index.json")
        .WithPackage(mergedPackage)
        .Run()
        .EnsureSuccess();
}

WriteLine($"Package version: {nuGetVersion}", Color.Highlighted);

return 0;

record Settings(string AnalyzerRoslynVersion, string AnalyzerRoslynPackageVersion)
{
    public (string name, string value)[] CreateBuildProps(NuGetVersion nuGetVersion) =>
    [
        ("AnalyzerRoslynVersion", AnalyzerRoslynVersion),
        ("AnalyzerRoslynPackageVersion", AnalyzerRoslynPackageVersion),
        ("version", nuGetVersion.ToString())
    ];
}

// Run this from the working directory where the solution or project to build is located.

using HostApi;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NuGet.Versioning;

// ReSharper disable ArrangeTypeModifiers

var cases = new[]
{
    new Case("3.8", "3.8.0"),
    new Case("4.0", "4.0.1")
};

const string solutionFile = "Immutype.sln";
const string configuration = "Release";
const string packageId = "Immutype";

var currentDir = Environment.CurrentDirectory;
if (!File.Exists(solutionFile))
{
    Error($"Cannot find the solution \"{solutionFile}\". Current directory is \"{currentDir}\".");
    return 1;
}

var defaultVersion = NuGetVersion.Parse(Property.Get("version", "1.0.0-dev", Tools.UnderTeamCity));
var apiKey = Property.Get("apiKey", "");

var nuGetVersion = Version.GetNext(new NuGetRestoreSettings(packageId).WithPackageType(NuGetPackageType.Tool), defaultVersion);

var output = Path.Combine("Immutype", "Bin", configuration);
var packages = new List<string>();
foreach (var @case in cases)
{
    var props = new List<(string name, string value)>(@case.Props)
    {
        ("version", nuGetVersion.ToString())
    };

    Assertion.Succeed(
        new DotNetClean()
            .WithConfiguration(configuration)
            .WithProps(props)
            .Build());

    Assertion.Succeed(
        new DotNetTest()
            .WithConfiguration(configuration)
            .WithProps(props)
            .Build());

    Assertion.Succeed(
        new DotNetPack()
            .WithConfiguration(configuration)
            .WithNoBuild(true)
            .WithProps(props)
            .Build());

    packages.Add(Path.Combine(output, $"roslyn{@case.AnalyzerRoslynVersion}", $"Immutype.{nuGetVersion}.nupkg"));
}

var package = Path.Combine(output, $"Immutype.{nuGetVersion}.nupkg");
Tools.MergeNuGetPackages(packages, package);

Info("Publishing artifacts.");
var teamCityWriter = GetService<ITeamCityWriter>();
teamCityWriter.PublishArtifact($"{package} => .");

if (!string.IsNullOrWhiteSpace(apiKey) && nuGetVersion.Release != "dev")
{
    Assertion.Succeed(
        new DotNetNuGetPush()
            .WithApiKey(apiKey)
            .WithSources("https://api.nuget.org/v3/index.json")
            .WithPackage(package)
            .Run(),
        $"Pushing {Path.GetFileName(package)}");
}

WriteLine($"Package version: {nuGetVersion}", Color.Highlighted);

return 0;

record Case(string AnalyzerRoslynVersion, string AnalyzerRoslynPackageVersion)
{
    public IEnumerable<(string name, string value)> Props =>
        new[]
        {
            ("AnalyzerRoslynVersion", AnalyzerRoslynVersion),
            ("AnalyzerRoslynPackageVersion", AnalyzerRoslynPackageVersion)
        };
}

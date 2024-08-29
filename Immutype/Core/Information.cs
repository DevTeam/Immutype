namespace Immutype.Core;

using System.Reflection;

internal sealed class Information : IInformation
{
    private static readonly string CurrentDescription = nameof(Immutype);
    
    static Information()
    {
        var assembly = typeof(Information).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(version))
        {
            CurrentDescription = $"{CurrentDescription} {version}";
        }
    }

    public string Description => CurrentDescription;
}
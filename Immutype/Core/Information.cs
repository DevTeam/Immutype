namespace Immutype.Core;

using System.Reflection;

internal sealed class Information : IInformation
{
    private static readonly string CurrentDescription = nameof(Immutype);
    
    static Information()
    {
        var version = typeof(Information)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(version))
        {
            CurrentDescription = $"{CurrentDescription} {version}";
        }
    }

    public string Description => CurrentDescription;
}
namespace Immutype.Core;

using System.IO;
using System.Text.RegularExpressions;

internal class ComponentsBuilder : IComponentsBuilder
{
    private static readonly List<(string name, string text)> Components;

    static ComponentsBuilder()
    {
        Regex componentsRegex = new(@"Immutype.Components.[\w]+.cs", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        Components = GetResources(componentsRegex).Select(i => (i.file, i.code)).ToList();
    }

    public IEnumerable<Source> Build(CancellationToken cancellationToken) =>
        from source in Components
        where !cancellationToken.IsCancellationRequested
        select new Source(source.name, SourceText.From(source.text, Encoding.UTF8));

    private static IEnumerable<(string file, string code)> GetResources(Regex filter)
    {
        var assembly = typeof(ComponentsBuilder).Assembly;
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!filter.IsMatch(resourceName))
            {
                continue;
            }

            using var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Cannot read {resourceName}."));
            var code = reader.ReadToEnd();
            yield return (resourceName, code);
        }
    }
}
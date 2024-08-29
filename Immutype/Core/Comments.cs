// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

internal class Comments : IComments
{
    public ImmutableDictionary<string, string> GetParamsComments(IEnumerable<SyntaxTrivia> comments)
    {
        var text = new StringBuilder();
        foreach (var comment in comments)
        {
            text.Append(comment.ToFullString());
        }

        var lines = 
            from rawLine in text.ToString().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
            let trimmedLine = rawLine.Trim()
            where trimmedLine.StartsWith("///")
            let line = trimmedLine.Substring(3).TrimStart()
            select line;

        text.Clear();
        text.AppendLine("<doc>");
        foreach (var line in lines)
        {
            text.AppendLine(line);
        }
        text.AppendLine("</doc>");

        var reader = new StringReader(text.ToString());
        var doc = XDocument.Load(reader);
        var paramComments = 
            from paramElement in doc.XPathSelectElements("doc/param")
            let comment = paramElement.Value
            where !string.IsNullOrWhiteSpace(comment)
            let name = paramElement.Attribute("name").Value
            where !string.IsNullOrWhiteSpace(name)
            select (name, comment);

        var result = new Dictionary<string, string>();
        foreach (var (name, comment) in paramComments)
        {
            result[name] = comment;
        }

        return result.ToImmutableDictionary();
    }
}
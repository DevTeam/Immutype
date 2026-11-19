// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

internal class CommentsGenerator : ICommentsGenerator
{
    public void Write(
        LineBuilder code,
        GenerationContext<TypeDeclarationSyntax> context,
        string title,
        ParameterSyntax parameter,
        string? targetComment)
    {
        var simpleComments = new List<string>();
        if (context.DocComments.TryGetValue(parameter.Identifier.ValueText, out var docComment))
        {
            simpleComments.Add(docComment);
        }

        simpleComments.AddRange(
            from trivia in parameter.GetLeadingTrivia()
            where trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            let comment = trivia.ToString()
            where comment.StartsWith("//") && !comment.StartsWith("///")
            let text = comment.Substring(2).Trim()
            where text.Length > 0
            select text);

        if (simpleComments.Count == 0)
        {
            return;
        }

        code.AppendLine("/// <summary>");
        var first = true;
        foreach (var line in simpleComments.SelectMany(comment => comment.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)))
        {
            code.AppendLine(first ? $"/// {title} {line}" : $"/// {line}");
            first = false;
        }

        code.AppendLine("/// </summary>");
        code.AppendLine("/// <param name=\"it\">The original instance.</param>");
        if (!string.IsNullOrWhiteSpace(targetComment))
        {
            code.AppendLine($"/// <param name=\"{parameter.Identifier.ValueText}\">{targetComment}</param>");
        }

        code.AppendLine("/// <returns>The modified copy of the original instance.</returns>");
    }
}

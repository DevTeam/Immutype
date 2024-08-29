namespace Immutype.Core;

internal class CommentsGenerator : ICommentsGenerator
{
    public T AddComments<T>(
        GenerationContext<TypeDeclarationSyntax> context,
        string title,
        ParameterSyntax parameter,
        string targetComment,
        T target)
        where T: SyntaxNode
    {
        var comments = (
                from trivia in parameter.GetLeadingTrivia()
                where trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                let str = trivia.ToFullString().Trim()
                where !string.IsNullOrWhiteSpace(str)
                select str)
            .ToArray();

        var simpleComments = new List<string>();
        if (context.DocComments.TryGetValue(parameter.Identifier.Text, out var docComment))
        {
            simpleComments.Add(docComment);
        }

        simpleComments.AddRange(
            from comment in comments
            where comment.StartsWith("//") && !comment.StartsWith("///")
            let str = comment.Substring(2, comment.Length-2).Trim()
            where !string.IsNullOrWhiteSpace(str)
            select str);

        if (!simpleComments.Any())
        {
            return target;
        }
        
        var lines = new List<string> { "/// <summary>" };
        for (var i = 0; i < simpleComments.Count; i++)
        {
            var comment = simpleComments[i];
            if (i == 0)
            {
                comment = $"{title} {comment}";
            }

            lines.Add($"/// {comment}");
        }
            
        lines.Add("/// </summary>");
        lines.Add("/// <param name=\"it\">The original instance.</param>");
        if (!string.IsNullOrWhiteSpace(targetComment))
        {
            lines.Add($"/// <param name=\"{parameter.Identifier.Text}\">{targetComment}</param>");
        }

        lines.Add("/// <returns>The modified copy of the original instance.</returns>");
        return target.WithLeadingTrivia(lines.SelectMany(s => new List<SyntaxTrivia> { SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed, SyntaxFactory.Comment(s)}));
    }
}
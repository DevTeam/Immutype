namespace Immutype.Core;

public class CommentsGenerator : ICommentsGenerator
{
    public T AddComments<T>(
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

        var simpleComments = (
                from comment in comments
                where comment.StartsWith("//") && !comment.StartsWith("///")
                let str = comment.Substring(2, comment.Length-2).Trim()
                where !string.IsNullOrWhiteSpace(str)
                select str)
            .ToArray();

        if (simpleComments.Any())
        {
            var lines = new List<string>();
            lines.Add("/// <summary>");
            for (var i = 0; i < simpleComments.Length; i++)
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
            return target.WithLeadingTrivia(lines.SelectMany(s => new List<SyntaxTrivia>() { SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed, SyntaxFactory.Comment(s)}));
        }
        
        return target;
    }
}
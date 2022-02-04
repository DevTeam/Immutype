namespace Immutype.Core;

public interface ICommentsGenerator
{
    T AddComments<T>(
        string title,
        ParameterSyntax parameter,
        string targetComment,
        T target)
        where T : SyntaxNode;
}
namespace Immutype.Core;

internal interface ICommentsGenerator
{
    T AddComments<T>(
        GenerationContext<TypeDeclarationSyntax> context,
        string title,
        ParameterSyntax parameter,
        string targetComment,
        T target)
        where T : SyntaxNode;
}
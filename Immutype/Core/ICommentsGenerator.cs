namespace Immutype.Core;

internal interface ICommentsGenerator
{
    void Write(
        LineBuilder code,
        GenerationContext<TypeDeclarationSyntax> context,
        string title,
        ParameterSyntax parameter,
        string? targetComment);
}

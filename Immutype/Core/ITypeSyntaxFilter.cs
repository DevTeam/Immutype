namespace Immutype.Core;

internal interface ITypeSyntaxFilter
{
    bool IsAccepted(TypeDeclarationSyntax typeDeclarationSyntax);
}
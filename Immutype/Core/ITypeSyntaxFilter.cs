namespace Immutype.Core
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface ITypeSyntaxFilter
    {
        bool IsAccepted(TypeDeclarationSyntax typeDeclarationSyntax);
    }
}
// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

internal class TypeSyntaxFilter(ISyntaxNodeFactory syntaxNodeFactory) : ITypeSyntaxFilter
{
    public bool IsAccepted(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        if (!syntaxNodeFactory.IsAccessible(typeDeclarationSyntax.Modifiers))
        {
            return false;
        }

        if (typeDeclarationSyntax.Ancestors().OfType<TypeDeclarationSyntax>().Any())
        {
            return false;
        }

        if (!syntaxNodeFactory.HasTargetAttribute(typeDeclarationSyntax))
        {
            return false;
        }

        if (typeDeclarationSyntax is RecordDeclarationSyntax { ParameterList.Parameters.Count: > 0 })
        {
            return true;
        }

        return typeDeclarationSyntax.Members
            .OfType<ConstructorDeclarationSyntax>()
            .Any(ctor => ctor.ParameterList.Parameters.Count > 0 && ctor.Modifiers.Any(i => i.IsKind(SyntaxKind.PublicKeyword) || i.IsKind(SyntaxKind.InternalKeyword)) || !ctor.Modifiers.Any());
    }
}
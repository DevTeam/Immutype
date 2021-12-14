// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class TypeSyntaxFilter : ITypeSyntaxFilter
    {
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;

        public TypeSyntaxFilter(ISyntaxNodeFactory syntaxNodeFactory) =>
            _syntaxNodeFactory = syntaxNodeFactory;

        public bool IsAccepted(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            if (!_syntaxNodeFactory.IsAccessible(typeDeclarationSyntax.Modifiers))
            {
                return false;
            }

            if (typeDeclarationSyntax.Ancestors().OfType<TypeDeclarationSyntax>().Any())
            {
                return false;
            }
            
            if (!_syntaxNodeFactory.HasTargetAttribute(typeDeclarationSyntax))
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
}
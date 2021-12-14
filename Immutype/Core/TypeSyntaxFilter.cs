// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core
{
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class TypeSyntaxFilter : ITypeSyntaxFilter
    {
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;

        public TypeSyntaxFilter(ISyntaxNodeFactory syntaxNodeFactory)
        {
            _syntaxNodeFactory = syntaxNodeFactory;
        }

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
            
            if (typeDeclarationSyntax.AttributeLists
                .SelectMany(i => i.Attributes)
                .Select(i => _syntaxNodeFactory.GetUnqualified(i.Name)?.ToString())
                .All(i => i != "Target" && i != "TargetAttribute"))
            {
                return false;
            }

            return typeDeclarationSyntax switch
            {
                RecordDeclarationSyntax recordDeclarationSyntax => recordDeclarationSyntax.ParameterList is { Parameters.Count: > 0 },
                _ => typeDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>().Any(i => i.ParameterList.Parameters.Count > 0)
            };
        }
    }
}
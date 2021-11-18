// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable LoopCanBeConvertedToQuery
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    internal class ExtensionsFactory : IUnitFactory
    {
        private readonly IMethodsFactory _methodsFactory;

        public ExtensionsFactory(IMethodsFactory methodsFactory) =>
            _methodsFactory = methodsFactory;

        public IEnumerable<Source> Create(TypeDeclarationSyntax typeDeclarationSyntax, IReadOnlyList<ParameterSyntax> parameters, CancellationToken cancellationToken)
        {
            var ns = typeDeclarationSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().Reverse().ToArray();
            var typeName = string.Join(".", ns.Select(i => i.Name.ToString()).Concat(new []{typeDeclarationSyntax.Identifier.Text}));
            var typeSyntax = SyntaxFactory.ParseName(typeName);
            var className = $"{typeDeclarationSyntax.Identifier.Text}Extensions";
            var extensionsClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(typeDeclarationSyntax.Modifiers.Where(i => !i.IsKind(SyntaxKind.ReadOnlyKeyword) && !i.IsKind(SyntaxKind.PartialKeyword)).ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(_methodsFactory.Create(typeDeclarationSyntax, typeSyntax, parameters, cancellationToken).ToArray());

            var usingDirectives = typeDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()
                .Concat(new []
                {
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq"))
                })
                .GroupBy(i => i.Name.ToString())
                .Select(i => i.First())
                .ToArray();
            
            var compilationUnit = SyntaxFactory.CompilationUnit().AddUsings(usingDirectives);
            NamespaceDeclarationSyntax? prevNamespaceNode = null;
            foreach (var originalNamespaceNode in ns.Reverse())
            {
                var namespaceNode = 
                    SyntaxFactory.NamespaceDeclaration(originalNamespaceNode.Name)
                        .AddUsings(originalNamespaceNode.Usings.ToArray());

                prevNamespaceNode = prevNamespaceNode == null ? namespaceNode : prevNamespaceNode.AddMembers(namespaceNode);
            }
            
            if (prevNamespaceNode != null)
            {
                prevNamespaceNode = prevNamespaceNode.AddMembers(extensionsClass);
                compilationUnit = compilationUnit.AddMembers(prevNamespaceNode);
            }
            else
            {
                compilationUnit = compilationUnit.AddMembers(extensionsClass);
            }

            var code = compilationUnit.NormalizeWhitespace().ToString();
            yield return new Source(className, SourceText.From(code, Encoding.UTF8));
        }
    }
}
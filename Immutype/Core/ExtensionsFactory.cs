// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable IdentifierTypo
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
#if ROSLYN38
    using NamespaceType = Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax;
#else
    using NamespaceType = Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax;
#endif

    internal class ExtensionsFactory : IUnitFactory
    {
        private static readonly UsingDirectiveSyntax[] AdditionalUsings = {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq"))
        };
    
        private readonly IMethodsFactory _methodsFactory;

        public ExtensionsFactory(IMethodsFactory methodsFactory) =>
            _methodsFactory = methodsFactory;

        public IEnumerable<Source> Create(GenerationContext<TypeDeclarationSyntax> context, IReadOnlyList<ParameterSyntax> parameters)
        {
            var typeDeclarationSyntax = context.Syntax;
            var ns = typeDeclarationSyntax.Ancestors()
#if ROSLYN38
                .OfType<NamespaceDeclarationSyntax>()
#else
                .OfType<BaseNamespaceDeclarationSyntax>()
#endif
                .Reverse()
                .ToArray();

            var typeName = 
                string.Join(".", ns.Select(i => i.Name.ToString())
                    .Concat(new []{typeDeclarationSyntax.Identifier.Text + typeDeclarationSyntax.TypeParameterList}));
            var typeSyntax = SyntaxFactory.ParseName(typeName);
            var className = $"{typeDeclarationSyntax.Identifier.Text}Extensions";
            var extensionsClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(typeDeclarationSyntax.Modifiers.Where(i => !i.IsKind(SyntaxKind.ReadOnlyKeyword) && !i.IsKind(SyntaxKind.PartialKeyword)).ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(_methodsFactory.Create(context, typeSyntax, parameters).ToArray());

            extensionsClass = TryAddAttribute(context.SemanticModel, extensionsClass, "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage");

            var code = CreateRootNode(typeDeclarationSyntax, AdditionalUsings, extensionsClass).NormalizeWhitespace().ToString();
            var fileName = string.Join(".", ns.Select(i => i.Name.ToString()).Concat(new []{typeDeclarationSyntax.Identifier.Text}));
            yield return new Source(fileName, SourceText.From(code, Encoding.UTF8));
        }

        private static CompilationUnitSyntax CreateRootNode(SyntaxNode targetNode, UsingDirectiveSyntax[] additionalUsings, params MemberDeclarationSyntax[] members)
        {
            var namespaces = targetNode.Ancestors().OfType<NamespaceType>();
            NamespaceType? rootNamespace = default;
            foreach (var ns in namespaces)
            {
                var nextNs = ns.WithMembers(new SyntaxList<MemberDeclarationSyntax>(Enumerable.Empty<MemberDeclarationSyntax>()));
                rootNamespace = rootNamespace == default 
                    ? nextNs.AddMembers(members).AddUsings(GetUsings(nextNs.Usings, additionalUsings))
                    : nextNs.AddMembers(rootNamespace);
            }
            
            var baseCompilationUnit = targetNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
            var rootCompilationUnit = (baseCompilationUnit ?? SyntaxFactory.CompilationUnit())
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(Enumerable.Empty<MemberDeclarationSyntax>()));

            return rootNamespace != default
                ? rootCompilationUnit.AddMembers(rootNamespace)
                : rootCompilationUnit.AddUsings(GetUsings(rootCompilationUnit.Usings, additionalUsings)).AddMembers(members);
        }

        private static UsingDirectiveSyntax[] GetUsings(IEnumerable<UsingDirectiveSyntax> usings, IEnumerable<UsingDirectiveSyntax> additionalUsings)
        {
            var currentUsins = usings.Select(i => i.Name.ToString()).ToImmutableHashSet();
            return additionalUsings.Where(i => !currentUsins.Contains(i.Name.ToString())).ToArray();
        }
        
        private static ClassDeclarationSyntax TryAddAttribute(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax, string attributeClassName)
        {
            var excludeFromCodeCoverageType = semanticModel.Compilation.GetTypeByMetadataName(attributeClassName + "Attribute");
            if (excludeFromCodeCoverageType != default)
            {
                classDeclarationSyntax = classDeclarationSyntax
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeClassName))));
            }

            return classDeclarationSyntax;
        }
    }
}
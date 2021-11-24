#if ROSLYN40
namespace Immutype
{
    using Immutype;
    using Immutype.Core;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                foreach (var source in Composer.ResolveIComponentsBuilder().Build(ctx.CancellationToken))
                {
                    ctx.AddSource(source.HintName, source.Code);
                }
            });

            var sourceBuilder = Composer.ResolveISourceBuilder();
            var typeSyntaxFilter = Composer.ResolveITypeSyntaxFilter();
            var changes = context.SyntaxProvider.CreateSyntaxProvider(
                (node, token) => node is TypeDeclarationSyntax typeDeclarationSyntax && typeSyntaxFilter.IsAccepted(typeDeclarationSyntax),
                (syntaxContext, token) => (TypeDeclarationSyntax)syntaxContext.Node)
                .Collect();
            
            context.RegisterSourceOutput(changes, (ctx, syntax) =>
            {
                foreach (var typeDeclarationSyntax in syntax)
                {
                    foreach (var source in sourceBuilder.Build(typeDeclarationSyntax, ctx.CancellationToken))
                    {
                        ctx.AddSource(source.HintName, source.Code);
                    }
                }
            });
        }
    }
}
#endif
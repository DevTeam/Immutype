#if ROSLYN40
namespace Immutype
{
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
                (node, _) => node is TypeDeclarationSyntax typeDeclarationSyntax && typeSyntaxFilter.IsAccepted(typeDeclarationSyntax),
                (syntaxContext, _) => (TypeDeclarationSyntax)syntaxContext.Node)
                .Combine(context.ParseOptionsProvider)
                .Collect();

            context.RegisterSourceOutput(changes, (ctx, changes) =>
            {
                foreach (var (typeDeclarationSyntax, options) in changes)
                {
                    foreach (var source in sourceBuilder.Build(options, typeDeclarationSyntax, ctx.CancellationToken))
                    {
                        ctx.AddSource(source.HintName, source.Code);
                    }
                }
            });
        }
    }
}
#endif
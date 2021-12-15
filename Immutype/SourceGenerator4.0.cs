﻿#if ROSLYN40
namespace Immutype
{
    using Core;
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
                (syntaxContext, _) => syntaxContext)
                .Combine(context.ParseOptionsProvider)
                .Combine(context.CompilationProvider)
                .Collect();

            context.RegisterSourceOutput(changes, (ctx, changes) =>
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                // ReSharper disable once UseDeconstruction
                foreach (var change in changes)
                {
                    var generationContext = new GenerationContext<TypeDeclarationSyntax>(change.Left.Right, change.Right, change.Left.Left.SemanticModel, (TypeDeclarationSyntax)change.Left.Left.Node, ctx.CancellationToken);
                    foreach (var source in sourceBuilder.Build(generationContext))
                    {
                        ctx.AddSource(source.HintName, source.Code);
                    }
                }
            });
        }
    }
}
#endif
#if !ROSLYN38
namespace Immutype
{
    using Core;

    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            /*if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }*/

            var composition = new Composition();
            context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (ctx, options) =>
            {
                if (options.GlobalOptions.TryGetValue("build_property.ImmutypeAPI", out var valueStr)
                    && bool.TryParse(valueStr, out var value)
                    && value == false)
                {
                    return;
                }
                
                foreach (var source in composition.ComponentsBuilder.Build(ctx.CancellationToken))
                {
                    ctx.AddSource(source.HintName, source.Code);
                }
            });

            var changes = context.SyntaxProvider.CreateSyntaxProvider(
                    (node, _) => node is TypeDeclarationSyntax typeDeclarationSyntax && composition.SyntaxFilter.IsAccepted(typeDeclarationSyntax),
                    (syntaxContext, _) => syntaxContext)
                .Combine(context.ParseOptionsProvider)
                .Combine(context.CompilationProvider)
                .Collect();

            // ReSharper disable once VariableHidesOuterVariable
            context.RegisterSourceOutput(changes, (ctx, changes) =>
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                // ReSharper disable once UseDeconstruction
                foreach (var change in changes)
                {
                    var generationContext = new GenerationContext<TypeDeclarationSyntax>(change.Left.Right, change.Right, change.Left.Left.SemanticModel, (TypeDeclarationSyntax)change.Left.Left.Node, ctx.CancellationToken);
                    foreach (var source in composition.SourceBuilder.Build(generationContext))
                    {
                        ctx.AddSource(source.HintName, source.Code);
                    }
                }
            });
        }
    }
}
#endif
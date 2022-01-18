#if ROSLYN38
namespace Immutype
{
    using System.Diagnostics;
    using Core;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class SourceGenerator: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            /*if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }*/

            if (!(context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ImmutypeAPI", out var valueStr) && bool.TryParse(valueStr, out var value) && value == false))
            {
                foreach (var source in Composer.ResolveIComponentsBuilder().Build(context.CancellationToken))
                {
                    context.AddSource(source.HintName, source.Code);
                }
            }

            var sourceBuilder = Composer.ResolveISourceBuilder();
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var semanticModel = context.Compilation.GetSemanticModel(tree);
                var generationContext = new GenerationContext<SyntaxNode>(context.ParseOptions, context.Compilation, semanticModel, tree.GetRoot(), context.CancellationToken);
                foreach (var source in sourceBuilder.Build(generationContext))
                {
                    context.AddSource(source.HintName, source.Code);
                }
            }
        }
    }
}
#endif
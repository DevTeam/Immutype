#if ROSLYN38
namespace Immutype
{
    using Core;

    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private static readonly Composition Composition = new();
        
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            /*if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }*/

            var root = Composition.Root;
            if (!(context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ImmutypeAPI", out var valueStr) && bool.TryParse(valueStr, out var value) && value == false))
            {
                foreach (var source in root.ComponentsBuilder.Build(context.CancellationToken))
                {
                    context.AddSource(source.HintName, source.Code);
                }
            }

            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var semanticModel = context.Compilation.GetSemanticModel(tree);
                var generationContext = new GenerationContext<SyntaxNode>(context.ParseOptions, context.Compilation, semanticModel, tree.GetRoot(), context.CancellationToken, ImmutableDictionary<string, string>.Empty);
                foreach (var source in root.SourceBuilder.Build(generationContext))
                {
                    context.AddSource(source.HintName, source.Code);
                }
            }
        }
    }
}
#endif
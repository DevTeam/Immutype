#if ROSLYN38
namespace Immutype
{
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

            foreach (var source in Composer.ResolveIComponentsBuilder().Build(context.CancellationToken))
            {
                context.AddSource(source.HintName, source.Code);
            }
            
            foreach (var source in Composer.ResolveISourceBuilder().Build(context.ParseOptions, context.Compilation.SyntaxTrees, context.CancellationToken))
            {
                context.AddSource(source.HintName, source.Code);
            }
        }
    }
}
#endif


// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype;

using Core;
using Pure.DI;

internal partial class Composition
{
    private static void Setup() => DI.Setup(nameof(Composition))
        .DefaultLifetime(Lifetime.PerBlock)
        
        .Root<ISourceBuilder>("SourceBuilder")
        .Root<IComponentsBuilder>("ComponentsBuilder")
        .Root<ITypeSyntaxFilter>("SyntaxFilter")
        
        .Bind<ImmutableArray<TT>>()
            .To<ImmutableArray<TT>>(ctx =>
            {
                ctx.Inject(out TT[] arr);
                return ImmutableArray.Create(arr);
            })
        .Bind<ISourceBuilder>().To<SourceBuilder>()
        .Bind<ISyntaxNodeFactory>().To<SyntaxNodeFactory>()
        .Bind<INameService>().To<NameService>()
        .Bind<IUnitFactory>().To<ExtensionsFactory>()
        .Bind<IMethodsFactory>().To<MethodsFactory>()
        .Bind<IDataContainerFactory>().To<DataContainerFactory>()
        .Bind<IMethodFactory>(typeof(MethodWithFactory)).To<MethodWithFactory>()
        .Bind<IMethodFactory>(typeof(MethodAddRemoveFactory)).To<MethodAddRemoveFactory>()
        .Bind<ICommentsGenerator>().To<CommentsGenerator>()
    
        .DefaultLifetime(Lifetime.Singleton)
        .Bind<IComponentsBuilder>().To<ComponentsBuilder>()
        .Bind<ITypeSyntaxFilter>().To<TypeSyntaxFilter>();
}
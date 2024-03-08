

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
        
        .Bind()
            .To<ImmutableArray<TT>>(ctx =>
            {
                ctx.Inject(out TT[] arr);
                return ImmutableArray.Create(arr);
            })
        .Bind().To<SourceBuilder>()
        .Bind().To<SyntaxNodeFactory>()
        .Bind().To<NameService>()
        .Bind().To<ExtensionsFactory>()
        .Bind().To<MethodsFactory>()
        .Bind().To<DataContainerFactory>()
        .Bind(Tag.Type).To<MethodWithFactory>()
        .Bind(Tag.Type).To<MethodAddRemoveFactory>()
        .Bind().To<CommentsGenerator>()
    
        .DefaultLifetime(Lifetime.Singleton)
        .Bind().To<ComponentsBuilder>()
        .Bind().To<TypeSyntaxFilter>();
}
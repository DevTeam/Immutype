

// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype;

using Core;
using Pure.DI;

internal partial class Composition
{
    private static void Setup() => DI.Setup()
        .Root<ISourceBuilder>(nameof(SourceBuilder))
        .Root<IComponentsBuilder>(nameof(ComponentsBuilder))
        .Root<ITypeSyntaxFilter>(nameof(SyntaxFilter))
        
        .DefaultLifetime(Lifetime.PerBlock)
            .Bind().To((TT[] arr) => ImmutableArray.Create(arr))
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


// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype;

using Core;
using Pure.DI;

internal partial class Composition
{
    private static void Setup() => DI.Setup()
        .Root<(ISourceBuilder SourceBuilder, IComponentsBuilder ComponentsBuilder, ITypeSyntaxFilter SyntaxFilter)>(nameof(Root))
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
        .Bind().To<Information>()
        .Bind().To<Comments>()
        .Bind().To<TypeSyntaxFilter>()
        .Bind().To<ComponentsBuilder>();
}
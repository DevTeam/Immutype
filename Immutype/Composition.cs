

// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype;

using Core;
using Pure.DI;
using static Pure.DI.Lifetime;
using static Pure.DI.Tag;

internal partial class Composition
{
    private static void Setup() => DI.Setup()
        .Root<(
            ISourceBuilder SourceBuilder,
            IComponentsBuilder ComponentsBuilder,
            ITypeSyntaxFilter SyntaxFilter)>(nameof(Root))

        .DefaultLifetime(PerBlock)
        .Bind().To<SourceBuilder>()
        .Bind().To<SyntaxNodeFactory>()
        .Bind().To<NameService>()
        .Bind().To<ExtensionsFactory>()
        .Bind().To<MethodsFactory>()
        .Bind().To<DataContainerFactory>()
        .Bind(Unique).To<MethodWithFactory>()
        .Bind(Unique).To<MethodAddRemoveFactory>()
        .Bind().To<CommentsGenerator>()
        .Bind().To<Information>()
        .Bind().To<Comments>()
        .Bind().To<TypeSyntaxFilter>()
        .Bind().To<ComponentsBuilder>();
}
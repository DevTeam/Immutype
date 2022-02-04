

// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype;

using Core;
using Pure.DI;

internal static partial class Composer
{
    private static void Setup() => DI.Setup()
        .Default(Lifetime.Singleton)
        .Bind<ISourceBuilder>().To<SourceBuilder>()
        .Bind<IComponentsBuilder>().To<ComponentsBuilder>()
        .Bind<ITypeSyntaxFilter>().To<TypeSyntaxFilter>()
        .Bind<ISyntaxNodeFactory>().To<SyntaxNodeFactory>()
        .Bind<INameService>().To<NameService>()
        .Bind<IUnitFactory>().To<ExtensionsFactory>()
        .Bind<IMethodsFactory>().To<MethodsFactory>()
        .Bind<IDataContainerFactory>().To<DataContainerFactory>()
        .Bind<IMethodFactory>(typeof(MethodWithFactory)).To<MethodWithFactory>()
        .Bind<IMethodFactory>(typeof(MethodAddRemoveFactory)).To<MethodAddRemoveFactory>()
        .Bind<ICommentsGenerator>().To<CommentsGenerator>();
}
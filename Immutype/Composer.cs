using Immutype.Core;
using Pure.DI;
// ReSharper disable UnusedMember.Local
// ReSharper disable PartialTypeWithSinglePart

namespace Immutype
{
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
                .Bind<IMethodFactory>(typeof(MethodWithFactory)).To<MethodWithFactory>()
                .Bind<IMethodFactory>(typeof(MethodAddFactory)).To<MethodAddFactory>();
    }
}
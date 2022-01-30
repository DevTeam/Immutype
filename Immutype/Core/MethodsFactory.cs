// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

internal class MethodsFactory : IMethodsFactory
{
    private readonly ImmutableArray<IMethodFactory> _methodFactories;
    private readonly ISyntaxNodeFactory _syntaxNodeFactory;

    public MethodsFactory(ImmutableArray<IMethodFactory> methodFactories, ISyntaxNodeFactory syntaxNodeFactory)
    {
        _methodFactories = methodFactories;
        _syntaxNodeFactory = syntaxNodeFactory;
    }

    public IEnumerable<MemberDeclarationSyntax> Create(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax targetType, IReadOnlyList<ParameterSyntax> parameters)
    {
        var thisParameter =
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("it"))
                .WithType(targetType)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword));

        if (
            context.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp7_2 }
            && _syntaxNodeFactory.IsReadonlyType(context.Syntax))
        {
            thisParameter = thisParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
        }

        return
            from currentParameter in parameters
            let parameterType = currentParameter.Type
            where parameterType != default
            from methodFactory in _methodFactories
            from method in methodFactory.Create(context, targetType, parameters, currentParameter, thisParameter)
            where !context.CancellationToken.IsCancellationRequested
            select method;
    }
}
// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

internal class MethodsFactory(
    IReadOnlyCollection<IMethodFactory> methodFactories,
    ISyntaxNodeFactory syntaxNodeFactory)
    : IMethodsFactory
{
    public IEnumerable<MemberDeclarationSyntax> Create(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax targetType, IReadOnlyList<ParameterSyntax> parameters)
    {
        var thisParameter =
            SyntaxRepo.Parameter(SyntaxFactory.Identifier("it"))
                .WithType(targetType)
                .AddModifiers(SyntaxKind.ThisKeyword.WithSpace());

        if (
            context.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp7_2 }
            && syntaxNodeFactory.IsReadonlyType(context.Syntax))
        {
            thisParameter = thisParameter.AddModifiers(SyntaxKind.InKeyword.WithSpace());
        }

        return
            from currentParameter in parameters
            let parameterType = currentParameter.Type
            where parameterType != null
            from methodFactory in methodFactories
            from method in methodFactory.Create(context, targetType, parameters, currentParameter, thisParameter)
            where !context.CancellationToken.IsCancellationRequested
            select method;
    }
}
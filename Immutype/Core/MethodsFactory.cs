namespace Immutype.Core;

internal sealed class MethodsFactory(
    IReadOnlyCollection<IMethodFactory> methodFactories,
    ISyntaxInfo syntaxInfo,
    INameService nameService,
    IDataContainerFactory dataContainerFactory) : IMethodsFactory
{
    public void Build(LineBuilder code, GenerationContext<TypeDeclarationSyntax> context, string targetType, IReadOnlyList<ParameterSyntax> parameters)
    {
        var typeParameters = context.Syntax.TypeParameterList is { } list
            ? $"<{string.Join(", ", list.Parameters.Select(parameter => parameter.Identifier.Text))}>"
            : "";
        var modifier = context.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp7_2 }
            && context.Syntax.Modifiers.Any(token => token.IsKind(SyntaxKind.ReadOnlyKeyword))
            ? "this in"
            : "this";
        var target = targetType + typeParameters;
        var extensions = new ExtensionMethods(
            code, context, target, typeParameters, $"{modifier} {target} it", !syntaxInfo.IsValueType(context.Syntax),
            [.. parameters.Select(parameter => $"it.{syntaxInfo.GetMemberName(context.Syntax, parameter.Identifier)}")]);

        for (var index = 0; index < parameters.Count; index++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var parameter = parameters[index];
            if (parameter.Type is not { } type)
            {
                continue;
            }

            var method = new MethodContext(
                extensions,
                parameter,
                index,
                nameService.ConvertToName(parameter.Identifier.ValueText),
                dataContainerFactory.GetCollection(type),
                syntaxInfo.NeedsNullGuard(context.Compilation, type) ? type.ToString() : null);

            foreach (var factory in methodFactories)
            {
                factory.Build(method);
            }
        }
    }
}

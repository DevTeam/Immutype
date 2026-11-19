namespace Immutype.Core;

internal interface IMethodsFactory
{
    void Build(LineBuilder code, GenerationContext<TypeDeclarationSyntax> context, string targetType, IReadOnlyList<ParameterSyntax> parameters);
}

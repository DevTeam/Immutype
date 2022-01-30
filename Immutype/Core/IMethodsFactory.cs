namespace Immutype.Core;

internal interface IMethodsFactory
{
    IEnumerable<MemberDeclarationSyntax> Create(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax targetType, IReadOnlyList<ParameterSyntax> parameters);
}
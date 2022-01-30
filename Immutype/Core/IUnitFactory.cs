namespace Immutype.Core;

internal interface IUnitFactory
{
    IEnumerable<Source> Create(GenerationContext<TypeDeclarationSyntax> context, IReadOnlyList<ParameterSyntax> parameters);
}
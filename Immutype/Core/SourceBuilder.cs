// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf
namespace Immutype.Core;

internal class SourceBuilder(
    ITypeSyntaxFilter typeSyntaxFilter,
    IUnitFactory unitFactory,
    ISyntaxNodeFactory syntaxNodeFactory)
    : ISourceBuilder
{
    public IEnumerable<Source> Build(GenerationContext<SyntaxNode> context) =>
        from typeDeclarationSyntax in context.Syntax.DescendantNodes().OfType<TypeDeclarationSyntax>()
        where !context.CancellationToken.IsCancellationRequested
        where typeSyntaxFilter.IsAccepted(typeDeclarationSyntax)
        from source in Build(new GenerationContext<TypeDeclarationSyntax>(context.Options, context.Compilation, context.SemanticModel, typeDeclarationSyntax, context.CancellationToken, ImmutableDictionary<string, string>.Empty))
        select source;

    public IEnumerable<Source> Build(GenerationContext<TypeDeclarationSyntax> context)
    {
        IReadOnlyList<ParameterSyntax>? parameters = null;
        if (context.Syntax is RecordDeclarationSyntax recordDeclarationSyntax && recordDeclarationSyntax.ParameterList?.Parameters.Count != 0)
        {
            parameters = recordDeclarationSyntax.ParameterList?.Parameters;
        }

        parameters ??= (
                from ctor in context.Syntax.Members.OfType<ConstructorDeclarationSyntax>()
                where !context.CancellationToken.IsCancellationRequested
                where ctor.ParameterList.Parameters.Count > 0 && ctor.Modifiers.Any(i => i.IsKind(SyntaxKind.PublicKeyword) || i.IsKind(SyntaxKind.InternalKeyword)) || !ctor.Modifiers.Any()
                let weight = ctor.ParameterList.Parameters.Count + (syntaxNodeFactory.HasTargetAttribute(ctor) ? 0xffff : 0)
                orderby weight descending
                select ctor)
            .FirstOrDefault()
            ?.ParameterList
            .Parameters;

        return parameters != null
            ? unitFactory.Create(context, parameters)
            : [];
    }
}
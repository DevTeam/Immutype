// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf
namespace Immutype.Core;

internal class SourceBuilder : ISourceBuilder
{
    private readonly ITypeSyntaxFilter _typeSyntaxFilter;
    private readonly IUnitFactory _unitFactory;
    private readonly ISyntaxNodeFactory _syntaxNodeFactory;

    public SourceBuilder(
        ITypeSyntaxFilter typeSyntaxFilter,
        IUnitFactory unitFactory,
        ISyntaxNodeFactory syntaxNodeFactory)
    {
        _typeSyntaxFilter = typeSyntaxFilter;
        _unitFactory = unitFactory;
        _syntaxNodeFactory = syntaxNodeFactory;
    }

    public IEnumerable<Source> Build(GenerationContext<SyntaxNode> context) =>
        from typeDeclarationSyntax in context.Syntax.DescendantNodes().OfType<TypeDeclarationSyntax>()
        where !context.CancellationToken.IsCancellationRequested
        where _typeSyntaxFilter.IsAccepted(typeDeclarationSyntax)
        from source in Build(new GenerationContext<TypeDeclarationSyntax>(context.Options, context.Compilation, context.SemanticModel, typeDeclarationSyntax, context.CancellationToken, ImmutableDictionary<string, string>.Empty))
        select source;

    public IEnumerable<Source> Build(GenerationContext<TypeDeclarationSyntax> context)
    {
        IReadOnlyList<ParameterSyntax>? parameters = default;
        if (context.Syntax is RecordDeclarationSyntax recordDeclarationSyntax && recordDeclarationSyntax.ParameterList?.Parameters.Count != 0)
        {
            parameters = recordDeclarationSyntax.ParameterList?.Parameters;
        }

        parameters ??= (
                from ctor in context.Syntax.Members.OfType<ConstructorDeclarationSyntax>()
                where !context.CancellationToken.IsCancellationRequested
                where ctor.ParameterList.Parameters.Count > 0 && ctor.Modifiers.Any(i => i.IsKind(SyntaxKind.PublicKeyword) || i.IsKind(SyntaxKind.InternalKeyword)) || !ctor.Modifiers.Any()
                let weight = ctor.ParameterList.Parameters.Count + (_syntaxNodeFactory.HasTargetAttribute(ctor) ? 0xffff : 0)
                orderby weight descending
                select ctor)
            .FirstOrDefault()
            ?.ParameterList
            .Parameters;

        return parameters != default
            ? _unitFactory.Create(context, parameters)
            : [];
    }
}
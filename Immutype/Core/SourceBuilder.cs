// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class SourceBuilder: ISourceBuilder
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

        public IEnumerable<Source> Build(IEnumerable<SyntaxTree> trees, CancellationToken cancellationToken) =>
            from syntaxTree in trees
            where !cancellationToken.IsCancellationRequested
            from typeDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
            where !cancellationToken.IsCancellationRequested
            where _typeSyntaxFilter.IsAccepted(typeDeclarationSyntax)
            from source in Build(typeDeclarationSyntax, cancellationToken)
            select source;
        
        public IEnumerable<Source> Build(TypeDeclarationSyntax typeDeclarationSyntax, CancellationToken cancellationToken)
        {
            IReadOnlyList<ParameterSyntax>? parameters = default;
            if (typeDeclarationSyntax is RecordDeclarationSyntax recordDeclarationSyntax && recordDeclarationSyntax.ParameterList?.Parameters.Count != 0)
            {
                parameters = recordDeclarationSyntax.ParameterList?.Parameters;
            }

            parameters ??= (
                    from ctor in typeDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>()
                    where !cancellationToken.IsCancellationRequested
                    where ctor.ParameterList.Parameters.Count > 0 && ctor.Modifiers.Any(i => i.IsKind(SyntaxKind.PublicKeyword) || i.IsKind(SyntaxKind.InternalKeyword)) || !ctor.Modifiers.Any()
                    let weight = ctor.ParameterList.Parameters.Count + (_syntaxNodeFactory.HasTargetAttribute(ctor) ? 0xffff : 0)
                    orderby weight descending
                    select ctor)
                .FirstOrDefault()
                ?.ParameterList
                .Parameters;
            
            return parameters != default
                ? _unitFactory.Create(typeDeclarationSyntax, parameters, cancellationToken)
                : Enumerable.Empty<Source>();
        }
    }
}
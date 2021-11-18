// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
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

        public SourceBuilder(
            ITypeSyntaxFilter typeSyntaxFilter,
            IUnitFactory unitFactory)
        {
            _typeSyntaxFilter = typeSyntaxFilter;
            _unitFactory = unitFactory;
        }

        public IEnumerable<Source> Build(IEnumerable<SyntaxTree> trees, CancellationToken cancellationToken) =>
            from syntaxTree in trees
            where !cancellationToken.IsCancellationRequested
            from recordSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
            where !cancellationToken.IsCancellationRequested
            where _typeSyntaxFilter.IsAccepted(recordSyntax)
            from source in Build(recordSyntax, cancellationToken) select source;
        
        public IEnumerable<Source> Build(TypeDeclarationSyntax typeDeclarationSyntax, CancellationToken cancellationToken)
        {
            IReadOnlyList<ParameterSyntax>? parameters = typeDeclarationSyntax switch
            {
                RecordDeclarationSyntax recordDeclarationSyntax => recordDeclarationSyntax.ParameterList?.Parameters,
                _ => (
                    from ctor in typeDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>()
                    where !cancellationToken.IsCancellationRequested
                    where ctor.Modifiers.Any(i => i.IsKind(SyntaxKind.PublicKeyword) || i.IsKind(SyntaxKind.InternalKeyword)) || !ctor.Modifiers.Any()
                    orderby ctor.ParameterList.Parameters.Count descending select ctor
                    )
                    .FirstOrDefault()
                    ?.ParameterList
                    .Parameters
            };

            return parameters != default ? _unitFactory.Create(typeDeclarationSyntax, parameters, cancellationToken) : Enumerable.Empty<Source>();
        }
    }
}
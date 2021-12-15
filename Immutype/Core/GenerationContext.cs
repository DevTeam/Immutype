namespace Immutype.Core
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    internal class GenerationContext<TSyntax>
        where TSyntax: SyntaxNode
    {
        public readonly ParseOptions Options;
        public readonly Compilation Compilation;
        public readonly SemanticModel SemanticModel;
        public readonly TSyntax Syntax;
        public readonly CancellationToken CancellationToken;

        public GenerationContext(ParseOptions options, Compilation compilation, SemanticModel semanticModel, TSyntax syntax, CancellationToken cancellationToken)
        {
            Options = options;
            Compilation = compilation;
            SemanticModel = semanticModel;
            Syntax = syntax;
            CancellationToken = cancellationToken;
        }
    }
}
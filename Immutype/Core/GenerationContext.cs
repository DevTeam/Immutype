namespace Immutype.Core;

internal record GenerationContext<TSyntax>(
    ParseOptions Options,
    Compilation Compilation,
    TSyntax Syntax,
    CancellationToken CancellationToken,
    ImmutableDictionary<string, string> DocComments)
    where TSyntax : SyntaxNode;

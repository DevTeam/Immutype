namespace Immutype.Core
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal interface ISourceBuilder
    {
        IEnumerable<Source> Build(IEnumerable<SyntaxTree> trees, CancellationToken cancellationToken);

        IEnumerable<Source> Build(TypeDeclarationSyntax typeDeclarationSyntax, CancellationToken cancellationToken);
    }
}
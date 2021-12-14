// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace Immutype.Core
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal interface ISourceBuilder
    {
        IEnumerable<Source> Build(ParseOptions parseOptions, IEnumerable<SyntaxTree> trees, CancellationToken cancellationToken);

        IEnumerable<Source> Build(ParseOptions parseOptions, TypeDeclarationSyntax typeDeclarationSyntax, CancellationToken cancellationToken);
    }
}
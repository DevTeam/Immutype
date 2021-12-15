// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace Immutype.Core
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal interface ISourceBuilder
    {
        IEnumerable<Source> Build(GenerationContext<SyntaxNode> context);

        IEnumerable<Source> Build(GenerationContext<TypeDeclarationSyntax> context);
    }
}
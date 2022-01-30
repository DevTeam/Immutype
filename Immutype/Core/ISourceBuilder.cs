// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace Immutype.Core;

internal interface ISourceBuilder
{
    IEnumerable<Source> Build(GenerationContext<SyntaxNode> context);

    IEnumerable<Source> Build(GenerationContext<TypeDeclarationSyntax> context);
}
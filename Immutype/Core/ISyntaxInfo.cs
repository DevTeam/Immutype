namespace Immutype.Core;

internal interface ISyntaxInfo
{
    bool IsValueType(TypeDeclarationSyntax type);

    bool HasTargetAttribute(MemberDeclarationSyntax member);

    bool IsAccessible(IEnumerable<SyntaxToken> modifiers);

    string GetMemberName(TypeDeclarationSyntax owner, SyntaxToken parameter);

    bool NeedsNullGuard(Compilation compilation, TypeSyntax type);
}

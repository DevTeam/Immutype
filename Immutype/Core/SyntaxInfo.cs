namespace Immutype.Core;

internal sealed class SyntaxInfo : ISyntaxInfo
{
    public bool IsValueType(TypeDeclarationSyntax type) =>
        type is StructDeclarationSyntax
#if !ROSLYN38
            || type.IsKind(SyntaxKind.RecordStructDeclaration)
#endif
        ;

    public bool HasTargetAttribute(MemberDeclarationSyntax member) =>
        member.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(attribute => attribute.Name is QualifiedNameSyntax qualified ? qualified.Right : attribute.Name)
            .Any(name => name.ToString() is "Target" or "TargetAttribute");

    public bool IsAccessible(IEnumerable<SyntaxToken> modifiers) =>
        !modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)
            || modifier.IsKind(SyntaxKind.PrivateKeyword)
            || modifier.IsKind(SyntaxKind.ProtectedKeyword));

    public string GetMemberName(TypeDeclarationSyntax owner, SyntaxToken parameter)
    {
        foreach (var member in owner.Members.Where(member => IsAccessible(member.Modifiers)))
        {
            switch (member)
            {
                case PropertyDeclarationSyntax property when Matches(property.Identifier, parameter):
                    return property.Identifier.Text;

                case FieldDeclarationSyntax field:
                    foreach (var variable in field.Declaration.Variables.Where(variable => Matches(variable.Identifier, parameter)))
                    {
                        return variable.Identifier.Text;
                    }

                    break;
            }
        }

        return parameter.Text;
    }

    public bool NeedsNullGuard(Compilation compilation, TypeSyntax type) =>
        type switch
        {
            ArrayTypeSyntax => true,
            NullableTypeSyntax or TupleTypeSyntax => false,
            _ => type.ToString() is "string" or "object"
                || compilation.GetTypeByMetadataName(type.ToString())?.IsReferenceType == true
        };

    private static bool Matches(SyntaxToken member, SyntaxToken parameter) =>
        member.ValueText.Equals(parameter.ValueText, StringComparison.CurrentCultureIgnoreCase);
}

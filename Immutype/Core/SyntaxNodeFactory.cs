// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

using System.Runtime.CompilerServices;

internal class SyntaxNodeFactory : ISyntaxNodeFactory
{
    private static readonly AttributeSyntax AggressiveInliningAttr = SyntaxFactory.Attribute(
        SyntaxFactory.IdentifierName(typeof(MethodImplAttribute).FullName ?? string.Empty),
        SyntaxFactory.AttributeArgumentList()
            .AddArguments(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.ParseTypeName(typeof(MethodImplOptions).FullName ?? string.Empty),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0x100))))));

    private static readonly AttributeSyntax PureAttr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(PureAttribute).FullName ?? string.Empty));

    public bool IsValueType(TypeDeclarationSyntax typeDeclarationSyntax) =>
        typeDeclarationSyntax switch
        {
            StructDeclarationSyntax => true,
#if ROSLYN38
                RecordDeclarationSyntax => false,
#else
            RecordDeclarationSyntax => typeDeclarationSyntax.IsKind(SyntaxKind.RecordStructDeclaration),
#endif
            ClassDeclarationSyntax => false,
            _ => false
        };

    public bool IsReadonlyType(TypeDeclarationSyntax typeDeclarationSyntax) =>
        typeDeclarationSyntax.Modifiers.Any(i => i.IsKind(SyntaxKind.ReadOnlyKeyword));

    public bool HasTargetAttribute(MemberDeclarationSyntax memberDeclarationSyntax) =>
        memberDeclarationSyntax.AttributeLists
            .SelectMany(i => i.Attributes)
            .Select(i => GetUnqualified(i.Name)?.ToString())
            .Any(i => i is "Target" or "TargetAttribute");

    public TypeSyntax? GetUnqualified(TypeSyntax? typeSyntax) =>
        typeSyntax is QualifiedNameSyntax qualifiedType ? qualifiedType.Right : typeSyntax;

    public bool IsAccessible(IEnumerable<SyntaxToken> modifiers) =>
        !modifiers.Any(i => i.IsKind(SyntaxKind.StaticKeyword) || i.IsKind(SyntaxKind.PrivateKeyword) || i.IsKind(SyntaxKind.ProtectedKeyword));

    public ReturnStatementSyntax CreateReturnStatement(TypeSyntax typeSyntax, IEnumerable<ArgumentSyntax> arguments) =>
        SyntaxRepo.ReturnStatement(
            SyntaxRepo.ObjectCreationExpression(typeSyntax)
                .AddArgumentListArguments(
                    arguments.ToArray()))
            .WithNewLine();

    public MethodDeclarationSyntax CreateExtensionMethod(TypeSyntax returnTypeSyntax, string name) =>
        SyntaxRepo.MethodDeclaration(returnTypeSyntax, name)
            .AddModifiers(
                SyntaxKind.PublicKeyword.WithSpace(),
                SyntaxKind.StaticKeyword.WithSpace())
            .WithNewLine()
            .AddAttributeLists(
                SyntaxFactory.AttributeList()
                    .AddAttributes(AggressiveInliningAttr, PureAttr));

    public ArgumentSyntax CreateTransientArgument(TypeDeclarationSyntax owner, ParameterSyntax thisParameter, ParameterSyntax parameter) =>
        SyntaxFactory.Argument(CreateTransientArgumentExpression(owner, thisParameter, parameter));

    public MemberAccessExpressionSyntax CreateTransientArgumentExpression(TypeDeclarationSyntax owner, ParameterSyntax thisParameter, ParameterSyntax parameter) =>
        SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(thisParameter.Identifier),
            SyntaxFactory.IdentifierName(GetIdentifier(owner, parameter.Identifier)));

    public IEnumerable<StatementSyntax> CreateGuards(GenerationContext<TypeDeclarationSyntax> context, ParameterSyntax parameter, bool force)
    {
        if (parameter.Type is null or NullableTypeSyntax)
        {
            yield break;
        }

        if (!force && !IsReferenceType(context, parameter.Type))
        {
            yield break;
        }

        var checkDefault = SyntaxFactory.BinaryExpression(
            SyntaxKind.EqualsExpression,
            SyntaxFactory.IdentifierName(parameter.Identifier),
            SyntaxFactory.DefaultExpression(parameter.Type));

        yield return
            SyntaxFactory.IfStatement(
                checkDefault,
                SyntaxRepo.ThrowStatement(
                    SyntaxRepo.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName(
                            SyntaxFactory.Identifier("System.ArgumentNullException"))).AddArgumentListArguments(
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(parameter.Identifier.Text))))))
                .WithNewLine();
    }

    private static bool IsReferenceType(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax type)
    {
        bool? isReference = type switch
        {
            ArrayTypeSyntax => true,
            NullableTypeSyntax => false,
            TupleTypeSyntax => false,
            _ => null
        };

        if (isReference.HasValue)
        {
            return isReference.Value;
        }

        var typeName = type.ToString();
        return typeName switch
        {
            "string" or "object" => true,
            _ => context.Compilation.GetTypeByMetadataName(typeName)?.IsReferenceType ?? false
        };
    }

    private SyntaxToken GetIdentifier(TypeDeclarationSyntax owner, SyntaxToken identifier)
    {
        foreach (var memberDeclarationSyntax in owner.Members.Where(memberDeclarationSyntax => IsAccessible(memberDeclarationSyntax.Modifiers)))
        {
            switch (memberDeclarationSyntax)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    if (propertyDeclaration.Identifier.Text.Equals(identifier.Text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return propertyDeclaration.Identifier;
                    }

                    break;

                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    foreach (var variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration.Variables.Where(variableDeclaratorSyntax => variableDeclaratorSyntax.Identifier.Text.Equals(identifier.Text, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        return variableDeclaratorSyntax.Identifier;
                    }

                    break;
            }
        }

        return identifier;
    }
}
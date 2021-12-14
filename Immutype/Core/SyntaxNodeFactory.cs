// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class SyntaxNodeFactory : ISyntaxNodeFactory
    {
        private static readonly AttributeSyntax AggressiveInliningAttr = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName(typeof(MethodImplAttribute).FullName),
            SyntaxFactory.AttributeArgumentList()
                .AddArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.ParseTypeName(typeof(MethodImplOptions).FullName),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0x100))))));

        private static readonly AttributeSyntax PureAttr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(PureAttribute).FullName));
        
        public TypeSyntax? GetUnqualified(TypeSyntax? typeSyntax) =>
            typeSyntax is QualifiedNameSyntax qualifiedType ? qualifiedType.Right : typeSyntax;
        
        public bool IsAccessible(IEnumerable<SyntaxToken> modifiers) =>
            !modifiers.Any(i => i.IsKind(SyntaxKind.StaticKeyword) || i.IsKind(SyntaxKind.PrivateKeyword) || i.IsKind(SyntaxKind.ProtectedKeyword));
        
        public ReturnStatementSyntax CreateReturnStatement(TypeSyntax typeSyntax, IEnumerable<ArgumentSyntax> arguments) =>
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.ObjectCreationExpression(typeSyntax)
                    .AddArgumentListArguments(
                        arguments.ToArray()));
        
        public MethodDeclarationSyntax CreateExtensionMethod(TypeSyntax returnTypeSyntax, string name) =>
            SyntaxFactory.MethodDeclaration(returnTypeSyntax, name)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword))
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
        
        public IEnumerable<StatementSyntax> CreateGuards(ParameterSyntax parameter)
        {
            if (parameter.Type is null or NullableTypeSyntax)
            {
                yield break;
            }
            
            var checkNotValueType = SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.TypeOfExpression(parameter.Type),
                    SyntaxFactory.IdentifierName(nameof(Type.IsValueType))));

            var checkDefault = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("object"),
                        SyntaxFactory.IdentifierName(nameof(Equals))))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameter.Identifier)),
                    SyntaxFactory.Argument(SyntaxFactory.DefaultExpression(parameter.Type)));

            yield return 
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, checkNotValueType, checkDefault),
                    SyntaxFactory.ThrowStatement(
                        SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.IdentifierName(
                                    SyntaxFactory.Identifier("System.ArgumentNullException"))).
                            AddArgumentListArguments(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(parameter.Identifier.Text))))));
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
}
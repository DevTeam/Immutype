namespace Immutype.Core
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface ISyntaxNodeFactory
    {
        bool IsValueType(TypeDeclarationSyntax typeDeclarationSyntax);

        bool HasTargetAttribute(MemberDeclarationSyntax memberDeclarationSyntax);
            
        TypeSyntax? GetUnqualified(TypeSyntax? typeSyntax);

        bool IsAccessible(IEnumerable<SyntaxToken> modifiers);
        
        ReturnStatementSyntax CreateReturnStatement(TypeSyntax typeSyntax, IEnumerable<ArgumentSyntax> arguments);
        
        MethodDeclarationSyntax CreateExtensionMethod(TypeSyntax returnTypeSyntax, string name);

        ArgumentSyntax CreateTransientArgument(TypeDeclarationSyntax owner, ParameterSyntax thisParameter, ParameterSyntax parameter);
        
        MemberAccessExpressionSyntax CreateTransientArgumentExpression(TypeDeclarationSyntax owner, ParameterSyntax thisParameter, ParameterSyntax parameter);

        IEnumerable<StatementSyntax> CreateGuards(ParameterSyntax parameter);
    }
}
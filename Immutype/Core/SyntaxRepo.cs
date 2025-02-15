namespace Immutype.Core;

internal static class SyntaxRepo
{
    public static SyntaxToken WithSpace(this SyntaxKind syntaxKind) =>
        SyntaxFactory.Token(SyntaxFactory.TriviaList(), syntaxKind, SyntaxFactory.TriviaList(SyntaxFactory.ElasticSpace));
    
    public static SyntaxToken WithSpace(this SyntaxToken syntaxToken) =>
        syntaxToken.WithLeadingTrivia(syntaxToken.LeadingTrivia.Concat([SyntaxFactory.ElasticSpace]));

    private static SyntaxToken WithNewLine(this SyntaxKind syntaxKind) =>
        SyntaxFactory.Token(SyntaxFactory.TriviaList(), syntaxKind, SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed));

    public static TSyntax WithSpace<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode => 
        node.WithLeadingTrivia(node.GetLeadingTrivia().Concat([SyntaxFactory.ElasticSpace]));

    public static TSyntax WithNewLine<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode =>
        node.WithLeadingTrivia(node.GetLeadingTrivia().Concat([SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed]));

    public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, ArgumentListSyntax? argumentList = null, InitializerExpressionSyntax? initializer = null)
        => SyntaxFactory.ObjectCreationExpression(SyntaxKind.NewKeyword.WithSpace(), type, argumentList, initializer);
    
    public static ReturnStatementSyntax ReturnStatement(ExpressionSyntax? expression = null)
        => SyntaxFactory.ReturnStatement(default, SyntaxKind.ReturnKeyword.WithSpace(), expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    public static MethodDeclarationSyntax MethodDeclaration(TypeSyntax returnType, string identifier)
        => SyntaxFactory.MethodDeclaration(default, default, returnType, null, SyntaxFactory.Identifier(identifier).WithSpace(), null, SyntaxFactory.ParameterList(), default, null, null, default);

    public static ClassDeclarationSyntax ClassDeclaration(string identifier)
        => SyntaxFactory.ClassDeclaration(default, default, SyntaxKind.ClassKeyword.WithSpace(), SyntaxFactory.Identifier(identifier), null, null, default, SyntaxKind.OpenBraceToken.WithNewLine(), default, SyntaxKind.CloseBraceToken.WithNewLine(), default);
    
    public static ParameterSyntax Parameter(SyntaxToken identifier)
        => SyntaxFactory.Parameter(default, default, null, identifier.WithSpace(), null);
    
    public static ThrowStatementSyntax ThrowStatement(ExpressionSyntax? expression = null)
        => SyntaxFactory.ThrowStatement(default, SyntaxKind.ThrowKeyword.WithSpace(), expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
}
namespace Immutype.Core;

internal static class SyntaxRepo
{
    public static SyntaxToken WithSpace(this SyntaxKind syntaxKind) =>
        SyntaxFactory.Token(SyntaxFactory.TriviaList(), syntaxKind, SyntaxFactory.TriviaList(SyntaxFactory.ElasticSpace));
    
    public static SyntaxToken WithSpace(this SyntaxToken syntaxToken) =>
        syntaxToken.WithLeadingTrivia(syntaxToken.LeadingTrivia.Concat(new []{SyntaxFactory.ElasticSpace}));

    private static SyntaxToken WithNewLine(this SyntaxKind syntaxKind) =>
        SyntaxFactory.Token(SyntaxFactory.TriviaList(), syntaxKind, SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed));

    public static TSyntax WithSpace<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode => 
        node.WithLeadingTrivia(node.GetLeadingTrivia().Concat(new []{SyntaxFactory.ElasticSpace}));

    public static TSyntax WithNewLine<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode =>
        node.WithLeadingTrivia(node.GetLeadingTrivia().Concat(new []{SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed}));
    
    public static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression)
        => SyntaxFactory.ExpressionStatement(default, expression, SyntaxKind.SemicolonToken.WithNewLine());
    
    public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, ArgumentListSyntax? argumentList = default, InitializerExpressionSyntax? initializer = default)
        => SyntaxFactory.ObjectCreationExpression(SyntaxKind.NewKeyword.WithSpace(), type, argumentList, initializer);
    
#pragma warning disable RS0027
    public static ReturnStatementSyntax ReturnStatement(ExpressionSyntax? expression = default)
        => SyntaxFactory.ReturnStatement(default, SyntaxKind.ReturnKeyword.WithSpace(), expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
#pragma warning restore RS0027
    
    public static ArrayCreationExpressionSyntax ArrayCreationExpression(ArrayTypeSyntax type, InitializerExpressionSyntax? initializer = default)
        => SyntaxFactory.ArrayCreationExpression(SyntaxKind.NewKeyword.WithSpace(), type, initializer);
    
    public static MethodDeclarationSyntax MethodDeclaration(TypeSyntax returnType, string identifier)
        => SyntaxFactory.MethodDeclaration(default, default, returnType, default, SyntaxFactory.Identifier(identifier).WithSpace(), default, SyntaxFactory.ParameterList(), default, default, default, default);
    
    public static MethodDeclarationSyntax MethodDeclaration(TypeSyntax returnType, SyntaxToken identifier)
        => SyntaxFactory.MethodDeclaration(default, default, returnType, default, identifier.WithSpace(), default, SyntaxFactory.ParameterList(), default, default, default, default);
    
    public static ClassDeclarationSyntax ClassDeclaration(string identifier)
        => SyntaxFactory.ClassDeclaration(default, default, SyntaxKind.ClassKeyword.WithSpace(), SyntaxFactory.Identifier(identifier), default, default, default, SyntaxKind.OpenBraceToken.WithNewLine(), default, SyntaxKind.CloseBraceToken.WithNewLine(), default);
    
    public static ParameterSyntax Parameter(SyntaxToken identifier)
        => SyntaxFactory.Parameter(default, default, default, identifier.WithSpace(), default);
    
#pragma warning disable RS0027
    public static ThrowStatementSyntax ThrowStatement(ExpressionSyntax? expression = default)
        => SyntaxFactory.ThrowStatement(default, SyntaxKind.ThrowKeyword.WithSpace(), expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
#pragma warning restore RS0027
}
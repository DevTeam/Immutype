namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class MethodAddFactory : IMethodFactory
    {
        private readonly INameService _nameService;
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;

        public MethodAddFactory(
            INameService nameService,
            ISyntaxNodeFactory syntaxNodeFactory)
        {
            _nameService = nameService;
            _syntaxNodeFactory = syntaxNodeFactory;
        }

        public MethodDeclarationSyntax? Create(TypeDeclarationSyntax targetDeclaration, TypeSyntax targetType, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax thisParameter)
        {
            if (currentParameter.Type == default)
            {
                return default;
            }
            
            var elementType = GetElementType(currentParameter.Type);
            if (elementType == default)
            {
                return default;
            }

            var arrayType = SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier());
            var arrayParameter = SyntaxFactory.Parameter(currentParameter.Identifier).WithType(arrayType).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
            var arguments = CreateAddArguments(targetDeclaration, thisParameter, parameters, currentParameter, arrayParameter);
            return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"Add{_nameService.ConvertToName(currentParameter.Identifier.Text)}")
                .AddParameterListParameters(thisParameter, arrayParameter)
                .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, arguments));
        }
        
        private TypeSyntax? GetElementType(TypeSyntax typeSyntax) =>
            _syntaxNodeFactory.GetUnqualified(typeSyntax) switch
            {
                GenericNameSyntax genericNameSyntax => genericNameSyntax.TypeArgumentList.Arguments[0],
                ArrayTypeSyntax arrayTypeSyntax => arrayTypeSyntax.ElementType,
                _ => default
            };
        
        private IEnumerable<ArgumentSyntax> CreateAddArguments(TypeDeclarationSyntax owner, ParameterSyntax thisParameter, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax arrayParameter)
        {
            foreach (ParameterSyntax parameter in parameters)
            {
                if (parameter == currentParameter)
                {
                    yield return SyntaxFactory.Argument(
                        CreateAddExpression(
                            _syntaxNodeFactory.CreateTransientArgumentExpression(owner, thisParameter, currentParameter),
                            currentParameter,
                            arrayParameter));
                }
                else
                {
                    yield return _syntaxNodeFactory.CreateTransientArgument(owner, thisParameter, parameter);
                }
            }
        }

        private ExpressionSyntax CreateAddExpression(ExpressionSyntax thisExpression, BaseParameterSyntax currentParameter, ParameterSyntax arrayParameter)
        {
            var concatCall = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    thisExpression,
                    SyntaxFactory.IdentifierName(nameof(Enumerable.Concat))))
                .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(arrayParameter.Identifier)));

            var list = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    concatCall,
                    SyntaxFactory.IdentifierName(nameof(Enumerable.ToList))));

            switch (_syntaxNodeFactory.GetUnqualified(currentParameter.Type))
            {
                case GenericNameSyntax genericNameSyntax:
                    if (genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
                    {
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case "List":
                                return list;
                            
                            case "ImmutableList":
                            case "IImmutableList":
                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        concatCall,
                                        SyntaxFactory.IdentifierName(nameof(ImmutableList.ToImmutableList))));
                            
                            case "ImmutableArray":
                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        concatCall,
                                        SyntaxFactory.IdentifierName(nameof(ImmutableArray.ToImmutableArray))));
                        }
                    }

                    break;
                
                case ArrayTypeSyntax:
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            list,
                            SyntaxFactory.IdentifierName(nameof(List<object>.ToArray))));
            }
            
            var readonlyList = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    list,
                    SyntaxFactory.IdentifierName(nameof(List<object>.AsReadOnly))));
            
            return readonlyList;
        }
    }
}
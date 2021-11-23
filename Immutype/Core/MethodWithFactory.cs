// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class MethodWithFactory : IMethodFactory
    {
        private readonly INameService _nameService;
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;

        public MethodWithFactory(
            INameService nameService,
            ISyntaxNodeFactory syntaxNodeFactory)
        {
            _nameService = nameService;
            _syntaxNodeFactory = syntaxNodeFactory;
        }

        public IEnumerable<MethodDeclarationSyntax> Create(TypeDeclarationSyntax targetDeclaration, TypeSyntax targetType, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax thisParameter)
        {
            var curParameters = parameters.ToArray();
            var argumentParameter = currentParameter.WithDefault(default);
            var newArgumentParameter = argumentParameter;
            var arguments = new List<ArgumentSyntax>();
            foreach (ParameterSyntax parameter in curParameters)
            {
                if (parameter == currentParameter)
                {
                    arguments.Add(SyntaxFactory.Argument(CreateWithExpression(argumentParameter, out newArgumentParameter)));
                }
                else
                {
                    arguments.Add(_syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
                }
            }

            var name = _nameService.ConvertToName(currentParameter.Identifier.Text);
            yield return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"With{name}")
                .AddParameterListParameters(thisParameter, newArgumentParameter)
                .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, arguments));

            if (argumentParameter != newArgumentParameter && argumentParameter.Type is not ArrayTypeSyntax)
            {
                var args = curParameters.Select(parameter => parameter == currentParameter ? SyntaxFactory.Argument(SyntaxFactory.IdentifierName(currentParameter.Identifier)) : _syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
                yield return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"With{name}")
                    .AddParameterListParameters(thisParameter, argumentParameter)
                    .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, args));
            }

            if (currentParameter.Default != default && argumentParameter == newArgumentParameter)
            {
                var args = curParameters.Select(parameter => parameter == currentParameter ? SyntaxFactory.Argument(currentParameter.Default.Value) : _syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
                yield return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"WithDefault{name}")
                    .AddParameterListParameters(thisParameter)
                    .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, args));
            }
        }
        
        private ExpressionSyntax CreateWithExpression(ParameterSyntax currentParameter, out ParameterSyntax argumentParameter)
        {
            argumentParameter = currentParameter;
            ExpressionSyntax result = SyntaxFactory.IdentifierName(currentParameter.Identifier);
            if (currentParameter.Type == default)
            {
                return result;
            }

            switch (_syntaxNodeFactory.GetUnqualified(currentParameter.Type))
            {
                case GenericNameSyntax genericNameSyntax:
                    if (genericNameSyntax.TypeArgumentList.Arguments.Count == 1 && currentParameter.Type != default)
                    {
                        var elementType = genericNameSyntax.TypeArgumentList.Arguments[0];
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case "List":
                            case "IEnumerable": 
                            case "IReadOnlyCollection":
                            case "IReadOnlyList":
                            case "ICollection":
                            case "IList":
                            {
                                var listType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("System.Collections.Generic.List")).AddTypeArgumentListArguments(elementType);
                                result = SyntaxFactory.ObjectCreationExpression(listType).AddArgumentListArguments(SyntaxFactory.Argument(result));
                                argumentParameter = argumentParameter.WithType(SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier())).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
                                break;
                            }
                                
                            case "ImmutableList":
                            case "IImmutableList":
                            {
                                var listType = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("System.Collections.Immutable.ImmutableList"));
                                result = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            listType,
                                            SyntaxFactory.GenericName(nameof(ImmutableList.Create)).AddTypeArgumentListArguments(elementType)))
                                    .AddArgumentListArguments(SyntaxFactory.Argument(result));
                                argumentParameter = argumentParameter.WithType(SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier())).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
                                break;
                            }
                                
                            case "ImmutableArray":
                            {
                                var listType = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("System.Collections.Immutable.ImmutableArray"));
                                result = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            listType,
                                            SyntaxFactory.GenericName(nameof(ImmutableList.Create)).AddTypeArgumentListArguments(elementType)))
                                    .AddArgumentListArguments(SyntaxFactory.Argument(result));
                                argumentParameter = argumentParameter.WithType(SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier())).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
                                break;
                            }
                        }
                    }

                    break;

                case ArrayTypeSyntax:
                    argumentParameter = argumentParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
                    break;
            }

            return result;
        }
    }
}
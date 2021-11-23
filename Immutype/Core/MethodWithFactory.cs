// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class MethodWithFactory : IMethodFactory
    {
        private readonly INameService _nameService;
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;
        private readonly IDataContainerFactory _dataContainerFactory;

        public MethodWithFactory(
            INameService nameService,
            ISyntaxNodeFactory syntaxNodeFactory,
            IDataContainerFactory dataContainerFactory)
        {
            _nameService = nameService;
            _syntaxNodeFactory = syntaxNodeFactory;
            _dataContainerFactory = dataContainerFactory;
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
            return CreateWithExpression(currentParameter.Type, ref argumentParameter,  SyntaxFactory.IdentifierName(currentParameter.Identifier));
        }

        private ExpressionSyntax CreateWithExpression(TypeSyntax? currentParameterType, ref ParameterSyntax argumentParameter, ExpressionSyntax result)
        {
            if (currentParameterType == default)
            {
                return result;
            }
            
            switch (_syntaxNodeFactory.GetUnqualified(currentParameterType))
            {
                case NullableTypeSyntax nullableTypeSyntax:
                    result = CreateWithExpression(nullableTypeSyntax.ElementType, ref argumentParameter, result);
                    break;

                case GenericNameSyntax genericNameSyntax:
                    _dataContainerFactory.TryCreate(genericNameSyntax, ref result!, ref argumentParameter);
                    break;

                case ArrayTypeSyntax:
                    if (!argumentParameter.Modifiers.Any(i => i.IsKind(SyntaxKind.ParamsKeyword)))
                    {
                        argumentParameter = argumentParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
                    }

                    break;
            }

            return result!;
        }
    }
}
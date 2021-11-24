namespace Immutype.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class MethodAddRemoveFactory : IMethodFactory
    {
        private readonly INameService _nameService;
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;
        private readonly IDataContainerFactory _dataContainerFactory;

        public MethodAddRemoveFactory(
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
            if (currentParameter.Type == default)
            {
                yield break;
            }
            
            var elementType = GetElementType(currentParameter.Type);
            if (elementType == default)
            {
                yield break;
            }

            var arrayType = SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier());
            var arrayParameter = SyntaxFactory.Parameter(currentParameter.Identifier).WithType(arrayType).AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));

            var curParameters = parameters as ParameterSyntax[] ?? parameters.ToArray();
            var name = _nameService.ConvertToName(currentParameter.Identifier.Text);
            yield return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"Add{name}")
                .AddParameterListParameters(thisParameter, arrayParameter)
                .AddBodyStatements(_syntaxNodeFactory.CreateGuards(thisParameter).ToArray())
                .AddBodyStatements(_syntaxNodeFactory.CreateGuards(arrayParameter).ToArray())
                .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, CreateArguments(nameof(Enumerable.Concat), targetDeclaration, thisParameter, curParameters, currentParameter, arrayParameter)));
            
            yield return _syntaxNodeFactory.CreateExtensionMethod(targetType, $"Remove{name}")
                .AddParameterListParameters(thisParameter, arrayParameter)
                .AddBodyStatements(_syntaxNodeFactory.CreateGuards(thisParameter).ToArray())
                .AddBodyStatements(_syntaxNodeFactory.CreateGuards(arrayParameter).ToArray())
                .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, CreateArguments(nameof(Enumerable.Except), targetDeclaration, thisParameter, curParameters, currentParameter, arrayParameter)));
        }

        private TypeSyntax? GetElementType(TypeSyntax typeSyntax) =>
            _syntaxNodeFactory.GetUnqualified(typeSyntax) switch
            {
                NullableTypeSyntax nullableTypeSyntax => GetElementType(nullableTypeSyntax.ElementType),
                GenericNameSyntax genericNameSyntax => genericNameSyntax.TypeArgumentList.Arguments[0],
                ArrayTypeSyntax arrayTypeSyntax => arrayTypeSyntax.ElementType,
                _ => default
            };
        
        private IEnumerable<ArgumentSyntax> CreateArguments(string enumerableMethod, TypeDeclarationSyntax owner, ParameterSyntax thisParameter, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax arrayParameter)
        {
            foreach (ParameterSyntax parameter in parameters)
            {
                if (parameter == currentParameter)
                {
                    yield return SyntaxFactory.Argument(
                        CreateExpression(
                            enumerableMethod,
                            _syntaxNodeFactory.CreateTransientArgumentExpression(owner, thisParameter, currentParameter),
                            currentParameter.Type,
                            arrayParameter));
                }
                else
                {
                    yield return _syntaxNodeFactory.CreateTransientArgument(owner, thisParameter, parameter);
                }
            }
        }

        private ExpressionSyntax CreateExpression(string enumerableMethod, ExpressionSyntax? thisExpression, TypeSyntax? currentParameterType, ParameterSyntax arrayParameter)
        {
            ExpressionSyntax? result = default;
            if (thisExpression != default)
            {
                result = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            thisExpression,
                            SyntaxFactory.IdentifierName(enumerableMethod)))
                    .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(arrayParameter.Identifier)));
            }

            switch (_syntaxNodeFactory.GetUnqualified(currentParameterType))
            {
                case NullableTypeSyntax nullableTypeSyntax:
                    var defaultExpression = CreateExpression(enumerableMethod, default, nullableTypeSyntax.ElementType, arrayParameter);
                    thisExpression = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, thisExpression!, defaultExpression));
                    return CreateExpression(enumerableMethod, thisExpression, nullableTypeSyntax.ElementType, arrayParameter);

                case GenericNameSyntax genericNameSyntax:
                    if (_dataContainerFactory.TryCreate(genericNameSyntax, ref result, ref arrayParameter))
                    {
                        return result!;
                    }

                    break;
                
                case ArrayTypeSyntax arrayTypeSyntax:
                    if (result != default)
                    {
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                result,
                                SyntaxFactory.IdentifierName(nameof(Enumerable.ToArray))));
                    }
                    else
                    {
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.GenericName(nameof(Enumerable.Empty))
                                .AddTypeArgumentListArguments(arrayTypeSyntax.ElementType));
                    }
            }

            return result!;
        }
    }
}
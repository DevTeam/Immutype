// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

internal class MethodAddRemoveFactory(
    INameService nameService,
    ISyntaxNodeFactory syntaxNodeFactory,
    IDataContainerFactory dataContainerFactory,
    ICommentsGenerator commentsGenerator)
    : IMethodFactory
{
    public IEnumerable<MethodDeclarationSyntax> Create(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax targetType, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax thisParameter)
    {
        if (currentParameter.Type == null)
        {
            yield break;
        }

        var curParameters = parameters as ParameterSyntax[] ?? parameters.ToArray();
        var name = nameService.ConvertToName(currentParameter.Identifier.Text);
        var targetDeclaration = context.Syntax;
        
        var elementType = GetElementType(currentParameter.Type);
        if (elementType == null)
        {
            yield break;
        }
        
        var arrayType = SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier());
        var arrayParameter = SyntaxRepo.Parameter(currentParameter.Identifier).WithType(arrayType).AddModifiers(SyntaxKind.ParamsKeyword.WithSpace());

        var addArgs = CreateArguments(nameof(Enumerable.Concat), targetDeclaration, thisParameter, curParameters, currentParameter, arrayParameter);
        if (addArgs.Any())
        {
            yield return commentsGenerator.AddComments(
                context,
                $"Add <c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be added to the copy of the instance.",
                syntaxNodeFactory.CreateExtensionMethod(targetType, $"Add{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, arrayParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, thisParameter, !syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, arrayParameter, false).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateReturnStatement(targetType, addArgs)));
        }

        var removeArgs = CreateArguments(nameof(Enumerable.Except), targetDeclaration, thisParameter, curParameters, currentParameter, arrayParameter);
        if (removeArgs.Any())
        {
            yield return commentsGenerator.AddComments(
                context,
                $"Remove <c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be removed from the instance copy.",
                syntaxNodeFactory.CreateExtensionMethod(targetType, $"Remove{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, arrayParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, thisParameter, !syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, arrayParameter, false).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateReturnStatement(targetType, removeArgs)));
        }
        
        if (currentParameter.Type is ArrayTypeSyntax)
        {
            yield break;
        }

        addArgs = CreateArguments(nameof(Enumerable.Concat), targetDeclaration, thisParameter, curParameters, currentParameter, currentParameter);
        if (addArgs.Any())
        {
            yield return commentsGenerator.AddComments(
                context,
                $"Add <c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be added to the copy of the instance.",
                syntaxNodeFactory.CreateExtensionMethod(targetType, $"Add{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, currentParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, thisParameter, !syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, currentParameter, false).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateReturnStatement(targetType, addArgs)));
        }

        removeArgs = CreateArguments(nameof(Enumerable.Except), targetDeclaration, thisParameter, curParameters, currentParameter, currentParameter);
        if (removeArgs.Any())
        {
            yield return commentsGenerator.AddComments(
                context,
                $"Remove <c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be removed from the instance copy.",
                syntaxNodeFactory.CreateExtensionMethod(targetType, $"Remove{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, currentParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, thisParameter, !syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateGuards(context, currentParameter, false).ToArray())
                    .AddBodyStatements(syntaxNodeFactory.CreateReturnStatement(targetType, removeArgs)));
        }
    }

    private TypeSyntax? GetElementType(TypeSyntax typeSyntax) =>
        syntaxNodeFactory.GetUnqualified(typeSyntax) switch
        {
            NullableTypeSyntax nullableTypeSyntax => GetElementType(nullableTypeSyntax.ElementType),
            GenericNameSyntax genericNameSyntax => genericNameSyntax.TypeArgumentList.Arguments[0],
            ArrayTypeSyntax arrayTypeSyntax => arrayTypeSyntax.ElementType,
            _ => null
        };

    private IReadOnlyCollection<ArgumentSyntax> CreateArguments(string enumerableMethod, TypeDeclarationSyntax owner, ParameterSyntax thisParameter, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax arrayParameter)
    {
        var args = new List<ArgumentSyntax>();
        foreach (var parameter in parameters)
        {
            if (parameter == currentParameter)
            {
                var thisArg = syntaxNodeFactory.CreateTransientArgumentExpression(owner, thisParameter, currentParameter);

                var expression = TryCreateExpression(
                    enumerableMethod,
                    thisArg,
                    currentParameter.Type,
                    arrayParameter);

                if (expression == null)
                {
                    return Array.Empty<ArgumentSyntax>();
                }

                args.Add(SyntaxFactory.Argument(expression));
            }
            else
            {
                args.Add(syntaxNodeFactory.CreateTransientArgument(owner, thisParameter, parameter));
            }
        }

        return args;
    }

    private ExpressionSyntax? TryCreateExpression(string enumerableMethod, ExpressionSyntax? thisExpression, TypeSyntax? currentParameterType, ParameterSyntax arrayParameter, bool addCheck = true)
    {
        ExpressionSyntax? result = null;
        if (thisExpression != null)
        {
            ExpressionSyntax valExpression = SyntaxFactory.IdentifierName(arrayParameter.Identifier);
            if (addCheck)
            {
                var defaultExpression = TryCreateExpression(enumerableMethod, null, currentParameterType, arrayParameter);
                if (defaultExpression == null)
                {
                    return null;
                }

                thisExpression = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.ConditionalExpression(
                    SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, thisExpression, SyntaxFactory.DefaultExpression(currentParameterType!)),
                    defaultExpression,
                    thisExpression));
            }

            if (arrayParameter.Type is NullableTypeSyntax nullableTypeSyntax)
            {
                var defaultExpression = TryCreateExpression(enumerableMethod, null, nullableTypeSyntax.ElementType, arrayParameter);
                if (defaultExpression == null)
                {
                    return null;
                }
                
                valExpression = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.ConditionalExpression(
                    SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, valExpression, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                    defaultExpression,
                    valExpression));
            }

            result = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        thisExpression,
                        SyntaxFactory.IdentifierName(enumerableMethod)))
                .AddArgumentListArguments(SyntaxFactory.Argument(valExpression));
        }

        switch (syntaxNodeFactory.GetUnqualified(currentParameterType))
        {
            case NullableTypeSyntax nullableTypeSyntax:
                // ReSharper disable once TailRecursiveCall
                return TryCreateExpression(enumerableMethod, thisExpression, nullableTypeSyntax.ElementType, arrayParameter, false);

            case GenericNameSyntax genericNameSyntax:
                if (dataContainerFactory.TryCreate(genericNameSyntax, ref result, ref arrayParameter))
                {
                    return result!;
                }

                break;

            case ArrayTypeSyntax arrayTypeSyntax:
                if (result != null)
                {
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            result,
                            SyntaxFactory.IdentifierName(nameof(Enumerable.ToArray))));
                }

                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("System.Array"),
                        SyntaxFactory.GenericName(nameof(Enumerable.Empty))
                            .AddTypeArgumentListArguments(arrayTypeSyntax.ElementType)));
        }

        return result!;
    }
}
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf
namespace Immutype.Core;

internal class MethodWithFactory : IMethodFactory
{
    private readonly INameService _nameService;
    private readonly ISyntaxNodeFactory _syntaxNodeFactory;
    private readonly IDataContainerFactory _dataContainerFactory;
    private readonly ICommentsGenerator _commentsGenerator;

    public MethodWithFactory(
        INameService nameService,
        ISyntaxNodeFactory syntaxNodeFactory,
        IDataContainerFactory dataContainerFactory,
        ICommentsGenerator commentsGenerator)
    {
        _nameService = nameService;
        _syntaxNodeFactory = syntaxNodeFactory;
        _dataContainerFactory = dataContainerFactory;
        _commentsGenerator = commentsGenerator;
    }

    public IEnumerable<MethodDeclarationSyntax> Create(GenerationContext<TypeDeclarationSyntax> context, TypeSyntax targetType, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax thisParameter)
    {
        var curParameters = parameters.ToArray();
        var argumentParameter = currentParameter.WithDefault(default);
        var newArgumentParameter = argumentParameter;
        var targetDeclaration = context.Syntax;
        var arguments = new List<ArgumentSyntax>();
        var newArguments = new List<ArgumentSyntax>();
        foreach (var parameter in curParameters)
        {
            if (parameter == currentParameter)
            {
                arguments.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(argumentParameter.Identifier.Text)));
                newArguments.Add(SyntaxFactory.Argument(CreateWithExpression(argumentParameter, out newArgumentParameter)));
            }
            else
            {
                arguments.Add(_syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
                newArguments.Add(_syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
            }
        }
        
        var name = _nameService.ConvertToName(currentParameter.Identifier.Text);
        var isArrayParam = argumentParameter.Type is ArrayTypeSyntax;
        var isCollectionParam = !AreEqu(argumentParameter, newArgumentParameter) || isArrayParam;
        if (isArrayParam && argumentParameter.Modifiers.All(i => i.Kind() != SyntaxKind.ParamsKeyword))
        {
            argumentParameter = argumentParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
        }
        
        var variants = new List<ParameterSyntax>();
        if (!isCollectionParam || argumentParameter.Type is not NullableTypeSyntax)
        {
            yield return _commentsGenerator.AddComments(
                $"Set <c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be changed in the copy of the instance.",
                _syntaxNodeFactory.CreateExtensionMethod(targetType, $"With{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, argumentParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(_syntaxNodeFactory.CreateGuards(context, thisParameter, !_syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(_syntaxNodeFactory.CreateGuards(context, argumentParameter, false).ToArray())
                    .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, arguments)));
        }

        if (isCollectionParam && !isArrayParam)
        {
            yield return _commentsGenerator.AddComments(
                $"Set <c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be changed in the copy of the instance.",
                _syntaxNodeFactory.CreateExtensionMethod(targetType, $"With{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter, newArgumentParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(_syntaxNodeFactory.CreateGuards(context, thisParameter, !_syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(_syntaxNodeFactory.CreateGuards(context, newArgumentParameter, false).ToArray())
                    .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, newArguments)));
        }
        
        if (isCollectionParam)
        {
            yield return _commentsGenerator.AddComments(
                $"Clear <c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                $"<c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c> to be changed in the copy of the instance.",
                _syntaxNodeFactory.CreateExtensionMethod(targetType, $"Clear{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(thisParameter.Identifier.Text),
                                    SyntaxFactory.IdentifierName($"With{name}" + targetDeclaration.TypeParameterList))))));
        }
        
        if (currentParameter.Default != default)
        {
            var args = curParameters.Select(parameter => parameter == currentParameter ? SyntaxFactory.Argument(currentParameter.Default.Value) : _syntaxNodeFactory.CreateTransientArgument(targetDeclaration, thisParameter, parameter));
            yield return _commentsGenerator.AddComments(
                $"Set a default for <c>{_nameService.ConvertToName(currentParameter.Identifier.Text)}</c>.",
                currentParameter,
                "",
                _syntaxNodeFactory.CreateExtensionMethod(targetType, $"WithDefault{name}" + targetDeclaration.TypeParameterList)
                    .AddParameterListParameters(thisParameter)
                    .WithConstraintClauses(targetDeclaration.ConstraintClauses)
                    .AddBodyStatements(_syntaxNodeFactory.CreateGuards(context, thisParameter, !_syntaxNodeFactory.IsValueType(context.Syntax)).ToArray())
                    .AddBodyStatements(_syntaxNodeFactory.CreateReturnStatement(targetType, args)));
        }
    }

    private static bool AreEqu(BaseParameterSyntax argumentParameter, BaseParameterSyntax newArgumentParameter)
    {
        if (Equals(argumentParameter.Type, newArgumentParameter.Type))
        {
            return true;
        }

        if (argumentParameter.Type == default || newArgumentParameter.Type == default)
        {
            return false;
        }

        return argumentParameter.Type.IsEquivalentTo(newArgumentParameter.Type);
    }

    private ExpressionSyntax CreateWithExpression(ParameterSyntax currentParameter, out ParameterSyntax argumentParameter)
    {
        argumentParameter = currentParameter;
        return CreateWithExpression(currentParameter.Type, ref argumentParameter, SyntaxFactory.IdentifierName(currentParameter.Identifier));
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

        return result;
    }
}
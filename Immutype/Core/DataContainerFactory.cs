// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InvertIf
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class DataContainerFactory : IDataContainerFactory
    {
        private static readonly Dictionary<string, string> GenericTypeMap = new()
        {
            {"List", "List"},
            {"IEnumerable", "List"}, 
            {"IReadOnlyCollection", "List"},
            {"IReadOnlyList", "List"},
            {"ICollection", "List"},
            {"IList", "List"},
            {"HashSet", "HashSet"},
            {"ISet", "HashSet"},
            {"Queue", "Queue"},
            {"Stack", "Stack"}
        };
        
        private static readonly Dictionary<string, string> ReadonlyTypeMap = new()
        {
            {"IReadOnlyCollection", "List"},
            {"IReadOnlyList", "List"},
            {"IReadOnlySet", "List"}
        };

        private static readonly Dictionary<string, string> ImmutableTypeMap = new()
        {
            {"ImmutableList", "ImmutableList"},
            {"IImmutableList", "ImmutableList"}, 
            {"ImmutableArray", "ImmutableArray"},
            {"ImmutableQueue", "ImmutableQueue"},
            {"IImmutableQueue", "ImmutableQueue"},
            {"ImmutableStack", "ImmutableStack"},
            {"IImmutableStack", "ImmutableStack"}
        };

        public bool TryCreate(GenericNameSyntax genericNameSyntax, ref ExpressionSyntax? expressionSyntax, ref ParameterSyntax argumentParameter)
        {
            if (genericNameSyntax.TypeArgumentList.Arguments.Count != 1)
            {
                return false;
            }
            
            var elementType = genericNameSyntax.TypeArgumentList.Arguments[0];
            var newArgumentParameter = argumentParameter.WithType(
                SyntaxFactory.ArrayType(elementType).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
            
            if (GenericTypeMap.TryGetValue(genericNameSyntax.Identifier.Text, out var genericTypeName))
            {
                expressionSyntax = CreateGenericContainer(expressionSyntax, genericTypeName, elementType);
                argumentParameter = newArgumentParameter;
                return true;
            }
            
            if (ReadonlyTypeMap.TryGetValue(genericNameSyntax.Identifier.Text, out genericTypeName))
            {
                expressionSyntax = CreateGenericContainer(expressionSyntax, genericTypeName, elementType);
                expressionSyntax = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expressionSyntax,
                        SyntaxFactory.IdentifierName(nameof(List<object>.AsReadOnly))));
                argumentParameter = newArgumentParameter;
                return true;
            }

            if (ImmutableTypeMap.TryGetValue(genericNameSyntax.Identifier.Text, out var immutableTypeName))
            {
                var immutableDataTypeName = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier($"System.Collections.Immutable.{immutableTypeName}"));
                if (expressionSyntax != default)
                {
                    expressionSyntax = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            immutableDataTypeName,
                            SyntaxFactory.GenericName("CreateRange").AddTypeArgumentListArguments(elementType)))
                        .AddArgumentListArguments(SyntaxFactory.Argument(expressionSyntax));
                }
                else
                {
                    expressionSyntax = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.GenericName(immutableTypeName).AddTypeArgumentListArguments(elementType),
                        SyntaxFactory.IdentifierName("Empty"));
                }

                argumentParameter = newArgumentParameter;
                return true;
            }

            return false;
        }

        private static ExpressionSyntax CreateGenericContainer(ExpressionSyntax? expressionSyntax, string genericTypeName, TypeSyntax elementType)
        {
            var genericDataType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"System.Collections.Generic.{genericTypeName}"))
                .AddTypeArgumentListArguments(elementType);

            var result = SyntaxFactory.ObjectCreationExpression(genericDataType);
            return expressionSyntax != default
                ? result.AddArgumentListArguments(SyntaxFactory.Argument(expressionSyntax))
                : result.AddArgumentListArguments();
        }
    }
}
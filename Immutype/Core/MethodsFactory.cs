// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class MethodsFactory : IMethodsFactory
    {
        private readonly ImmutableArray<IMethodFactory> _methodFactories;
        private readonly ISyntaxNodeFactory _syntaxNodeFactory;

        public MethodsFactory(ImmutableArray<IMethodFactory> methodFactories, ISyntaxNodeFactory syntaxNodeFactory)
        {
            _methodFactories = methodFactories;
            _syntaxNodeFactory = syntaxNodeFactory;
        }

        public IEnumerable<MemberDeclarationSyntax> Create(ParseOptions parseOptions, TypeDeclarationSyntax targetDeclaration, TypeSyntax targetType, IReadOnlyList<ParameterSyntax> parameters, CancellationToken cancellationToken)
        {
            var thisParameter = 
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("it"))
                .WithType(targetType)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword));

            if (parseOptions is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp7_2 } && _syntaxNodeFactory.IsValueType(targetDeclaration))
            {
                thisParameter = thisParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
            }

            return
                from currentParameter in parameters
                let parameterType = currentParameter.Type
                where parameterType != default
                from methodFactory in _methodFactories
                from method in methodFactory.Create(targetDeclaration, targetType, parameters, currentParameter, thisParameter)
                where !cancellationToken.IsCancellationRequested
                select method;
        }
    }
}
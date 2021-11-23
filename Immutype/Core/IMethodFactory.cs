namespace Immutype.Core
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IMethodFactory
    {
        IEnumerable<MethodDeclarationSyntax> Create(TypeDeclarationSyntax targetDeclaration, TypeSyntax targetType, IEnumerable<ParameterSyntax> parameters, ParameterSyntax currentParameter, ParameterSyntax thisParameter);
    }
}
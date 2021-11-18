namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IMethodsFactory
    {
        IEnumerable<MemberDeclarationSyntax> Create(TypeDeclarationSyntax targetDeclaration, TypeSyntax targetType, IReadOnlyList<ParameterSyntax> parameters, CancellationToken cancellationToken);
    }
}
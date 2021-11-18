namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IUnitFactory
    {
        IEnumerable<Source> Create(TypeDeclarationSyntax typeDeclarationSyntax, IReadOnlyList<ParameterSyntax> parameters, CancellationToken cancellationToken);
    }
}
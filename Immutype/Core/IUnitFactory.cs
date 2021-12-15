namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IUnitFactory
    {
        IEnumerable<Source> Create(GenerationContext<TypeDeclarationSyntax> context, IReadOnlyList<ParameterSyntax> parameters);
    }
}
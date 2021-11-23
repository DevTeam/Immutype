namespace Immutype.Core
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IDataContainerFactory
    {
        bool TryCreate(GenericNameSyntax genericNameSyntax, ref ExpressionSyntax? expressionSyntax, ref ParameterSyntax argumentParameter);
    }
}
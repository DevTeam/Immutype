namespace Immutype.Core;

internal interface IDataContainerFactory
{
    bool TryCreate(GenericNameSyntax genericNameSyntax, ref ExpressionSyntax? expressionSyntax, ref ParameterSyntax argumentParameter);
}
namespace Immutype.Core;

internal interface IDataContainerFactory
{
    CollectionType? GetCollection(TypeSyntax type);
}

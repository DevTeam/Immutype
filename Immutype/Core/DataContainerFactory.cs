namespace Immutype.Core;

internal sealed class DataContainerFactory : IDataContainerFactory
{
    public CollectionType? GetCollection(TypeSyntax type)
    {
        if (type is NullableTypeSyntax nullable)
        {
            type = nullable.ElementType;
        }

        switch (type)
        {
            case ArrayTypeSyntax { RankSpecifiers.Count: 1 } array when array.RankSpecifiers[0].Rank == 1:
                return new CollectionType(array.ElementType.ToString(), null, false);

            case QualifiedNameSyntax qualified:
                type = qualified.Right;
                break;
        }

        if (type is not GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } generic)
        {
            return null;
        }

        var element = generic.TypeArgumentList.Arguments[0].ToString();
        return generic.Identifier.ValueText switch
        {
            "List" or "IEnumerable" or "IReadOnlyCollection" or "IReadOnlyList" or "ICollection" or "IList" =>
                new CollectionType(element, "List", false),
            "HashSet" or "ISet" => new CollectionType(element, "HashSet", false),
            "Queue" or "Stack" => new CollectionType(element, generic.Identifier.ValueText, false),
            "ImmutableList" or "IImmutableList" => new CollectionType(element, "ImmutableList", true),
            "ImmutableArray" => new CollectionType(element, "ImmutableArray", true),
            "ImmutableQueue" or "IImmutableQueue" => new CollectionType(element, "ImmutableQueue", true),
            "ImmutableStack" or "IImmutableStack" => new CollectionType(element, "ImmutableStack", true),
            _ => null
        };
    }
}

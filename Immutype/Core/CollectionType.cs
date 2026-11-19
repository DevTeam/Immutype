namespace Immutype.Core;

internal sealed record CollectionType(string ElementType, string? Container, bool IsImmutable)
{
    public string ParamsType => $"{ElementType}[]";

    public bool IsArray => Container == null;

    public string Materialize(string items) =>
        Container == null
            ? $"global::System.Linq.Enumerable.ToArray({items})"
            : IsImmutable
                ? $"global::System.Collections.Immutable.{Container}.CreateRange<{ElementType}>({items})"
                : $"new global::System.Collections.Generic.{Container}<{ElementType}>({items})";

    public string OrEmpty(string items, bool nullable)
    {
        if (Container != "ImmutableArray")
        {
            return $"({items} ?? global::System.Linq.Enumerable.Empty<{ElementType}>())";
        }

        var value = nullable ? $"{items}.GetValueOrDefault()" : items;
        return $"({value}.IsDefault ? global::System.Collections.Immutable.ImmutableArray<{ElementType}>.Empty : {value})";
    }
}

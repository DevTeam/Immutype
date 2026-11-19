namespace Immutype.Core;

internal sealed class MethodAddRemoveFactory(IMethodWriter writer) : IMethodFactory
{
    public void Build(MethodContext context)
    {
        if (context.Collection is not { } collection)
        {
            return;
        }

        var isNullable = context.Parameter.Type is NullableTypeSyntax;
        var currentItems = collection.OrEmpty(context.MemberAccess, isNullable);
        var paramsType = collection.ParamsType;
        WriteOverloads($"params {paramsType} {context.ArgumentName}", context.ArgumentName, paramsType);

        if (collection.IsArray)
        {
            return;
        }

        var items = isNullable ? collection.OrEmpty(context.ArgumentName, true) : context.ArgumentName;
        WriteOverloads(context.Parameter.ToString(), items, context.ArgumentGuardType);
        return;

        void WriteOverloads(string argument, string items, string? guardType)
        {
            var name = context.Name;
            var concatenated = $"global::System.Linq.Enumerable.Concat({currentItems}, {items})";
            var except = $"global::System.Linq.Enumerable.Except({currentItems}, {items})";
            writer.Write(context, $"Add{name}", $"Add <c>{name}</c>.",
                context.CreateCopy(collection.Materialize(concatenated)), argument,
                $"<c>{name}</c> to be added to the copy of the instance.", guardType);
            writer.Write(context, $"Remove{name}", $"Remove <c>{name}</c>.",
                context.CreateCopy(collection.Materialize(except)), argument,
                $"<c>{name}</c> to be removed from the instance copy.", guardType);
        }
    }
}

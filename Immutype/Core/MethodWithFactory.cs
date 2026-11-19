namespace Immutype.Core;

internal sealed class MethodWithFactory(IMethodWriter writer) : IMethodFactory
{
    public void Build(MethodContext context)
    {
        var parameter = context.Parameter;
        var collection = context.Collection;
        var name = context.Name;
        var withMethod = $"With{name}";
        var title = $"Set <c>{name}</c>.";
        var argumentComment = $"<c>{name}</c> to be changed in the copy of the instance.";

        if (collection == null || collection is { IsArray: false } && parameter.Type is not NullableTypeSyntax)
        {
            writer.Write(context, withMethod, title, context.CreateCopy(context.ArgumentName),
                parameter.WithDefault(null).ToString(), argumentComment, context.ArgumentGuardType);
        }

        if (collection != null)
        {
            var paramsType = collection.ParamsType;
            var value = collection.IsArray ? context.ArgumentName : collection.Materialize(context.ArgumentName);
            var attributes = string.Join(" ", parameter.AttributeLists);
            var declaration = $"params {paramsType} {context.ArgumentName}";
            if (attributes.Length > 0)
            {
                declaration = $"{attributes} {declaration}";
            }

            writer.Write(context, withMethod, title, context.CreateCopy(value),
                declaration, argumentComment, paramsType);
            writer.Write(context, $"Clear{name}", $"Clear <c>{name}</c>.",
                $"it.{withMethod}{context.Extensions.TypeParameters}()", guardReceiver: false);
        }

        if (parameter.Default is { } defaultValue)
        {
            writer.Write(context, $"WithDefault{name}", $"Set a default for <c>{name}</c>.",
                context.CreateCopy(defaultValue.Value.ToString()));
        }
    }
}

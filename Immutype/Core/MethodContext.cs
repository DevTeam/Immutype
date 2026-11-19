namespace Immutype.Core;

internal sealed record ExtensionMethods(
    LineBuilder Code,
    GenerationContext<TypeDeclarationSyntax> Generation,
    string TargetType,
    string TypeParameters,
    string Receiver,
    bool GuardReceiver,
    string[] MemberArguments);

internal sealed record MethodContext(
    ExtensionMethods Extensions,
    ParameterSyntax Parameter,
    int Index,
    string Name,
    CollectionType? Collection,
    string? ArgumentGuardType)
{
    public string ArgumentName => Parameter.Identifier.Text;

    public string MemberAccess => Extensions.MemberArguments[Index];

    public string CreateCopy(string replacement)
    {
        var arguments = Extensions.MemberArguments.Select((argument, index) => index == Index ? replacement : argument);
        return $"new {Extensions.TargetType}({string.Join(", ", arguments)})";
    }
}

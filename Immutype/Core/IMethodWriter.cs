namespace Immutype.Core;

internal interface IMethodWriter
{
    void Write(
        MethodContext context,
        string name,
        string title,
        string returnValue,
        string? argument = null,
        string? argumentComment = null,
        string? argumentGuardType = null,
        bool guardReceiver = true);
}

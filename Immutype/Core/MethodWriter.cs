namespace Immutype.Core;

using System.Runtime.CompilerServices;

internal sealed class MethodWriter(ICommentsGenerator commentsGenerator) : IMethodWriter
{
    public void Write(
        MethodContext context,
        string name,
        string title,
        string returnValue,
        string? argument = null,
        string? argumentComment = null,
        string? argumentGuardType = null,
        bool guardReceiver = true)
    {
        // ReSharper disable once UseDeconstruction
        var extensions = context.Extensions;
        var code = extensions.Code;
        code.AppendLine();
        commentsGenerator.Write(code, extensions.Generation, title, context.Parameter, argumentComment);
        const int inlining = (int)MethodImplOptions.AggressiveInlining;
        code.AppendLine($"[global::System.Runtime.CompilerServices.MethodImplAttribute((global::System.Runtime.CompilerServices.MethodImplOptions){inlining})]");
        code.AppendLine("[global::System.Diagnostics.Contracts.PureAttribute]");
        var arguments = argument == null ? extensions.Receiver : $"{extensions.Receiver}, {argument}";
        code.AppendLine($"public static {extensions.TargetType} {name}{extensions.TypeParameters}({arguments})");
        foreach (var constraint in extensions.Generation.Syntax.ConstraintClauses)
        {
            code.AppendLine($"    {constraint}");
        }

        using (code.Block())
        {
            if (guardReceiver && extensions.GuardReceiver)
            {
                WriteGuard(code, "it", extensions.TargetType);
            }

            if (argumentGuardType != null)
            {
                WriteGuard(code, context.ArgumentName, argumentGuardType);
            }

            code.AppendLine($"return {returnValue};");
        }
    }

    private static void WriteGuard(LineBuilder code, string name, string type)
    {
        code.AppendLine($"if ({name} == default({type}))");
        using (code.Block())
        {
            code.AppendLine($"throw new global::System.ArgumentNullException({SymbolDisplay.FormatLiteral(name, true)});");
        }

        code.AppendLine();
    }
}

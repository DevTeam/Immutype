
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global

namespace Immutype.Core;

internal readonly struct Source(
    string hintName,
    SourceText code)
{
    public readonly string HintName = hintName;
    public readonly SourceText Code = code;
}
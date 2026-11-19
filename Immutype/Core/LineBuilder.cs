namespace Immutype.Core;

internal sealed class LineBuilder
{
    private readonly StringBuilder _text = new();
    private int _indent;

    public void AppendLine(string line = "")
    {
        if (line.Length > 0)
        {
            _text.Append(' ', _indent * 4);
        }

        _text.AppendLine(line);
    }

    public BlockScope Block()
    {
        AppendLine("{");
        _indent++;
        return new BlockScope(this);
    }

    public override string ToString() => _text.ToString();

    public readonly struct BlockScope(LineBuilder code) : IDisposable
    {
        public void Dispose()
        {
            code._indent--;
            code.AppendLine("}");
        }
    }
}

// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype.Core;

using System.Globalization;

internal class NameService : INameService
{
    public string ConvertToName(string name) =>
        new([.. ConvertToName((IEnumerable<char>)name)]);

    private static IEnumerable<char> ConvertToName(IEnumerable<char> ch)
    {
        var isFirst = true;
        foreach (var c in ch)
        {
            if (isFirst)
            {
                yield return char.ToUpper(c, CultureInfo.InvariantCulture);
                isFirst = false;
            }
            else
            {
                yield return c;
            }
        }
    }
}
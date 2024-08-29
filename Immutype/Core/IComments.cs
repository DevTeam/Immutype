namespace Immutype.Core;

internal interface IComments
{
    ImmutableDictionary<string, string> GetParamsComments(IEnumerable<SyntaxTrivia> comments);
}
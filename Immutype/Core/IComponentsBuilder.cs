// ReSharper disable UnusedMember.Global
namespace Immutype.Core;

internal interface IComponentsBuilder
{
    IEnumerable<Source> Build(CancellationToken cancellationToken);
}
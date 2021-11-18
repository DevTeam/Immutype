namespace Immutype.Core
{
    using System.Collections.Generic;
    using System.Threading;

    internal interface IComponentsBuilder
    {
        IEnumerable<Source> Build(CancellationToken cancellationToken);
    }
}
 #if !IMMUTYPE_API_SUPPRESSION
#pragma warning disable
 
namespace Immutype
{
    using System;
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor, Inherited = false)]
    public class TargetAttribute: Attribute { }
}

#pragma warning restore
#endif

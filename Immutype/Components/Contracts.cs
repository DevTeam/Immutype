// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
namespace Immutype
{
    using System;
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class TargetAttribute: Attribute { }
}
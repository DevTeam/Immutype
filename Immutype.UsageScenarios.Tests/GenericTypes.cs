// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ArrangeNamespaceBody
namespace Immutype.UsageScenarios.Tests.GenericTypes
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=03
    // $description=Generic types
    // $header=It is possible to use generic types including any generic constraints.
    // {
    [Immutype.Target]
    internal record Person<TAge>(string Name, TAge Age = default, IEnumerable<Person<TAge>>? Friends = null) 
        where TAge : struct;

    public class GenericTypes
    { 
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person<int>("John")
                .WithAge(15)
                .WithFriends(new Person<int>("David").WithAge(16))
                .AddFriends(
                    new Person<int>("James"),
                    new Person<int>("Daniel").WithAge(17));
            
            john.Friends?.Count().ShouldBe(3);
        }
    }
    // }
}
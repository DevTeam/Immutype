// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
namespace Immutype.UsageScenarios.Tests.Set
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=03
    // $description=Set
    // {
    [Immutype.Target]
    internal record Person(
        string Name,
        int Age = 0,
        ISet<Person>? Friends = default);

    public class Set
    {
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person("John",15)
                .WithFriends(
                    new Person("David").WithAge(16),
                    new Person("David").WithAge(16),
                    new Person("James").WithAge(17)
                        .WithFriends(new Person("Tyler").WithAge(16)));
            
            john.Friends?.Count.ShouldBe(2);
        }
    }
    // }
}
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
namespace Immutype.UsageScenarios.Tests.ImmutableCollection
{
    using System.Collections.Immutable;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=02
    // $description=Immutable collection
    // {
    [Immutype.Target]
    internal readonly struct Person
    {
        public readonly string Name;
        public readonly int Age;
        public readonly IImmutableList<Person> Friends;

        public Person(
            string name,
            int age = 0,
            IImmutableList<Person>? friends = default)
        {
            Name = name;
            Age = age;
            Friends = friends ?? ImmutableList<Person>.Empty;
        }
    };

    public class ImmutableCollection
    {
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person("John",15)
                .WithFriends(
                    new Person("David").WithAge(16),
                    new Person("James").WithAge(17))
                .AddFriends(
                    new Person("David").WithAge(22));
            
            john.Friends.Count.ShouldBe(3);
        }
    }
    // }
}
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable DefaultStructEqualityIsUsed.Global
namespace Immutype.UsageScenarios.Tests.ExplicitConstructorChoice
{
    using System.Collections.Immutable;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=06
    // $description=Explicit constructor choice
    // {
    [Immutype.Target]
    internal readonly struct Person
    {
        public readonly string Name;
        public readonly int Age;
        public readonly IImmutableList<Person> Friends;

        // You can explicitly select a constructor by marking it with the [Immutype.Target] attribute
        [Immutype.Target]
        public Person(
            string name,
            int age = 0,
            IImmutableList<Person>? friends = null)
        {
            Name = name;
            Age = age;
            Friends = friends ?? ImmutableList<Person>.Empty;
        }
        
        public Person(
            string name,
            int age,
            IImmutableList<Person>? friends,
            int someArg = 99)
        {
            Name = name;
            Age = age;
            Friends = friends ?? ImmutableList<Person>.Empty;
        }
    }

    public class ExplicitConstructorChoice
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
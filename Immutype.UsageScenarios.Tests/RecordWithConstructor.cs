// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ArrangeNamespaceBody
namespace Immutype.UsageScenarios.Tests.RecordWithConstructor
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=05
    // $description=Record with constructor
    // {
    [Immutype.Target]
    internal record Person
    {
        public Person(
            string name,
            int? age = null,
            ICollection<Person>? friends = null)
        {
            Name = name;
            Age = age;
            Friends = friends;
        }

        public string Name { get; }

        public int? Age { get; }

        public ICollection<Person>? Friends { get; }

        public void Deconstruct(
            out string name,
            out int? age,
            out ICollection<Person>? friends)
        {
            name = Name;
            age = Age;
            friends = Friends;
        }
    }

    public class RecordWithConstructor
    {
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person("John",15)
                .WithFriends(
                    new Person("David").WithAge(16),
                    new Person("James").WithAge(17)
                        .WithFriends(new Person("Tyler").WithAge(16)));
            
            john.Friends?.Count.ShouldBe(2);
        }
    }
    // }
}
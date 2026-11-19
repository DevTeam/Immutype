namespace Immutype.UsageScenarios.Tests.Unions
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=07
    // $description=Union types
    // $header=Mark a union with `[Immutype.Target]` to generate a `WithValue` overload for each case type. A union can also be a record parameter, where the usual `With` method accepts its case types through C# union conversions.
    // {
    [Immutype.Target]
    internal union Contact(string, long);

    [Immutype.Target]
    internal record Person(string Name, Contact PreferredContact);

    public class Union
    {
    // }
        [Fact]
    // {
        public void Run()
        {
            Contact email = "alice@example.com";
            var phone = email.WithValue(123456789L);
            var alice = new Person("Alice", email);
            var updated = alice.WithPreferredContact(123456789L);

            (email is "alice@example.com").ShouldBeTrue();
            (phone is 123456789L).ShouldBeTrue();
            (phone.WithValue("alice@example.com") is "alice@example.com").ShouldBeTrue();
            (alice.PreferredContact is "alice@example.com").ShouldBeTrue();
            (updated.PreferredContact is 123456789L).ShouldBeTrue();
            updated.Name.ShouldBe("Alice");
        }
    // }

        [Fact]
        public void ShouldReplaceGenericAndNullableCollectionCases()
        {
            var original = default(Choice<int, int>);
            var number = original.WithValue(42);
            var items = new List<int> { 1, 2 };
            var collection = number.WithValue(items);
            var empty = collection.WithValue(null);

            (original is null).ShouldBeTrue();
            (number is 42).ShouldBeTrue();
            collection.Value.ShouldBeSameAs(items);
            (empty is null).ShouldBeTrue();
        }
    // {
    }
    // }

#pragma warning disable CS8981 // Exercise type parameter names that collide with generated parameter names.
    [Immutype.Target]
    public union Choice<it, value>(it, List<value>?) where it : struct;
#pragma warning restore CS8981
}

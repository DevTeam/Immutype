// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
namespace Immutype.UsageScenarios.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=01
    // $description=Simple parameters
    // {
    [Immutype.Target]
    public record PersonWithContacts(string FirstName, string SecondName, IEnumerable<string> PhoneNumbers)
    {
        // Additional concise constructor
        public PersonWithContacts(string FirstName):
            this(FirstName, string.Empty, Enumerable.Empty<string>())
        {
        }
    }

    public class EnumerableParameter
    {
        [Fact]
        public void Run()
        {
            var person = new PersonWithContacts("John").WithSecondName("Smith").AddPhoneNumbers("+7931123556", "+7931123557");
            person.SecondName.ShouldBe("Smith");
            person.PhoneNumbers.ShouldBe(new []{"+7931123556", "+7931123557"});
        }
    }
    // }
}

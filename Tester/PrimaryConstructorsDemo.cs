using ICSharpCode.Decompiler.CSharp;
using Spectre.Console;

namespace Tester;

[Order(1)]
internal class PrimaryConstructors(DemoRunner demo) : Runnable
{
    public override void Run()
    {
        demo.Dump(new Person("Mike", "Oldfield", new("UK", "London")));

        demo.HighlightDecompiledCSharp(typeof(Person), [LanguageVersion.CSharp10_0, LanguageVersion.CSharp12_0]);
    }
}

class Person(string name/*0. not exposed outside*/, string familyName, Address address)
{
    public string Name => name; //1. Property 1
    public string FamilyName { get; } = familyName; //2. Property 2
    public Address Address { get; } = address;
    //public readonly string name = name; //3. capture

    public void Greet() => AnsiConsole.WriteLine($"Hello, {name}!");
}

class Address(string country, string city)
{
    public string Country { get; } = country;
    public string City { get; } = city;
}

class Developer(string name, string familyName, float salary)
   : Person(name, familyName, new Address("", ""))
{
    public float Salary { get; } = salary;

    /*public Developer()
    {
        var temp = salary; //CS9105  Cannot use primary constructor parameter 'float salary' in this context.
    }*/

    /*public void DoSomething()
    {
        var temp = salary; //CS9124  Parameter 'float salary' is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.	
    }*/
}

//4. decompile
class Test(int value)
{
    public int Value => value;
}
/*
  internal class Test
  {
    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int <value>P;

    public Test(int value)
    {
      this.<value>P = value;
      base.ctor();
    }

    public int Value
    {
      get
      {
        return this.<value>P;
      }
    }
  }
 */
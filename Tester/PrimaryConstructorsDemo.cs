using static Nemesis.Demos.Extensions;

namespace Tester;

[Order(1)]
internal class PrimaryConstructors : IShowable
{
    public void Show()
    {
        new Person("Mike", "Oldfield", new("UK", "London")).Dump();

        DecompileAsCSharp(typeof(Person));
    }
}

file class Person(string name/*0. not exposed outside*/, string familyName, Address address)
{
    public string Name => name; //1. Property 1
    public string FamilyName { get; } = familyName; //2. Property 2
    public Address Address { get; } = address;
    //public readonly string name = name; //3. capture

    public void Greet() => Console.WriteLine($"Hello, {name}!");
}

file class Address(string country, string city)
{
    public string Country { get; } = country;
    public string City { get; } = city;
}

file class Developer(string name, string familyName, float salary)
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
file class Test(int value)
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
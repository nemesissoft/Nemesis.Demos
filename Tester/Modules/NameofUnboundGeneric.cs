namespace Tester.Modules;

public sealed class NameofUnboundGeneric(DemoRunner demo) : Runnable(demo, order: 3, group: "C# 14 features", description: "03. nameof in unbound generics")
{
    public override void Run()
    {
        Section("nameof in unbound generics");

        HighlightCode("""
            // C# 14 allows nameof with unbound generic types.
            string name = nameof(List<>); // "List"

            // pre C# 14
            string invalidName = nameof(List<>); // Error: The type 'List<>' cannot be used with the 'nameof' operator because it is an unbound generic type.
            string name = nameof(List<int>); // "List"   
            
            """);
    }
}
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Nemesis.Demos.Highlighters;
using Spectre.Console;
namespace Tester;

internal partial class JsonExamples(DemoRunner demo) : RunnableAsync(demo, order: 102)
{
    public override void Run()
    {
        Section("JSON");
        Action[] actions = [Required, MissingProperty, ResolverChain, JsonSourceGenerationOptions_NewOptions, DisableReflection_ForAot, PopulateReadOnlyMembers, NewTypesSupport, OptionImmutability, NewsInJsonNode];

        foreach (var action in actions)
        {
            DrawLine(action.Method.Name);
            action();
        }
    }

    public override async Task RunAsync()
    {
        Subsection("Single");
        await JsonNodeSingleAsync();
        Subsection("Multi");
        await JsonNodeMultiAsync();
    }

    private void HighlightJson(string json, string? prepend = null)
    {
        if (prepend is not null)
            AnsiConsole.Write(prepend);
        HighlightCode(json, Language.Json);
    }

    private void Required()
    {
        Dump(JsonSerializer.Deserialize("""{"Name" : "Mike", "Age" : 39 }""", MyContext.Default.Person));

        ExpectFailure<JsonException>(
            () => JsonSerializer.Deserialize("""{"Name" : "Mike" }""", MyContext.Default.Person),
            "was missing required properties, including the following: Age"
        );
    }

    private static void MissingProperty()
    {
        ExpectFailure<JsonException>(
            () => JsonSerializer.Deserialize("""{"Name" : "Mike", "Age" : 39, "FamilyName" : "Oldfield" }""", MyContext.Default.Person),
            "The JSON property 'FamilyName' could not be mapped to any .NET member contained in type"
        );
    }

    //alternatively: [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public record Person
    {
        public required string Name { get; init; }
        public required int Age { get; init; }

        public override string? ToString() => $"{Name} @ {Age}";

    }


    private void ResolverChain()
    {
        var task = new WeeklyRecurringTask(DaysOfWeek.Wednesday);

        HighlightJson(JsonSerializer.Serialize(task, MyContext.Default.WeeklyRecurringTask)); //{"Day":4}

        HighlightJson(JsonSerializer.Serialize(task, MyContext2.Default.WeeklyRecurringTask)); //{"day":"Wednesday"}

        //ANALYSER CA1869
        var options = new JsonSerializerOptions();
        //options.TypeInfoResolver = JsonTypeInfoResolver.Combine(ContextA.Default, ContextB.Default, ContextC.Default);
        options.TypeInfoResolverChain.Insert(0, MyContext2.Default);

        HighlightJson(JsonSerializer.Serialize(task, options));//{"Day":"Wednesday"}
    }

    public record WeeklyRecurringTask(DaysOfWeek Day);

    [Flags]
    public enum DaysOfWeek : byte
    {
        None = 0,
        Monday
            = 0b0000_0001,
        Tuesday
            = 0b0000_0010,
        Wednesday
            = 0b0000_0100,
        Thursday
            = 0b0000_1000,
        Friday
            = 0b0001_0000,
        Saturday
            = 0b0010_0000,
        Sunday
            = 0b0100_0000,

        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekends = Saturday | Sunday,
        All = Weekdays | Weekends
    }



    [JsonSourceGenerationOptions(UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
    [
        JsonSerializable(typeof(Person)),
        JsonSerializable(typeof(WeeklyRecurringTask))
    ]
    public partial class MyContext : JsonSerializerContext { }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(WeeklyRecurringTask))]
    public partial class MyContext2 : JsonSerializerContext { }




    private static void JsonSourceGenerationOptions_NewOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            DefaultBufferSize = 10
        };
        //[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowTrailingCommas = true, DefaultBufferSize = 10)]
    }

    private void DisableReflection_ForAot()
    {
        Dump(JsonSerializer.IsReflectionEnabledByDefault, "IsReflectionEnabledByDefault: ");
        JsonSerializer.Serialize(42);

        //<JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>
        /*Extensions.ExpectFailure<InvalidOperationException>(
            () => JsonSerializer.Serialize(42),
            "Reflection-based serialization has been disabled for this application"
        );*/
    }


    private void PopulateReadOnlyMembers()
    {
        Dump(JsonSerializer.Deserialize<ReadOnlyMember>("""{ "Values" : [1,2,3] }"""));

        Dump(JsonSerializer.Deserialize<PopulatingMember>("""{ "Populate" : [11,22,33], "Replace" : [111,222,333] }"""));
    }

    public class ReadOnlyMember
    {
        public List<int> Values { get; } = [];

        public override string ToString() => $"[{string.Join(", ", Values)}]";
    }

    //alternatively: [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    //or var options = new JsonSerializerOptions { PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate };
    //[JsonSourceGenerationOptions(PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate)]
    public class PopulatingMember
    {
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<int> Populate { get; } = [1, 2, 3];

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Replace)]
        public List<int> Replace { get; set; } = [4, 5, 6];

        public override string ToString() => $"{nameof(Populate)}[{string.Join(", ", Populate)}] ; {nameof(Replace)}[{string.Join(", ", Replace)}]";
    }


    private void NewTypesSupport()
    {
        HighlightJson(JsonSerializer.Serialize(new object[] { Half.MaxValue, Int128.MaxValue, UInt128.MaxValue }), "Numbers:"); //[65500,170141183460469231731687303715884105727,340282366920938463463374607431768211455]

        HighlightJson(JsonSerializer.Serialize<ReadOnlyMemory<byte>>(new byte[] { 1, 2, 3, 4, 5 }), "Base64: "); // "AQIDBAU="
        HighlightJson(JsonSerializer.Serialize<ReadOnlyMemory<int>>(new int[] { 1, 2, 3 }), "Numbers: "); // [1,2,3]
    }


    private void OptionImmutability()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = MyContext.Default
            .WithAddedModifier(static typeInfo =>
            {
                foreach (JsonPropertyInfo prop in typeInfo.Properties)
                {
                    prop.Name = $"_{prop.Name.ToUpperInvariant()}_";
                }
            })
        };
        HighlightJson(JsonSerializer.Serialize(new WeeklyRecurringTask(DaysOfWeek.Weekdays), options)); //{"_DAY_":31}


        var options2 = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options2.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;

        options2.MakeReadOnly();
        ExpectFailure<InvalidOperationException>(
            () => options2.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper,
            "JsonSerializerOptions instance is read-only or has already been used in serialization or deserialization."
        );

        HighlightJson(JsonSerializer.Serialize(new Singer("Mike", "Oldfield"), options2));//{"name":"Mike","family-name":"Oldfield"}
    }

    record Singer(string Name, string FamilyName);



    private void NewsInJsonNode()
    {
        var node = JsonNode.Parse("""{"Name" : "Mike", "Age" : 39, "Prop" : { "NestedProp" : 42 } }""")!;
        HighlightJson(node.ToJsonString(), "Original:\n");

        var other = node.DeepClone();
        HighlightJson(other.ToJsonString(), "Cloned:\n");

        Dump(JsonNode.DeepEquals(node, other), "Are same");


        var jsonArray = new JsonArray(1, 2, 3, 2);
        IEnumerable<int> values = jsonArray.GetValues<int>().Where(i => i == 2);
    }

    private async Task JsonNodeSingleAsync()
    {
        var text = """{"Name" : "Mike", "Age" : 39, "Prop" : { "NestedProp" : 42 } }"""u8.ToArray();
        using var stream = new MemoryStream(text);
        var node = await JsonNode.ParseAsync(stream);
        HighlightJson(node?.ToJsonString() ?? "{}");
    }

    private async Task JsonNodeMultiAsync()
    {
        using var client = new HttpClient();
        IAsyncEnumerable<Book?> books = client
            .GetFromJsonAsAsyncEnumerable<Book>(@"https://raw.githubusercontent.com/bvaughn/infinite-list-reflow-examples/master/books.json");

        var table = new Table { Border = TableBorder.Rounded };
        table.AddColumn("No.");
        table.AddColumn("ISBN");
        table.AddColumn("Title");
        table.AddColumn("Authors");

        int i = 1;
        await foreach (var book in books)
        {
            table.AddRow(
                $"{i,3}",
                book?.Isbn ?? "",
                book?.Title ?? "",
                string.Join(", ", book?.Authors ?? []));
            if (i++ >= 10) break;
        }

        AnsiConsole.Write(table);
    }

    record Book(string Isbn, string Title, string[] Authors);
}
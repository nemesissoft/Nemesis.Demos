using System.Text;
using ICSharpCode.Decompiler.CSharp;
using Microsoft.CodeAnalysis;
using Nemesis.Demos.Highlighters;
using Spectre.Console;

namespace Nemesis.Demos;

public partial class DemoRunner
{
    public DemoOptions DemoOptions { get; }
    public MarkupSyntaxHighlighterFactory HighlighterFactory { get; }
    public Decompiler Decompiler { get; }

    private readonly string? _title;
    private readonly IReadOnlyCollection<Type> _demoTypes;

    public DemoRunner(DemoOptions? demosOptions = null, string? title = null)
    {
        DemoOptions = demosOptions ?? new();
        _title = title;
        HighlighterFactory = new MarkupSyntaxHighlighterFactory(DemoOptions);
        Decompiler = new(DemoOptions);

        _demoTypes =
            Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && typeof(Runnable).IsAssignableFrom(t))
            .ToList().AsReadOnly();

    }

    public async Task Run(string[]? args = null)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (!string.IsNullOrWhiteSpace(_title))
        {
            AnsiConsole.Write(
                new FigletText(FigletFontStore.GetFont("univers"), _title)
                .LeftJustified()
                .Color(Color.Red)
                .Centered()
            );
        }

        if (args is not null && args.Length > 0 && args.Any(a => string.Equals(a, "/debug", StringComparison.OrdinalIgnoreCase)) && !Debugger.IsAttached)
            Debugger.Launch();


        var demoGroups = _demoTypes
              .Select(t => CreateRunnable(t, this))
              .OrderBy(r => r.Order)
              .GroupBy(r => r.Group)
              .OrderBy(g => g.Key);

        IEnumerable<Runnable> builtInActions = [new ClearAction(), new ChangeThemeAction(DemoOptions), new ChangeLanguageVersionAction(DemoOptions), new ExitAction()];


        var prompt =
            new SelectionPrompt<Runnable>()
                .PageSize(40)
                .WrapAround(true)
                .UseConverter(s => s.Description)
                .EnableSearch()
                ;

        foreach (var group in demoGroups)
            prompt.AddChoiceGroup(new NoOpAction(group.Key), group);

        prompt.AddChoiceGroup(new NoOpAction("Built in"), builtInActions);

        while (true)
        {
            Runnable choice = await AnsiConsole.PromptAsync(prompt);

            try
            {
                choice.Run();
                await choice.RunAsync();
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }
        }
    }

    private static Runnable CreateRunnable(Type type, DemoRunner demo)
    {
        var ctors = type.GetConstructors();
        if (ctors.Length != 1)
            throw new NotSupportedException($"Only single public constructor is supported by {type.Name}");
        var ctor = ctors[0];

        var ctorParams = ctor.GetParameters();
        return ctorParams.Length switch
        {
            0 => (Runnable)Activator.CreateInstance(type)!,
            1 => ctorParams[0].ParameterType == typeof(DemoRunner)
                ? (Runnable)ctor.Invoke([demo])
                : throw new NotSupportedException($"Only parameter of type DemoRunner is supported for constructor of arity 1 in {type.Name}"),
            _ => throw new NotSupportedException($"Only constructor of arity 0..1 is supported by {type.Name}")
        };
    }

    private abstract class BuiltInRunnable() : Runnable(null!);

    private sealed class NoOpAction(string description) : BuiltInRunnable
    {
        public override string Description => description;
    }

    private sealed class ClearAction() : BuiltInRunnable
    {
        public override void Run() => AnsiConsole.Clear();

        public override string Description => "Clear";
    }

    private sealed class ChangeThemeAction(DemoOptions Options) : BuiltInRunnable
    {
        public override string Description => "Change theme";

        const string DEMO_CODE = """
            using System;

            // This is a single-line comment.
            public class TokenExample
            {
                // Define a constant number
                private const Int32 MaxCount = 100;

                /*
                This is a multi-line comment, demonstrating 
                comments that span multiple lines.
                */
                public void RunExample()
                {
                    string name = "Spectre.Console"; // String literal
                    int count = MaxCount + 5;        // Number literal

                    if (count > 100)
                    {
                        Console.WriteLine(name);
                    }
                }
            }
            """;

        public override void Run()
        {
            SyntaxNode parsedCode = CSharpHighlighter.GetParsedCodeRoot(DEMO_CODE);

            AnsiConsole.Clear();

            var state = new SelectionState<ThemeMeta>(
                SyntaxTheme.All.Select(t => new ThemeMeta(t.Theme, IsCurrent: Equals(Options.Theme, t.Theme))).ToList()
                );
            var console = AnsiConsole.Console;

            SyntaxTheme? result = AnsiConsole.Live(CreateLayout(state, parsedCode))
                .AutoClear(true)
                .Start(ctx =>
                {
                    ctx.UpdateTarget(CreateLayout(state, parsedCode));

                    while (true)
                    {
                        ConsoleKeyInfo? key = console.Input.ReadKey(intercept: true);

                        if (!key.HasValue)// If the terminal doesn't support reading keys, or the key is null, we break (or continue)                        
                            Thread.Sleep(50); // Prevent burning CPU if ReadKey somehow returns null rapidly                         
                        else
                        {
                            var oldIndex = state.SelectedIndex;

                            switch (key.Value.Key)
                            {
                                case ConsoleKey.UpArrow:
                                case ConsoleKey.LeftArrow: state.SelectPrev(); break;

                                case ConsoleKey.DownArrow:
                                case ConsoleKey.RightArrow: state.SelectNext(); break;

                                case ConsoleKey.Home: state.SelectFirst(); break;

                                case ConsoleKey.End: state.SelectLast(); break;

                                case ConsoleKey.Enter: return state.SelectedOption.Theme;

                                case ConsoleKey.Escape: return null;
                            }

                            if (oldIndex != state.SelectedIndex)
                                ctx.UpdateTarget(CreateLayout(state, parsedCode));
                        }
                    }
                });
            if (result is not null)
            {
                Options.Theme = result;
                AnsiConsole.MarkupLine($"[yellow]Theme changed to {result.Name}![/]");
            }
        }

        private static Layout CreateLayout(SelectionState<ThemeMeta> state, SyntaxNode parsedCode) =>
            new Layout("Root")
                .SplitColumns(
                    new Layout("SelectionPanel", CreateSelectionPanel(state)).Size(30),
                    new Layout("DetailPanel", CreateDetailPanel(state.SelectedOption.Theme, parsedCode))
                );

        private static Panel CreateSelectionPanel(SelectionState<ThemeMeta> state)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[yellow]Use Up, Down, Home and End to select, [green]Enter[/] to confirm or [red]Esc[/] to cancel.[/]");
            sb.AppendLine();

            for (int i = 0; i < state.Options.Count; i++)
            {
                var meta = state.Options[i];

                string text = meta.Theme.Name;

                text = meta.IsCurrent ? $"[underline]{text}[/]" : text;

                if (i == state.SelectedIndex)
                    sb.AppendLine($"[black on blue]> {text} <[/]"); // Highlight the selected option
                else
                    sb.AppendLine($"  {text}");
            }

            var selectionContent = new Markup(sb.ToString());

            return new Panel(selectionContent)
                .Header("[bold white]Themes[/]")
                .Expand();
        }

        private static Panel CreateDetailPanel(SyntaxTheme theme, SyntaxNode parsedCode)
        {
            var markup = new CSharpHighlighter(new() { Theme = theme })
                .GetHighlightedMarkup(parsedCode);

            var themeName = theme.Name;
            if (themeName.EndsWith("Light"))
                themeName = themeName[..^"Light".Length] + Emoji.Known.SunWithFace;
            else if (themeName.EndsWith("Dark"))
                themeName = themeName[..^"Dark".Length] + Emoji.Known.NewMoon;

            return new Panel(new Markup(markup))
                .Header($"[bold white]Preview for {themeName}[/]")
                .Expand();
        }

        private sealed record ThemeMeta(SyntaxTheme Theme, bool IsCurrent);

        private sealed class SelectionState<T>
        {
            public IReadOnlyList<T> Options { get; }
            public int SelectedIndex { get; private set; } = 0;
            public T SelectedOption => Options[SelectedIndex];

            public SelectionState(IReadOnlyList<T> options)
            {
                if (options == null || options.Count == 0)
                    throw new ArgumentNullException(nameof(options), "options should contain at least 1 element");
                Options = options;
            }

            public void SelectPrev() => SelectedIndex = SelectedIndex switch
            {
                > 0 => SelectedIndex - 1,
                _ => Options.Count - 1,// wrap to last
            };

            public void SelectNext() => SelectedIndex = SelectedIndex < Options.Count - 1
                ? SelectedIndex + 1
                : 0;// wrap to first

            public void SelectFirst() => SelectedIndex = 0;

            public void SelectLast() => SelectedIndex = Options.Count - 1;
        }
    }

    private sealed class ChangeLanguageVersionAction(DemoOptions Options) : BuiltInRunnable
    {
        public override void Run()
        {
            var (selectedVersion, _) = AnsiConsole.Prompt(
                new SelectionPrompt<(LanguageVersion Version, bool IsCurrent)>()
                    .Title("[green]Select default decompiler language version:[/]")
                    .WrapAround(true)
                    .PageSize(30)
                    .AddChoices(
                        Enum.GetValues<LanguageVersion>().Select(lv => (Version: lv, IsCurrent: Equals(Options.DefaultDecompilerLanguageVersion, lv)))
                    )
                    .UseConverter(t => t.IsCurrent ? $"[green]{t.Version} **[/]" : t.Version.ToString())
                    .EnableSearch()
            );

            Options.DefaultDecompilerLanguageVersion = selectedVersion;
            AnsiConsole.MarkupLine($"[yellow]Language version changed to {selectedVersion}![/]");
        }

        public override string Description => "Change decompiler language version";
    }

    private sealed class ExitAction() : BuiltInRunnable
    {
        public override void Run()
        {
            if (AnsiConsole.Confirm("[red]Are you sure you want to quit?[/]"))
                Environment.Exit(0);
        }

        public override string Description => "Exit";
    }
}
using Spectre.Console;

namespace Nemesis.Demos;
public class DemoRunner
{
    public static void Run(string[]? args = null)
    {
        if (args is not null)
            Extensions.CheckDebugger(args);

        IEnumerable<IShowable> demos =
            Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && typeof(IShowable).IsAssignableFrom(t))
            .Select(t => (
                Instance: (IShowable)Activator.CreateInstance(t)!,
                Order: t.GetCustomAttribute<OrderAttribute>()?.Value ?? int.MaxValue
                )
            )
            .OrderBy(tuple => tuple.Order)
            .Select(t => t.Instance)
            .ToList();
        IEnumerable<IShowable> builtInActions = [new ClearAction(), new ChangeThemeAction(), new ExitAction()];


        var prompt =
            new SelectionPrompt<IShowable>()
                .Title("Select an [green]option[/]:")
                .PageSize(25)
                .UseConverter(s => s.Description)
                .AddChoiceGroup(new NoOpAction("Demos"), demos)
                .AddChoiceGroup(new NoOpAction("Built in"), builtInActions)
                .EnableSearch()
            ;

        while (true)
        {
            var choice = AnsiConsole.Prompt(prompt);

            try
            {
                choice.Show();
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods);
            }

        }
    }
}

public interface IShowable
{
    void Show();

    string Description => GetType().Name;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int value) : Attribute
{
    public int Value => value;
}

record NoOpAction(string Description) : IShowable
{
    public void Show() { }
}

class ClearAction : IShowable
{
    public void Show() => AnsiConsole.Clear();

    public string Description => "Clear";
}

class ChangeThemeAction : IShowable
{
    public void Show()
    {
        var (name, theme, _) = AnsiConsole.Prompt(
                    new SelectionPrompt<(string Name, SyntaxTheme Theme, bool IsCurrent)>()
                        .Title("[green]Select a theme:[/]")
                        .PageSize(20)
                        .AddChoices(
                            SyntaxTheme.All.Select(t => (t.Name, t.Theme, IsCurrent: Equals(Extensions.Theme, t.Theme)))
                        )
                        .UseConverter(t => t.IsCurrent ? $"[green]{t.Name} **[/]" : t.Name)
                        .EnableSearch()
        );

        Extensions.Theme = theme;
        AnsiConsole.MarkupLine($"[yellow]Theme changed to {name}![/]");
    }

    public string Description => "Change theme";
}

class ExitAction : IShowable
{
    public void Show()
    {
        if (AnsiConsole.Confirm("[red]Are you sure you want to quit?[/]"))
            Environment.Exit(0);
    }

    public string Description => "Exit";
}

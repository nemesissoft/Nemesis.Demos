using Spectre.Console;

namespace Nemesis.Demos;
public class DemoRunner
{
    public static void Run(string[]? args = null)
    {
        if (args is not null)
            Extensions.CheckDebugger(args);

        var showables =
            Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && typeof(IShowable).IsAssignableFrom(t))
            .Select(t => (
                Instance: (IShowable)Activator.CreateInstance(t)!,
                Order: t.GetCustomAttribute<OrderAttribute>()?.Value ?? int.MaxValue,
                t.Name
                )
            )
            .OrderBy(tuple => tuple.Order)
            .ToList().AsReadOnly();


        int choice = -1;
        while (true)
        {
            int pad = (int)Math.Ceiling(Math.Log10(showables.Count + 1));

            for (int i = 0; i < showables.Count; i++)
                Console.WriteLine($"{(i + 1).ToString().PadLeft(pad + 1)}. {showables[i].Name}");

            using (Terminal.ForeColor(ConsoleColor.DarkGreen))
                Console.Write($"{Environment.NewLine}Select demo: ");
            var text = Console.ReadLine();
            if (string.Equals("EXIT", text, StringComparison.OrdinalIgnoreCase))
                break;
            else if (string.Equals("CLEAR", text, StringComparison.OrdinalIgnoreCase) || string.Equals("CLS", text, StringComparison.OrdinalIgnoreCase))
                AnsiConsole.Clear();
            else if (int.TryParse(text, out choice) && choice >= 1 && choice <= showables.Count)
                try
                {
                    showables[choice - 1].Instance.Show();
                }
                catch (Exception e)
                {
                    using var _ = Terminal.ForeColor(ConsoleColor.Red);

                    var lines = e.ToString().Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.None)
                        .Select(s => $"    {s}");

                    Console.WriteLine("ERROR:" + Environment.NewLine + string.Join(Environment.NewLine, lines));
                }
        }
    }
}

public interface IShowable
{
    void Show();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int value) : Attribute
{
    public int Value => value;
}

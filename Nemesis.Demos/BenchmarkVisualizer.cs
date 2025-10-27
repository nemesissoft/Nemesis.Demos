using Spectre.Console;

namespace Nemesis.Demos;

public static class BenchmarkVisualizer
{
    public static void Render(TextReader csvReader, Action<BenchmarkVisualizerOptions>? optionsBuilder = null)
    {
        BenchmarkVisualizerOptions options = new();
        optionsBuilder?.Invoke(options);

        var results = LoadBenchmarkResults(csvReader, options);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No valid benchmark entries found.[/]");
            return;
        }

        NormalizeUnits(results, "ns");

        if (options.ShowTable) RenderTable(results);

        if (options.ShowGraph) RenderChart(results);
    }

    static IList<BenchmarkResult> LoadBenchmarkResults(TextReader csvReader, BenchmarkVisualizerOptions options)
    {
        var list = new List<BenchmarkResult>();

        var headerLine = csvReader.ReadLine();

        if (string.IsNullOrWhiteSpace(headerLine))
            return list;

        var header = headerLine.Split(options.Separator);
        int methodIdx = FindIndex("Method"), jobIdx = FindIndex("Job"), meanIdx = FindIndex("Mean"), ratioIdx = FindIndex("Ratio"), gen0Idx = FindIndex("Gen0"), allocIdx = FindIndex("Allocated");

        int FindIndex(string columnName) =>
            Array.FindIndex(header, h => string.Equals(h, columnName, StringComparison.OrdinalIgnoreCase));

        String? line;
        while ((line = csvReader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(options.Separator);
            if (parts.Length <= Math.Max(meanIdx, methodIdx))
                continue;

            double meanValue = ExtractMean(TrimRecursive(parts[meanIdx]), out string unit);

            list.Add(new BenchmarkResult
            {
                Method = (jobIdx >= 0 && TrimRecursive(parts[jobIdx]) is { } job && !string.IsNullOrWhiteSpace(job) ? $"{job} \\ " : "") + TrimRecursive(parts[methodIdx]),
                Mean = meanValue,
                OriginalUnit = unit,
                Ratio = ratioIdx >= 0 ? TrimRecursive(parts[ratioIdx]) : "",
                Gen0 = gen0Idx >= 0 ? TrimRecursive(parts[gen0Idx]) : "",
                Allocated = allocIdx >= 0 ? TrimRecursive(parts[allocIdx]) : ""
            });
        }

        return list.AsReadOnly();
    }

    private static string TrimRecursive(string input)
    {
        if (input is null) return "";

        string trimmed = input.Trim();

        return trimmed switch
        {
            ['"', .. var inner, '"'] => TrimRecursive(new string(inner)),
            ['\'', .. var inner, '\''] => TrimRecursive(new string(inner)),
            _ => trimmed
        };
    }

    static double ExtractMean(string text, out string unit)
    {
        unit = "ns";
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return 0;

        string numberPart = parts[0].Trim();
        if (parts.Length > 1)
            unit = parts[1].Trim();

        double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double value);
        return value;
    }

    static void NormalizeUnits(IEnumerable<BenchmarkResult> results, string targetUnit)
    {
        foreach (var r in results)
        {
            r.Mean = ConvertToUnit(r.Mean, r.OriginalUnit, targetUnit);
            r.OriginalUnit = targetUnit;
        }
    }

    static double ConvertToUnit(double value, string from, string to)
    {
        double factor = from switch
        {
            "s" => 1_000_000_000,
            "ms" => 1_000_000,
            "µs" or "us" => 1_000,
            "ns" => 1,
            _ => 1
        };

        double toFactor = to switch
        {
            "s" => 1_000_000_000,
            "ms" => 1_000_000,
            "µs" or "us" => 1_000,
            "ns" => 1,
            _ => 1
        };

        return value * (factor / toFactor);
    }

    static void RenderTable(IEnumerable<BenchmarkResult> results)
    {
        var table = new Table().Border(TableBorder.Rounded).Title("[bold blue]📋 Benchmark Summary[/]")
            .AddColumns(
            new TableColumn("[bold]Method[/]"),
            new TableColumn("[bold]Mean (ns)[/]").RightAligned(),
            new TableColumn("[bold]Ratio[/]").RightAligned(),
            new TableColumn("[bold]Gen0[/]").RightAligned(),
            new TableColumn("[bold]Allocated[/]").RightAligned()
            );

        bool hasBaseline = results.Any(r => r.Ratio.Contains("baseline", StringComparison.OrdinalIgnoreCase));

        foreach (var r in results)
        {
            var ratioColor = hasBaseline ?
                r.Ratio switch
                {
                    { } s when s.StartsWith('-') => "green",
                    { } s when s.Contains("baseline", StringComparison.OrdinalIgnoreCase) => "grey",
                    { } s when string.IsNullOrWhiteSpace(s) => "grey",
                    _ => "red"
                } : "grey";

            table.AddRow(
                $"[cyan]{r.Method}[/]",
                r.Mean.ToString("N2", CultureInfo.InvariantCulture),
                $"[{ratioColor}]{r.Ratio}[/]",
                r.Gen0,
                r.Allocated
            );
        }

        AnsiConsole.Write(table);
    }

    static void RenderChart(IEnumerable<BenchmarkResult> results)
    {
        AnsiConsole.WriteLine();

        var chart = new BarChart()
            .Width(80)
            .Label("[bold yellow]📈 Mean Execution Time (converted to ns)[/]")
            .CenterLabel()
            .Culture(CultureInfo.InvariantCulture)
            .UseValueFormatter(d => d.ToString("N2", CultureInfo.InvariantCulture))
            .ShowValues()
            ;

        double baselineOrMin = (results.FirstOrDefault(r => r.Ratio.Contains("baseline", StringComparison.OrdinalIgnoreCase))
            ?? results.MinBy(r => r.Mean)
            ?? throw new InvalidOperationException("No benchmark results found")
            ).Mean;

        foreach (var r in results.OrderByDescending(r => r.Mean))
        {
            var improvement = (1 - r.Mean / baselineOrMin) * 100;

            const double Epsilon = 1e-6;

            var color = (r.Mean, improvement) switch
            {
                var (mean, _) when Math.Abs(mean - baselineOrMin) < Epsilon => Color.Grey,
                var (_, imp) when imp > 0 => Color.Green,
                _ => Color.Red
            };

            var emoji = (r.Mean, improvement) switch
            {
                var (mean, _) when Math.Abs(mean - baselineOrMin) < Epsilon => "⚪",
                var (_, imp) when imp > 0 => "🟢",
                _ => "🔴"
            };

            chart.AddItem($"{emoji} {r.Method}", (float)r.Mean, color);
        }

        AnsiConsole.Write(chart);
    }

    private sealed class BenchmarkResult
    {
        public string Method { get; set; } = "";
        public double Mean { get; set; }
        public string OriginalUnit { get; set; } = "ns";
        public string Ratio { get; set; } = "";
        public string Gen0 { get; set; } = "";
        public string Allocated { get; set; } = "";
    }
}

public class BenchmarkVisualizerOptions
{
    public char Separator { get; set; } = ';';
    public bool ShowTable { get; set; } = true;
    public bool ShowGraph { get; set; } = true;
}

using System.Collections;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using QRCoder;
using Spectre.Console;

namespace Nemesis.Demos;

public partial class Runnable
{
    private static void DumpUriAsQrCode(string urlToEncode)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(urlToEncode, QRCodeGenerator.ECCLevel.H);

        if (qrData.ModuleMatrix is { } modules)
        {
            var qrMarkup = QrHelpers.GetMarkupForQr(modules);

            if (OperatingSystem.IsWindows())
            {
                QRCode qrCode = new(qrData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                var qrCodeFile = QrHelpers.GetTempPngPathFromUrl(urlToEncode);
                qrCodeImage.Save(qrCodeFile);

                var qrPanel = new Panel(new Markup(qrMarkup + $"\n[link={Markup.Escape(new Uri(Path.GetFullPath(qrCodeFile)).AbsoluteUri)}]Click here to show saved QR code[/]"))
                {
                    Header = new PanelHeader("Scan Me", Justify.Center),
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(1, 0, 1, 0)
                };

                AnsiConsole.Write(qrPanel);
            }
            else
            {
                AnsiConsole.MarkupLine(qrMarkup);
                AnsiConsole.WriteLine();
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to generate QR code.[/]");
            return;
        }
    }
}

static partial class QrHelpers
{
    internal static string GetMarkupForQr(List<BitArray> modules)
    {
        int size = modules.Count;

        var qrBuilder = new StringBuilder();
        // Print with top & bottom half-blocks (▀ = upper half, ▄ = lower half). Each terminal row will represent two QR rows.
        for (int y = 0; y < size; y += 2)
        {
            var line = new StringBuilder();

            for (int x = 0; x < size; x++)
            {
                bool top = modules[y][x];
                bool bottom = (y + 1 < size) && modules[y + 1][x];

                // Map combination of bits to character and color
                if (top && bottom)
                    line.Append("[black on black]█[/]"); // both black
                else if (top && !bottom)
                    line.Append("[black on white]▀[/]"); // upper black
                else if (!top && bottom)
                    line.Append("[black on white]▄[/]"); // lower black
                else
                    line.Append("[white on white] [/]");
            }

            qrBuilder.AppendLine(line.ToString());
        }
        return qrBuilder.ToString();
    }

    internal static string GetTempPngPathFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL must not be null or empty.", nameof(url));

        string normalized = UriStripper.Replace(url, "");

        char[] illegalChars = Path.GetInvalidFileNameChars();
        foreach (char c in illegalChars)
            normalized = normalized.Replace(c, '_');

        normalized = UriSpecialCharactersStripper.Replace(normalized, "_");

        string tempDir = Path.GetTempPath();

        int maxBaseLength = 256 - tempDir.Length - 20; //reserve place for extensions and possible suffixes
        if (normalized.Length > maxBaseLength)
            normalized = normalized[..maxBaseLength];


        string fileName = $"{normalized}.png";
        string fullPath = Path.Combine(tempDir, fileName);

        // If it already exists, add a random suffix
        int counter = 1;
        while (File.Exists(fullPath) || fullPath.Length >= 256)
        {
            string suffix = $"_{counter++}";
            fileName = $"{normalized}{suffix}.png";
            if (fileName.Length > maxBaseLength)
                fileName = string.Concat(fileName.AsSpan(0, maxBaseLength - suffix.Length - 4), suffix, ".png");
            fullPath = Path.Combine(tempDir, fileName);
        }

        return fullPath;
    }

    [GeneratedRegex(@"^(https?|ftp)://(www\.)?", RegexOptions.IgnoreCase)]
    private static partial Regex UriStripper { get; }

    [GeneratedRegex(@"[^a-zA-Z0-9._-]")]
    private static partial Regex UriSpecialCharactersStripper { get; }
}

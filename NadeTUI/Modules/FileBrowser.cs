using Spectre.Console;

namespace NadeTUI.Modules;

public static class FileBrowser
{
    private static readonly string[] TextExtensions =
    {
        ".txt", ".cs", ".json", ".xml", ".md", ".log", ".yml", ".yaml"
    };

    public static void Run()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[yellow]Directory:[/] {currentDir}");

            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(Path.GetFileName)
                .Prepend("..")
                .ToList();

            entries.Add("Back");

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Browse")
                    .AddChoices(entries));

            if (selection == "Back")
                return;

            if (selection == "..")
            {
                currentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
                continue;
            }

            var fullPath = Path.Combine(currentDir, selection);

            if (Directory.Exists(fullPath))
            {
                currentDir = fullPath;
            }
            else
            {
                var ext = Path.GetExtension(fullPath).ToLower();

                if (TextExtensions.Contains(ext))
                {
                    TextEditor.Run(fullPath);
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Unsupported file type.[/]");
                    Console.ReadKey();
                }
            }
        }
    }
}
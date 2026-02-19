using Spectre.Console;
using NadeTUI.Modules;

namespace NadeTUI.Core;

public class Dashboard
{
    public void Run()
    {
        if (!IsInteractiveTerminal())
        {
            RunBasicMode();
            return;
        }

        RunSpectreMode();
    }

    private bool IsInteractiveTerminal()
    {
        try
        {
            return AnsiConsole.Profile.Capabilities.Ansi &&
                   AnsiConsole.Profile.Capabilities.Interactive;
        }
        catch
        {
            return false;
        }
    }

    private void RunSpectreMode()
    {
        while (true)
        {
            try
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Panel("[bold cyan]NadeTUI System Dashboard[/]")
                        .Border(BoxBorder.Rounded)
                );

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select module")
                        .PageSize(10)
                        .AddChoices(
                            "File Browser",
                            "Text Editor",
                            "System Information",
                            "Disk Usage",
                            "System Monitor",
                            "Exit"
                        )
                );

                HandleSelection(choice);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
    }

    private void RunBasicMode()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("NadeTUI (Basic Mode)");
            Console.WriteLine("--------------------");
            Console.WriteLine("1) File Browser");
            Console.WriteLine("2) Text Editor");
            Console.WriteLine("3) System Information");
            Console.WriteLine("4) Disk Usage");
            Console.WriteLine("5) System Monitor");
            Console.WriteLine("6) Exit");

            Console.Write("\nSelect: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1": SafeRun(() => FileBrowser.Run()); break;
                case "2":
                    Console.Write("Enter file path: ");
                    var path = Console.ReadLine() ?? "";
                    SafeRun(() => TextEditor.Run(path));
                    break;
                case "3": SafeRun(() => SystemInfo.Run()); break;
                case "4": SafeRun(() => DiskUsage.Run()); break;
                case "5": SafeRun(() => SystemMonitor.Run()); break;
                case "6": return;
            }
        }
    }

    private void HandleSelection(string choice)
    {
        switch (choice)
        {
            case "File Browser": SafeRun(() => FileBrowser.Run()); break;
            case "Text Editor":
                var path = AnsiConsole.Ask<string>("Enter file path:");
                SafeRun(() => TextEditor.Run(path));
                break;
            case "System Information": SafeRun(() => SystemInfo.Run()); break;
            case "Disk Usage": SafeRun(() => DiskUsage.Run()); break;
            case "System Monitor": SafeRun(() => SystemMonitor.Run()); break;
            case "Exit": Environment.Exit(0); break;
        }
    }

    private void SafeRun(Action module)
    {
        try
        {
            module();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void ShowError(Exception ex)
    {
        try
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Panel($"[red]{ex.Message}[/]")
                    .Header("Module Error")
                    .Border(BoxBorder.Double)
            );
            Console.ReadKey();
        }
        catch
        {
            Console.WriteLine("Fatal error:");
            Console.WriteLine(ex);
            Console.ReadKey();
        }
    }
}

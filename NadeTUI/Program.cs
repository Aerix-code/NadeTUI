using Spectre.Console;
using NadeTUI.Core;

class Program
{
    static void Main()
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.Detect,
            Interactive = InteractionSupport.Yes
        });

        AnsiConsole.Console = console;

        var dashboard = new Dashboard();
        dashboard.Run();
    }
}
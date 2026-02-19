namespace NadeTUI.Core;

public static class TerminalState
{
    private static int _width = Console.WindowWidth;
    private static int _height = Console.WindowHeight;

    public static bool HasResized()
    {
        if (_width != Console.WindowWidth ||
            _height != Console.WindowHeight)
        {
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
            return true;
        }

        return false;
    }
}
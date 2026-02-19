using Spectre.Console;

namespace NadeTUI.Modules;

public static class TextEditor
{
    public static void Run(string path)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var lines = File.Exists(path)
            ? File.ReadAllLines(path).ToList()
            : new List<string> { "" };

        int cursorX = 0;
        int cursorY = 0;
        int scrollOffset = 0;

        while (true)
        {
            AdjustScroll(ref scrollOffset, cursorY);
            DrawEditor(path, lines, cursorX, cursorY, scrollOffset);

            var key = Console.ReadKey(true);

            if (key.Modifiers == ConsoleModifiers.Control)
            {
                if (key.Key == ConsoleKey.S)
                {
                    File.WriteAllLines(path, lines);
                }
                else if (key.Key == ConsoleKey.X)
                {
                    Console.CursorVisible = false;
                    return;
                }
            }
            else
            {
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (cursorX > 0) cursorX--;
                        break;

                    case ConsoleKey.RightArrow:
                        if (cursorX < lines[cursorY].Length) cursorX++;
                        break;

                    case ConsoleKey.UpArrow:
                        if (cursorY > 0) cursorY--;
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;

                    case ConsoleKey.DownArrow:
                        if (cursorY < lines.Count - 1) cursorY++;
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;

                    case ConsoleKey.Backspace:
                        if (cursorX > 0)
                        {
                            lines[cursorY] =
                                lines[cursorY].Remove(cursorX - 1, 1);
                            cursorX--;
                        }
                        else if (cursorY > 0)
                        {
                            int prevLength = lines[cursorY - 1].Length;
                            lines[cursorY - 1] += lines[cursorY];
                            lines.RemoveAt(cursorY);
                            cursorY--;
                            cursorX = prevLength;
                        }
                        break;

                    case ConsoleKey.Enter:
                        var remainder = lines[cursorY].Substring(cursorX);
                        lines[cursorY] =
                            lines[cursorY].Substring(0, cursorX);
                        lines.Insert(cursorY + 1, remainder);
                        cursorY++;
                        cursorX = 0;
                        break;

                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            lines[cursorY] =
                                lines[cursorY].Insert(cursorX, key.KeyChar.ToString());
                            cursorX++;
                        }
                        break;
                }
            }
        }
    }

    private static void AdjustScroll(ref int scrollOffset, int cursorY)
    {
        int editorHeight = Console.WindowHeight - 4;

        if (cursorY < scrollOffset)
            scrollOffset = cursorY;

        if (cursorY >= scrollOffset + editorHeight)
            scrollOffset = cursorY - editorHeight + 1;
    }

    private static void DrawEditor(
        string path,
        List<string> lines,
        int cursorX,
        int cursorY,
        int scrollOffset)
    {
        Console.SetCursorPosition(0, 0);
        Console.Clear();

        Console.WriteLine($"NadeTUI Nano - {path}");
        Console.WriteLine("Ctrl+S Save | Ctrl+X Exit");
        Console.WriteLine(new string('-', Console.WindowWidth));

        int editorHeight = Console.WindowHeight - 4;

        for (int i = 0; i < editorHeight; i++)
        {
            int lineIndex = scrollOffset + i;

            if (lineIndex >= lines.Count)
            {
                Console.WriteLine("~");
                continue;
            }

            Console.WriteLine($"{lineIndex + 1,4} {lines[lineIndex]}");
        }

        int visibleY = cursorY - scrollOffset;
        Console.SetCursorPosition(cursorX + 5, visibleY + 3);
    }
}

using Spectre.Console;

namespace NadeTUI.Modules;

public static class DiskUsage
{
    public static void Run()
    {
        AnsiConsole.Clear();

        var table = new Table();
        table.AddColumn("Mount");
        table.AddColumn("Total (GB)");
        table.AddColumn("Used (GB)");
        table.AddColumn("Free (GB)");

        var mounts = File.ReadAllLines("/proc/mounts")
            .Select(l => l.Split(' '))
            .Where(parts => parts[0].StartsWith("/dev/"))
            .Select(parts => parts[1])
            .Distinct();

        foreach (var mount in mounts)
        {
            try
            {
                var drive = new DriveInfo(mount);

                if (!drive.IsReady)
                    continue;

                double total = drive.TotalSize / 1_073_741_824.0;
                double free = drive.AvailableFreeSpace / 1_073_741_824.0;
                double used = total - free;

                table.AddRow(
                    mount,
                    total.ToString("F2"),
                    used.ToString("F2"),
                    free.ToString("F2")
                );
            }
            catch
            {
                table.AddRow(mount, "Access Error", "-", "-");
            }
        }

        AnsiConsole.Write(table);
        Console.ReadKey();
    }
}
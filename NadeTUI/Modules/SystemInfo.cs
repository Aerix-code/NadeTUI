using Spectre.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NadeTUI.Modules;

public static class SystemInfo
{
    public static void Run()
    {
        AnsiConsole.Clear();

        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("OS", RuntimeInformation.OSDescription);
        table.AddRow("Architecture", RuntimeInformation.OSArchitecture.ToString());
        table.AddRow("Machine Name", Environment.MachineName);
        table.AddRow(".NET Version", Environment.Version.ToString());

        table.AddRow("CPU Model", GetCpuModel());
        table.AddRow("CPU Cores", Environment.ProcessorCount.ToString());

        var (totalRam, freeRam) = GetMemoryInfo();
        table.AddRow("Total RAM (GB)", totalRam);
        table.AddRow("Free RAM (GB)", freeRam);

        table.AddRow("GPU", GetGpuInfo());

        AnsiConsole.Write(table);
        Console.ReadKey();
    }

    private static string GetCpuModel()
    {
        try
        {
            var line = File.ReadLines("/proc/cpuinfo")
                .First(l => l.StartsWith("model name"));
            return line.Split(":")[1].Trim();
        }
        catch
        {
            return "Unknown";
        }
    }

    private static (string total, string free) GetMemoryInfo()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var total = lines.First(l => l.StartsWith("MemTotal"));
            var free = lines.First(l => l.StartsWith("MemAvailable"));

            double totalGb = ParseKb(total) / 1024 / 1024;
            double freeGb = ParseKb(free) / 1024 / 1024;

            return (totalGb.ToString("F2"), freeGb.ToString("F2"));
        }
        catch
        {
            return ("Unknown", "Unknown");
        }
    }

    private static double ParseKb(string line)
    {
        return double.Parse(
            new string(line.Where(char.IsDigit).ToArray())
        );
    }

    private static string GetGpuInfo()
    {
        try
        {
            var drmPath = "/sys/class/drm";
            if (!Directory.Exists(drmPath))
                return "No GPU detected";

            var cards = Directory.GetDirectories(drmPath, "card*");

            if (!cards.Any())
                return "No GPU detected";

            var integrated = cards.Any(c =>
                File.Exists(Path.Combine(c, "device/vendor")) &&
                File.ReadAllText(Path.Combine(c, "device/vendor")).Contains("0x8086")
            );

            return integrated ? "Integrated (Intel detected)" : "Discrete GPU detected";
        }
        catch
        {
            return "Unknown";
        }
    }
}

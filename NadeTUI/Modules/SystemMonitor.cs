using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NadeTUI.Modules;

public static class SystemMonitor
{
    private static double _lastIdle = 0;
    private static double _lastTotal = 0;

    public static void Run()
    {
        Console.Clear();
        Console.CursorVisible = false;

        try
        {
            while (!Console.KeyAvailable)
            {
                if (NadeTUI.Core.TerminalState.HasResized())
                    Console.Clear();

                // Gather stats
                double cpu = GetCpuUsage();
                (double usedRam, double totalRam) = GetMemoryUsage();
                string gpuInfo = GetGpuInfo();

                // Draw dashboard
                DrawDashboard(cpu, usedRam, totalRam, gpuInfo);

                Thread.Sleep(1000);
            }

            Console.ReadKey(true); // consume exit key
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private static void DrawDashboard(double cpu, double usedRam, double totalRam, string gpu)
    {
        Console.SetCursorPosition(0, 0);

        int width = Console.WindowWidth - 10;
        width = Math.Max(width, 10);

        // CPU bar
        double cpuPercent = Math.Min(Math.Max(cpu, 0), 100);
        Console.WriteLine("CPU Usage:");
        Console.WriteLine(BuildBar(cpuPercent, width) + $" {cpuPercent:F2}%");

        // RAM bar
        double ramPercent = Math.Min(Math.Max((usedRam / totalRam) * 100, 0), 100);
        Console.WriteLine($"RAM Usage: {usedRam:F2}/{totalRam:F2} GB");
        Console.WriteLine(BuildBar(ramPercent, width) + $" {ramPercent:F0}%");

        // GPU info
        Console.WriteLine("GPU Info:");
        Console.WriteLine(string.IsNullOrEmpty(gpu) ? "No GPU detected" : gpu);

        Console.WriteLine("\nPress any key to exit...");
    }

    private static string BuildBar(double percent, int width)
    {
        int fill = (int)(percent / 100 * width);
        int empty = width - fill;
        return "[" + new string('█', fill) + new string(' ', empty) + "]";
    }

    private static double GetCpuUsage()
    {
        try
        {
            var statLine = File.ReadLines("/proc/stat").First();
            var parts = statLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            double user = double.Parse(parts[1]);
            double nice = double.Parse(parts[2]);
            double system = double.Parse(parts[3]);
            double idle = double.Parse(parts[4]);

            double total = user + nice + system + idle;

            double totalDiff = total - _lastTotal;
            double idleDiff = idle - _lastIdle;

            _lastTotal = total;
            _lastIdle = idle;

            if (totalDiff == 0) return 0;

            return 100.0 * (1.0 - idleDiff / totalDiff);
        }
        catch
        {
            return 0;
        }
    }

    private static (double used, double total) GetMemoryUsage()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            double total = ParseKb(lines.First(l => l.StartsWith("MemTotal")));
            double available = ParseKb(lines.First(l => l.StartsWith("MemAvailable")));
            double used = total - available;
            return (used / 1024 / 1024, total / 1024 / 1024); // GB
        }
        catch
        {
            return (0, 1);
        }
    }

    private static double ParseKb(string line)
    {
        return double.Parse(new string(line.Where(char.IsDigit).ToArray()));
    }

    private static string GetGpuInfo()
    {
        try
        {
            // Detect CPU vendor
            string cpuVendor = "Unknown";
            try
            {
                var cpuInfo = File.ReadAllLines("/proc/cpuinfo");
                var vendorLine = cpuInfo.FirstOrDefault(l => l.StartsWith("vendor_id"));
                if (vendorLine != null)
                    cpuVendor = vendorLine.Split(':')[1].Trim();
            }
            catch { }

            // Run lspci for VGA devices
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lspci",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            var lines = output.Split('\n')
                .Where(l => l.Contains("VGA", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (lines.Length == 0)
                return "No GPU detected";

            string gpuLine = lines[0].Trim();

            // Detect integrated GPU
            bool integrated = false;

            if (cpuVendor.Contains("GenuineIntel"))
            {
                // Intel CPU → integrated Intel GPU
                integrated = true;
            }
            else if (cpuVendor.Contains("AuthenticAMD"))
            {
                // AMD CPU → check known APUs
                string[] apuKeywords = { "Lucienne", "Renoir", "Cezanne", "Raven", "Dali" };
                if (apuKeywords.Any(k => gpuLine.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    integrated = true;
            }

            // Return simplified string
            if (integrated)
                return "CPU integrated Graphics";

            return gpuLine; // discrete GPU
        }
        catch
        {
            return "No GPU detected";
        }
    }

}

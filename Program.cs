using System.Diagnostics;

namespace PerformanceCountersDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Windows Performance Counters Demo");
            Console.WriteLine("==================================");
            Console.WriteLine("Press Ctrl+C to exit\n");

            // Initialize all performance counters
            var counters = InitializeCounters();

            if (counters.Count == 0)
            {
                Console.WriteLine("No performance counters could be initialized. Make sure you're running on Windows with appropriate permissions.");
                return;
            }

            // Display counter values every 2 seconds
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Windows Performance Counters Demo - " + DateTime.Now.ToString("HH:mm:ss"));
                Console.WriteLine("==================================================\n");

                DisplayProcessorCounters(counters);
                DisplayMemoryCounters(counters);
                DisplayDiskCounters(counters);
                DisplayNetworkCounters(counters);

                Console.WriteLine("\nPress Ctrl+C to exit...");
                Thread.Sleep(2000);
            }
        }

        static Dictionary<string, PerformanceCounter> InitializeCounters()
        {
            var counters = new Dictionary<string, PerformanceCounter>();

            var counterDefinitions = new Dictionary<string, (string Category, string CounterName, string Instance)>
            {
                // Processor Counters
                ["ProcessorTime"] = ("Processor", "% Processor Time", "_Total"),
                ["UserTime"] = ("Processor", "% User Time", "_Total"),
                ["PrivilegedTime"] = ("Processor", "% Privileged Time", "_Total"),

                // Memory Counters
                ["AvailableBytes"] = ("Memory", "Available Bytes", ""),
                ["PagesPerSec"] = ("Memory", "Pages/sec", ""),
                ["PoolNonpagedBytes"] = ("Memory", "Pool Nonpaged Bytes", ""),

                // Physical Disk Counters
                ["DiskTime"] = ("PhysicalDisk", "% Disk Time", "_Total"),
                ["DiskReadsPerSec"] = ("PhysicalDisk", "Disk Reads/sec", "_Total"),
                ["AvgDiskQueueLength"] = ("PhysicalDisk", "Avg. Disk Queue Length", "_Total"),

                // Network Interface Counters (using first available network interface)
                ["BytesTotalPerSec"] = ("Network Interface", "Bytes Total/sec", GetFirstNetworkInterface()),
                ["PacketsPerSec"] = ("Network Interface", "Packets/sec", GetFirstNetworkInterface()),
                ["CurrentBandwidth"] = ("Network Interface", "Current Bandwidth", GetFirstNetworkInterface())
            };

            foreach (var kvp in counterDefinitions)
            {
                try
                {
                    var (category, counterName, instance) = kvp.Value;
                    
                    if (string.IsNullOrEmpty(instance) || instance == "Not Available")
                        continue;

                    var counter = string.IsNullOrEmpty(instance) 
                        ? new PerformanceCounter(category, counterName)
                        : new PerformanceCounter(category, counterName, instance);

                    // Initialize the counter (first call often returns 0)
                    counter.NextValue();
                    counters[kvp.Key] = counter;
                    
                    Console.WriteLine($"✓ Initialized: {category} - {counterName} ({instance})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Failed to initialize {kvp.Key}: {ex.Message}");
                }
            }

            Console.WriteLine($"\nInitialized {counters.Count} performance counters successfully.");
            Console.WriteLine("\nWaiting 2 seconds before starting monitoring...\n");
            Thread.Sleep(2000);

            return counters;
        }

        static string GetFirstNetworkInterface()
        {
            try
            {
                var category = new PerformanceCounterCategory("Network Interface");
                var instanceNames = category.GetInstanceNames();
                
                // Skip loopback and isatap interfaces, prefer Ethernet or Wi-Fi
                var preferredNames = instanceNames
                    .Where(name => !name.Contains("Loopback") && 
                                   !name.Contains("isatap") && 
                                   !name.Contains("Teredo"))
                    .OrderByDescending(name => name.Contains("Ethernet") || name.Contains("Wi-Fi"))
                    .FirstOrDefault();

                return preferredNames ?? "Not Available";
            }
            catch
            {
                return "Not Available";
            }
        }

        static void DisplayProcessorCounters(Dictionary<string, PerformanceCounter> counters)
        {
            Console.WriteLine("🖥️  PROCESSOR PERFORMANCE");
            Console.WriteLine("─────────────────────────");
            
            DisplayCounter(counters, "ProcessorTime", "Total CPU Usage", "%", 1);
            DisplayCounter(counters, "UserTime", "User Mode CPU", "%", 1);
            DisplayCounter(counters, "PrivilegedTime", "Kernel Mode CPU", "%", 1);
            Console.WriteLine();
        }

        static void DisplayMemoryCounters(Dictionary<string, PerformanceCounter> counters)
        {
            Console.WriteLine("💾 MEMORY PERFORMANCE");
            Console.WriteLine("─────────────────────");
            
            DisplayCounter(counters, "AvailableBytes", "Available Memory", "MB", 1024 * 1024);
            DisplayCounter(counters, "PagesPerSec", "Memory Page Faults", "/sec", 1);
            DisplayCounter(counters, "PoolNonpagedBytes", "Nonpaged Pool", "MB", 1024 * 1024);
            Console.WriteLine();
        }

        static void DisplayDiskCounters(Dictionary<string, PerformanceCounter> counters)
        {
            Console.WriteLine("💿 DISK PERFORMANCE");
            Console.WriteLine("───────────────────");
            
            DisplayCounter(counters, "DiskTime", "Disk Activity", "%", 1);
            DisplayCounter(counters, "DiskReadsPerSec", "Disk Reads", "/sec", 1);
            DisplayCounter(counters, "AvgDiskQueueLength", "Disk Queue Length", "requests", 1);
            Console.WriteLine();
        }

        static void DisplayNetworkCounters(Dictionary<string, PerformanceCounter> counters)
        {
            Console.WriteLine("🌐 NETWORK PERFORMANCE");
            Console.WriteLine("──────────────────────");
            
            DisplayCounter(counters, "BytesTotalPerSec", "Network Throughput", "KB/s", 1024);
            DisplayCounter(counters, "PacketsPerSec", "Network Packets", "/sec", 1);
            DisplayCounter(counters, "CurrentBandwidth", "Link Speed", "Mbps", 1000000);
            Console.WriteLine();
        }

        static void DisplayCounter(Dictionary<string, PerformanceCounter> counters, string key, string displayName, string unit, float divisor = 1)
        {
            if (counters.TryGetValue(key, out var counter))
            {
                try
                {
                    var value = counter.NextValue() / divisor;
                    var formattedValue = unit == "%" ? $"{value:F1}" : $"{value:F2}";
                    
                    Console.WriteLine($"  {displayName,-20}: {formattedValue,8} {unit}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {displayName,-20}: Error - {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"  {displayName,-20}: Not Available");
            }
        }
    }
} 
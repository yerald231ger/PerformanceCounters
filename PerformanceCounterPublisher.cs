using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace PerformanceCounterPublisher
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public class PerformanceCounterPublisher
    {
        private const string CATEGORY_NAME = "MyApp Performance";
        private const string CATEGORY_HELP = "Custom performance counters for MyApp demonstration";

        // Custom Performance Counters
        private PerformanceCounter? _apiCallsPerSecCounter;
        private PerformanceCounter? _apiResponseTimeCounter;
        private PerformanceCounter? _diskWritesPerSecCounter;
        private PerformanceCounter? _diskBytesWrittenCounter;
        private PerformanceCounter? _memoryUsageCounter;
        private PerformanceCounter? _activeTasksCounter;
        private PerformanceCounter? _errorRateCounter;
        private PerformanceCounter? _cpuTasksPerSecCounter;
        private PerformanceCounter? _cpuIntensiveOperationsCounter;
        private PerformanceCounter? _averageCalculationTimeCounter;

        // Activity tracking
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private readonly List<byte[]> _memoryAllocations;
        private long _totalApiCalls = 0;
        private long _totalDiskWrites = 0;
        private long _totalBytesWritten = 0;
        private long _totalErrors = 0;
        private int _activeTasks = 0;
        private long _totalCpuTasks = 0;
        private long _totalCpuOperations = 0;

        public PerformanceCounterPublisher()
        {
            _httpClient = new HttpClient();
            _random = new Random();
            _memoryAllocations = new List<byte[]>();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Performance Counter Publisher Demo");
            Console.WriteLine("=================================");
            Console.WriteLine();

            try
            {
                // Create custom performance counters
                await CreatePerformanceCountersAsync();
                
                Console.WriteLine("✅ Custom performance counters created successfully!");
                Console.WriteLine($"📊 Category: '{CATEGORY_NAME}'");
                Console.WriteLine();
                Console.WriteLine("You can now view these counters in Windows Performance Monitor:");
                Console.WriteLine("1. Press Win+R, type 'perfmon', press Enter");
                Console.WriteLine("2. Click the green '+' button to add counters");
                Console.WriteLine($"3. Look for the '{CATEGORY_NAME}' category");
                Console.WriteLine();
                Console.WriteLine("Starting application activities...");
                Console.WriteLine("Press Ctrl+C to stop");
                Console.WriteLine();

                // Start background activities
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                // Run multiple activities concurrently
                var tasks = new[]
                {
                    SimulateApiCallsAsync(cancellationTokenSource.Token),
                    SimulateDiskWritesAsync(cancellationTokenSource.Token),
                    SimulateMemoryUsageAsync(cancellationTokenSource.Token),
                    SimulateCpuIntensiveTasksAsync(cancellationTokenSource.Token),
                    UpdatePerformanceCountersAsync(cancellationTokenSource.Token),
                    DisplayStatusAsync(cancellationTokenSource.Token)
                };

                await Task.WhenAny(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                if (ex.Message.Contains("Access is denied"))
                {
                    Console.WriteLine();
                    Console.WriteLine("💡 Solution: Run this application as Administrator");
                    Console.WriteLine("   Creating performance counters requires elevated privileges.");
                }
            }
            finally
            {
                // Cleanup
                DisposeCounters();
                _httpClient?.Dispose();
                Console.WriteLine("\n🧹 Cleanup completed. Press any key to exit...");
                Console.ReadKey();
            }
        }

        private async Task CreatePerformanceCountersAsync()
        {
            // Check if category already exists
            if (PerformanceCounterCategory.Exists(CATEGORY_NAME))
            {
                Console.WriteLine($"⚠️  Category '{CATEGORY_NAME}' already exists. Deleting and recreating...");
                PerformanceCounterCategory.Delete(CATEGORY_NAME);
                
                // Wait a bit for the deletion to complete
                await Task.Delay(2000);
            }

            // Define counter data
            var counterDataCollection = new CounterCreationDataCollection
            {
                new CounterCreationData(
                    "API Calls/sec",
                    "Number of API calls per second",
                    PerformanceCounterType.RateOfCountsPerSecond32),

                new CounterCreationData(
                    "Average API Response Time",
                    "Average response time for API calls in milliseconds",
                    PerformanceCounterType.NumberOfItems32),

                new CounterCreationData(
                    "Disk Writes/sec",
                    "Number of disk write operations per second",
                    PerformanceCounterType.RateOfCountsPerSecond32),

                new CounterCreationData(
                    "Disk Bytes Written",
                    "Total bytes written to disk",
                    PerformanceCounterType.NumberOfItems64),

                new CounterCreationData(
                    "Memory Usage (MB)",
                    "Current memory usage in megabytes",
                    PerformanceCounterType.NumberOfItems32),

                new CounterCreationData(
                    "Active Background Tasks",
                    "Number of currently active background tasks",
                    PerformanceCounterType.NumberOfItems32),

                new CounterCreationData(
                    "Error Rate %",
                    "Percentage of operations that resulted in errors",
                    PerformanceCounterType.RawFraction),

                new CounterCreationData(
                    "CPU Tasks/sec",
                    "Number of CPU-intensive tasks started per second",
                    PerformanceCounterType.RateOfCountsPerSecond32),

                new CounterCreationData(
                    "CPU Operations Completed",
                    "Total number of CPU-intensive operations completed",
                    PerformanceCounterType.NumberOfItems64),

                new CounterCreationData(
                    "Average Calculation Time",
                    "Average time for CPU calculations in milliseconds",
                    PerformanceCounterType.NumberOfItems32)
            };

            // Create the category
            PerformanceCounterCategory.Create(
                CATEGORY_NAME,
                CATEGORY_HELP,
                PerformanceCounterCategoryType.SingleInstance,
                counterDataCollection);

            // Wait for category creation to complete
            await Task.Delay(3000);

            // Initialize performance counters
            _apiCallsPerSecCounter = new PerformanceCounter(CATEGORY_NAME, "API Calls/sec", false);
            _apiResponseTimeCounter = new PerformanceCounter(CATEGORY_NAME, "Average API Response Time", false);
            _diskWritesPerSecCounter = new PerformanceCounter(CATEGORY_NAME, "Disk Writes/sec", false);
            _diskBytesWrittenCounter = new PerformanceCounter(CATEGORY_NAME, "Disk Bytes Written", false);
            _memoryUsageCounter = new PerformanceCounter(CATEGORY_NAME, "Memory Usage (MB)", false);
            _activeTasksCounter = new PerformanceCounter(CATEGORY_NAME, "Active Background Tasks", false);
            _errorRateCounter = new PerformanceCounter(CATEGORY_NAME, "Error Rate %", false);
            _cpuTasksPerSecCounter = new PerformanceCounter(CATEGORY_NAME, "CPU Tasks/sec", false);
            _cpuIntensiveOperationsCounter = new PerformanceCounter(CATEGORY_NAME, "CPU Operations Completed", false);
            _averageCalculationTimeCounter = new PerformanceCounter(CATEGORY_NAME, "Average Calculation Time", false);

            // Initialize all counters to 0
            _apiCallsPerSecCounter.RawValue = 0;
            _apiResponseTimeCounter.RawValue = 0;
            _diskWritesPerSecCounter.RawValue = 0;
            _diskBytesWrittenCounter.RawValue = 0;
            _memoryUsageCounter.RawValue = 0;
            _activeTasksCounter.RawValue = 0;
            _errorRateCounter.RawValue = 0;
            _cpuTasksPerSecCounter.RawValue = 0;
            _cpuIntensiveOperationsCounter.RawValue = 0;
            _averageCalculationTimeCounter.RawValue = 0;
        }

        private async Task SimulateApiCallsAsync(CancellationToken cancellationToken)
        {
            var apiEndpoints = new[]
            {
                "https://jsonplaceholder.typicode.com/posts/1",
                "https://jsonplaceholder.typicode.com/posts/2",
                "https://jsonplaceholder.typicode.com/users/1",
                "https://httpbin.org/delay/1",
                "https://httpbin.org/status/200"
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Interlocked.Increment(ref _activeTasks);
                    
                    var endpoint = apiEndpoints[_random.Next(apiEndpoints.Length)];
                    var stopwatch = Stopwatch.StartNew();

                    // Simulate API call
                    using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    stopwatch.Stop();

                    // Update counters
                    Interlocked.Increment(ref _totalApiCalls);
                    _apiCallsPerSecCounter?.Increment();
                    _apiResponseTimeCounter!.RawValue = stopwatch.ElapsedMilliseconds;

                    if (!response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref _totalErrors);
                    }
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _totalErrors);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasks);
                }

                // Random delay between API calls (500ms to 3 seconds)
                await Task.Delay(_random.Next(500, 3000), cancellationToken);
            }
        }

        private async Task SimulateDiskWritesAsync(CancellationToken cancellationToken)
        {
            var logDirectory = Path.Combine(Path.GetTempPath(), "MyAppLogs");
            Directory.CreateDirectory(logDirectory);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Interlocked.Increment(ref _activeTasks);

                    // Generate sample data to write
                    var logData = new
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = _random.Next(1, 5) switch
                        {
                            1 => "DEBUG",
                            2 => "INFO",
                            3 => "WARN",
                            4 => "ERROR",
                            _ => "INFO"
                        },
                        Message = $"Sample log message {_random.Next(1000, 9999)}",
                        Data = new { Value = _random.Next(1, 100), Status = "Active" }
                    };

                    var json = JsonConvert.SerializeObject(logData, Formatting.Indented);
                    var bytes = Encoding.UTF8.GetBytes(json + Environment.NewLine);

                    // Write to file
                    var fileName = Path.Combine(logDirectory, $"app-{DateTime.Now:yyyy-MM-dd}.log");
                    await File.AppendAllTextAsync(fileName, json + Environment.NewLine, cancellationToken);

                    // Update counters
                    Interlocked.Increment(ref _totalDiskWrites);
                    Interlocked.Add(ref _totalBytesWritten, bytes.Length);
                    _diskWritesPerSecCounter?.Increment();
                    _diskBytesWrittenCounter!.RawValue = _totalBytesWritten;
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _totalErrors);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasks);
                }

                // Random delay between disk writes (1 to 4 seconds)
                await Task.Delay(_random.Next(1000, 4000), cancellationToken);
            }
        }

        private async Task SimulateMemoryUsageAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Randomly allocate or deallocate memory
                    if (_random.Next(0, 2) == 0 && _memoryAllocations.Count < 50)
                    {
                        // Allocate memory (1MB to 10MB chunks)
                        var size = _random.Next(1024 * 1024, 10 * 1024 * 1024);
                        var allocation = new byte[size];
                        _random.NextBytes(allocation); // Fill with random data
                        _memoryAllocations.Add(allocation);
                    }
                    else if (_memoryAllocations.Count > 0)
                    {
                        // Deallocate some memory
                        var index = _random.Next(_memoryAllocations.Count);
                        _memoryAllocations.RemoveAt(index);
                    }

                    // Calculate current memory usage
                    var totalMemoryMB = _memoryAllocations.Sum(arr => arr.Length) / (1024 * 1024);
                    _memoryUsageCounter?.SetValue(totalMemoryMB);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _totalErrors);
                }

                // Update every 2 seconds
                await Task.Delay(2000, cancellationToken);
            }
        }

        private async Task SimulateCpuIntensiveTasksAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Interlocked.Increment(ref _activeTasks);
                    Interlocked.Increment(ref _totalCpuTasks);
                    _cpuTasksPerSecCounter?.Increment();

                    var taskType = _random.Next(1, 5);
                    var stopwatch = Stopwatch.StartNew();

                    // Run different types of CPU-intensive operations
                    switch (taskType)
                    {
                        case 1:
                            await Task.Run(() => CalculatePrimeNumbers(1000 + _random.Next(9000)), cancellationToken);
                            break;
                        case 2:
                            await Task.Run(() => PerformMatrixMultiplication(50 + _random.Next(100)), cancellationToken);
                            break;
                        case 3:
                            await Task.Run(() => CalculateFibonacci(35 + _random.Next(10)), cancellationToken);
                            break;
                        case 4:
                            await Task.Run(() => PerformSortingOperations(10000 + _random.Next(40000)), cancellationToken);
                            break;
                    }

                    stopwatch.Stop();

                    // Update counters
                    Interlocked.Increment(ref _totalCpuOperations);
                    _cpuIntensiveOperationsCounter!.RawValue = _totalCpuOperations;
                    _averageCalculationTimeCounter!.RawValue = stopwatch.ElapsedMilliseconds;
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _totalErrors);
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasks);
                }

                // Random delay between CPU tasks (1 to 5 seconds)
                await Task.Delay(_random.Next(1000, 5000), cancellationToken);
            }
        }

        private void CalculatePrimeNumbers(int limit)
        {
            var primes = new List<int>();
            for (int i = 2; i <= limit; i++)
            {
                bool isPrime = true;
                for (int j = 2; j * j <= i; j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) primes.Add(i);
            }
            // Simulate using the result to prevent optimization
            var sum = primes.Sum();
        }

        private void PerformMatrixMultiplication(int size)
        {
            var matrix1 = new double[size, size];
            var matrix2 = new double[size, size];
            var result = new double[size, size];

            // Initialize matrices with random values
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix1[i, j] = _random.NextDouble();
                    matrix2[i, j] = _random.NextDouble();
                }
            }

            // Perform matrix multiplication
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < size; k++)
                    {
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }
        }

        private long CalculateFibonacci(int n)
        {
            if (n <= 1) return n;
            return CalculateFibonacci(n - 1) + CalculateFibonacci(n - 2);
        }

        private void PerformSortingOperations(int arraySize)
        {
            // Generate random array
            var array = new int[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                array[i] = _random.Next(1, 100000);
            }

            // Perform multiple sorting operations
            var bubbleArray = (int[])array.Clone();
            BubbleSort(bubbleArray);

            var quickArray = (int[])array.Clone();
            QuickSort(quickArray, 0, quickArray.Length - 1);
        }

        private void BubbleSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }
        }

        private void QuickSort(int[] arr, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(arr, low, high);
                QuickSort(arr, low, pi - 1);
                QuickSort(arr, pi + 1, high);
            }
        }

        private int Partition(int[] arr, int low, int high)
        {
            int pivot = arr[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (arr[j] < pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
            return i + 1;
        }

        private async Task UpdatePerformanceCountersAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Update active tasks counter
                    _activeTasksCounter?.SetValue(_activeTasks);

                    // Calculate error rate percentage
                    var totalOperations = _totalApiCalls + _totalDiskWrites + _totalCpuTasks;
                    if (totalOperations > 0)
                    {
                        var errorRate = (int)((_totalErrors * 100.0) / totalOperations);
                        _errorRateCounter?.SetValue(errorRate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating counters: {ex.Message}");
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        private async Task DisplayStatusAsync(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("🚀 Performance Counter Publisher - RUNNING");
                    Console.WriteLine($"⏱️  Runtime: {DateTime.Now - startTime:mm\\:ss}");
                    Console.WriteLine("=" + new string('=', 50));
                    Console.WriteLine();

                    Console.WriteLine("📊 ACTIVITY STATISTICS:");
                    Console.WriteLine($"   🌐 Total API Calls: {_totalApiCalls:N0}");
                    Console.WriteLine($"   💾 Total Disk Writes: {_totalDiskWrites:N0}");
                    Console.WriteLine($"   📝 Bytes Written: {_totalBytesWritten:N0}");
                    Console.WriteLine($"   🧠 Memory Allocated: {_memoryAllocations.Sum(arr => arr.Length) / (1024 * 1024):N0} MB");
                    Console.WriteLine($"   🖥️  CPU Tasks Started: {_totalCpuTasks:N0}");
                    Console.WriteLine($"   ⚙️  CPU Operations: {_totalCpuOperations:N0}");
                    Console.WriteLine($"   ⚡ Active Tasks: {_activeTasks}");
                    Console.WriteLine($"   ❌ Total Errors: {_totalErrors}");
                    Console.WriteLine();

                    Console.WriteLine("🔍 VIEW IN PERFORMANCE MONITOR:");
                    Console.WriteLine("   1. Press Win+R → type 'perfmon' → Enter");
                    Console.WriteLine("   2. Click green '+' button");
                    Console.WriteLine($"   3. Find '{CATEGORY_NAME}' category");
                    Console.WriteLine("   4. Add desired counters");
                    Console.WriteLine();
                    Console.WriteLine("Press Ctrl+C to stop...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Display error: {ex.Message}");
                }

                await Task.Delay(2000, cancellationToken);
            }
        }

        private void DisposeCounters()
        {
            _apiCallsPerSecCounter?.Dispose();
            _apiResponseTimeCounter?.Dispose();
            _diskWritesPerSecCounter?.Dispose();
            _diskBytesWrittenCounter?.Dispose();
            _memoryUsageCounter?.Dispose();
            _activeTasksCounter?.Dispose();
            _errorRateCounter?.Dispose();
            _cpuTasksPerSecCounter?.Dispose();
            _cpuIntensiveOperationsCounter?.Dispose();
            _averageCalculationTimeCounter?.Dispose();
        }
    }
} 
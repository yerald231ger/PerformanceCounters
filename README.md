# Performance Counter Publisher Demo

This console application demonstrates how to **create and publish custom Windows Performance Counters** that can be viewed in Windows Performance Monitor. The application performs various system activities (API calls, disk I/O, memory usage, CPU-intensive operations) and publishes real-time performance metrics.

## 📊 Custom Performance Counters Created

The application creates a **"MyApp Performance"** category with these counters:

| Counter Name | Type | Description |
|-------------|------|-------------|
| **API Calls/sec** | Rate | Number of API calls per second |
| **Average API Response Time** | Number | Response time in milliseconds |
| **Disk Writes/sec** | Rate | Disk write operations per second |
| **Disk Bytes Written** | Number | Total bytes written to disk |
| **Memory Usage (MB)** | Number | Current allocated memory in MB |
| **Active Background Tasks** | Number | Number of concurrent tasks |
| **Error Rate %** | Percentage | Percentage of failed operations |
| **CPU Tasks/sec** | Rate | Number of CPU-intensive tasks per second |
| **CPU Operations Completed** | Number | Total CPU operations completed |
| **Average Calculation Time** | Number | Average CPU task time in milliseconds |

## 🚀 Simulated Application Activities

1. **API Calls**: Makes HTTP requests to various endpoints
2. **Disk I/O**: Writes JSON log files to temp directory
3. **Memory Usage**: Dynamically allocates/deallocates memory
4. **CPU-Intensive Operations**: Prime number calculations, matrix multiplication, Fibonacci sequences, sorting algorithms
5. **Error Tracking**: Monitors and reports operation failures

## 🚀 How to Run

### Prerequisites
- **Windows OS** (Performance Counters are Windows-specific)
- **.NET 8.0 or later**
- **Administrator privileges** (REQUIRED for creating performance counters)

### Running the Application

**Important**: You MUST run as Administrator to create performance counters.

1. **Run as Administrator:**
   ```bash
   # Open Command Prompt as Administrator, then:
   dotnet run
   ```

2. **Or build and run executable:**
   ```bash
   dotnet build
   # Run as Administrator:
   dotnet bin/Debug/net8.0/PerformanceCountersDemo.exe
   ```

### Viewing in Performance Monitor
1. **Open Performance Monitor**: Press `Win+R`, type `perfmon`, press Enter
2. **Add Counters**: Click the green "+" button
3. **Find Your Category**: Look for **"MyApp Performance"** in the categories list
4. **Add Counters**: Select the counters you want to monitor
5. **Watch Real-Time Data**: See your application's activity reflected in the graphs!

### Expected Output
```
Performance Counter Publisher Demo
=================================

✅ Custom performance counters created successfully!
📊 Category: 'MyApp Performance'

You can now view these counters in Windows Performance Monitor:
1. Press Win+R, type 'perfmon', press Enter
2. Click the green '+' button to add counters
3. Look for the 'MyApp Performance' category

Starting application activities...
Press Ctrl+C to stop

🚀 Performance Counter Publisher - RUNNING
⏱️  Runtime: 02:34
==================================================

📊 ACTIVITY STATISTICS:
   🌐 Total API Calls: 45
   💾 Total Disk Writes: 28
   📝 Bytes Written: 125,432
   🧠 Memory Allocated: 127 MB
   🖥️  CPU Tasks Started: 18
   ⚙️  CPU Operations: 18
   ⚡ Active Tasks: 3
   ❌ Total Errors: 2

🔍 VIEW IN PERFORMANCE MONITOR:
   1. Press Win+R → type 'perfmon' → Enter
   2. Click green '+' button
   3. Find 'MyApp Performance' category
   4. Add desired counters

Press Ctrl+C to stop...
```

## 🔧 Key Implementation Details

### Creating Custom Performance Counters
```csharp
var counterDataCollection = new CounterCreationDataCollection
{
    new CounterCreationData(
        "API Calls/sec",
        "Number of API calls per second",
        PerformanceCounterType.RateOfCountsPerSecond32)
};

PerformanceCounterCategory.Create(
    "MyApp Performance",
    "Custom performance counters for MyApp",
    PerformanceCounterCategoryType.SingleInstance,
    counterDataCollection);
```

### Updating Counters
```csharp
// For rate counters - just increment
_apiCallsPerSecCounter?.Increment();

// For absolute values - set directly
_memoryUsageCounter?.SetValue(totalMemoryMB);

// For raw values - set RawValue property
_apiResponseTimeCounter.RawValue = stopwatch.ElapsedMilliseconds;
```

### Concurrent Activities
The application runs 6 concurrent tasks:
- API call simulation
- Disk write simulation  
- Memory allocation/deallocation
- CPU-intensive computation simulation
- Counter updates
- Console display updates

### CPU-Intensive Operations
The application performs various processor-intensive tasks:

1. **Prime Number Calculation**: Finds prime numbers up to randomly selected limits (1,000-10,000)
2. **Matrix Multiplication**: Performs mathematical operations on matrices (50x50 to 150x150)
3. **Fibonacci Sequence**: Calculates Fibonacci numbers recursively (35-45 terms)
4. **Sorting Algorithms**: Executes bubble sort and quicksort on large arrays (10,000-50,000 elements)

## ⚠️ Important Notes

1. **Windows Only**: This uses Windows Performance Counter API
2. **Permissions**: Some counters require elevated privileges
3. **Counter Availability**: Not all counters exist on every system
4. **Network Interface**: Auto-selects first non-loopback interface

## 🔗 Related Resources

- [Windows Performance Counters Documentation](https://docs.microsoft.com/en-us/windows/win32/perfctrs/performance-counters-portal)
- [.NET PerformanceCounter Class](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter)
- [Performance Monitor (PerfMon)](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/perfmon) 
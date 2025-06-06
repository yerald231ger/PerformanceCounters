# Performance Counter Publisher Demo

This application demonstrates how to **create and publish custom Windows Performance Counters** that can be viewed in Windows Performance Monitor. Unlike the previous demo that only *reads* existing counters, this application *creates* its own counters and publishes real activity data.

## 🎯 What This Application Does

### 📊 Creates Custom Performance Counters
The application creates a new performance counter category called **"MyApp Performance"** with these counters:

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

### 🚀 Simulates Real Application Activities

1. **API Calls**: Makes HTTP requests to various endpoints
2. **Disk I/O**: Writes JSON log files to temp directory
3. **Memory Usage**: Dynamically allocates/deallocates memory
4. **CPU-Intensive Operations**: Prime number calculations, matrix multiplication, Fibonacci sequences, sorting algorithms
5. **Error Tracking**: Monitors and reports operation failures

## 🏃‍♂️ How to Run

### Prerequisites
- **Windows OS** (Performance Counters are Windows-specific)
- **.NET 8.0 or later**
- **Administrator privileges** (REQUIRED for creating performance counters)

### Step 1: Build the Application
```bash
dotnet build
```

### Step 2: Run as Administrator
**Important**: You MUST run as Administrator to create performance counters.

**Option A - Command Line (as Admin):**
```bash
# Open Command Prompt as Administrator, then:
dotnet run --project PerformanceCounterPublisher.csproj
```

**Option B - PowerShell (as Admin):**
```powershell
# Open PowerShell as Administrator, then:
dotnet run --project PerformanceCounterPublisher.csproj
```

### Step 3: View in Performance Monitor
1. **Open Performance Monitor**: Press `Win+R`, type `perfmon`, press Enter
2. **Add Counters**: Click the green "+" button
3. **Find Your Category**: Look for **"MyApp Performance"** in the categories list
4. **Add Counters**: Select the counters you want to monitor
5. **Watch Real-Time Data**: See your application's activity reflected in the graphs!

## 📈 Expected Console Output

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

## 🔧 Technical Implementation Details

### Performance Counter Types Used

1. **RateOfCountsPerSecond32**: For "per second" metrics
   - Automatically calculates rate over time
   - Used for API calls/sec and Disk writes/sec

2. **NumberOfItems32/64**: For absolute values
   - Direct value display
   - Used for response times, memory usage, active tasks

3. **RawFraction**: For percentage values
   - Used for error rate percentage

### Key Code Patterns

#### Creating Counter Categories
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

#### Updating Counters
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

## 🔍 Viewing in Performance Monitor

### Step-by-Step Guide:

1. **Open PerfMon**: `Win+R` → `perfmon` → Enter

2. **Add Counters**: Click green "+" button

3. **Select Category**: Find "MyApp Performance" in the dropdown

4. **Available Counters**:
   - API Calls/sec
   - Average API Response Time
   - Disk Writes/sec  
   - Disk Bytes Written
   - Memory Usage (MB)
   - Active Background Tasks
   - Error Rate %
   - CPU Tasks/sec
   - CPU Operations Completed
   - Average Calculation Time

5. **Add All or Select**: Choose counters and click "Add"

6. **View Graphs**: See real-time data visualization!

### Pro Tips:
- **Scale**: Right-click graphs to adjust scale for better visibility
- **Time Range**: Change time window for different perspectives
- **Multiple Counters**: Add multiple counters to compare correlations
- **Export Data**: Save counter data for analysis

## 📁 Generated Files

The application creates log files in your temp directory:
- **Location**: `%TEMP%\MyAppLogs\`
- **Format**: `app-yyyy-MM-dd.log`
- **Content**: JSON formatted log entries

## ⚠️ Important Notes

### Administrator Rights Required
Creating performance counter categories requires administrator privileges. If you get "Access is denied" errors:

1. **Close the application**
2. **Right-click Command Prompt/PowerShell**
3. **Select "Run as Administrator"**
4. **Navigate to project directory**
5. **Run `dotnet run` again**

### Counter Persistence
- **Created counters persist** after application closes
- **Counters appear in PerfMon** even when app is not running
- **Values go to zero** when application stops
- **Category remains** until manually deleted

### Cleanup
To remove the custom counter category:
```csharp
// In code:
PerformanceCounterCategory.Delete("MyApp Performance");
```

Or use PowerShell as Administrator:
```powershell
[System.Diagnostics.PerformanceCounterCategory]::Delete("MyApp Performance")
```

## 🎯 Learning Objectives

After running this demo, you'll understand:

1. **How to create custom performance counter categories**
2. **Different performance counter types and their uses**
3. **How to update counters from application code**
4. **Integration between custom counters and Windows PerfMon**
5. **Real-world patterns for application monitoring**
6. **Thread-safe counter updates in multi-threaded applications**

## 🚀 Next Steps

This demo provides a foundation for:
- **Production application monitoring**
- **Custom business metrics**
- **Integration with monitoring tools**
- **Performance debugging and optimization**
- **Building monitoring dashboards**

The performance counters you create can be consumed by various monitoring tools beyond just Windows Performance Monitor, including System Center Operations Manager (SCOM), Nagios, and other enterprise monitoring solutions! 
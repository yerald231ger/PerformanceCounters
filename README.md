# Windows Performance Counters Demo

This console application demonstrates how to read and display various Windows Performance Counters in real-time using C#.

## 📊 Monitored System Metrics

### 🖥️ Processor Performance
- **Total CPU Usage**: Overall processor utilization percentage
- **User Mode CPU**: Time spent executing user-mode code
- **Kernel Mode CPU**: Time spent executing kernel/system code

### 💾 Memory Performance  
- **Available Memory**: Free physical memory in MB
- **Memory Page Faults**: Pages loaded from disk per second
- **Nonpaged Pool**: Kernel memory that cannot be paged to disk

### 💿 Disk Performance
- **Disk Activity**: Percentage of time disk is busy
- **Disk Reads**: Number of read operations per second
- **Disk Queue Length**: Average number of pending I/O requests

### 🌐 Network Performance
- **Network Throughput**: Total bytes sent/received per second
- **Network Packets**: Total packets sent/received per second  
- **Link Speed**: Current network interface bandwidth

## 🚀 How to Run

### Prerequisites
- **Windows OS** (Performance Counters are Windows-specific)
- **.NET 8.0 or later**
- **Administrator privileges** (recommended for full counter access)

### Running the Application

1. **Build and run:**
   ```bash
   dotnet run
   ```

2. **Or build and run executable:**
   ```bash
   dotnet build
   dotnet bin/Debug/net8.0/PerformanceCountersDemo.exe
   ```

### Expected Output
```
Windows Performance Counters Demo - 14:32:15
==================================================

🖥️  PROCESSOR PERFORMANCE
─────────────────────────
  Total CPU Usage     :     12.5 %
  User Mode CPU       :      8.3 %
  Kernel Mode CPU     :      4.2 %

💾 MEMORY PERFORMANCE
─────────────────────
  Available Memory    :   8432.15 MB
  Memory Page Faults  :     125.30 /sec
  Nonpaged Pool       :      89.75 MB

💿 DISK PERFORMANCE
───────────────────
  Disk Activity       :      5.2 %
  Disk Reads          :     15.80 /sec
  Disk Queue Length   :      0.02 requests

🌐 NETWORK PERFORMANCE
──────────────────────
  Network Throughput  :    245.67 KB/s
  Network Packets     :     89.50 /sec  
  Link Speed          :   1000.00 Mbps
```

## 🔧 Key Implementation Details

### Performance Counter Categories
- **Processor**: `"Processor"` category with `"_Total"` instance
- **Memory**: `"Memory"` category (no instance needed)
- **PhysicalDisk**: `"PhysicalDisk"` category with `"_Total"` instance
- **Network Interface**: `"Network Interface"` category with auto-detected interface

### Error Handling
- Graceful handling of missing or inaccessible counters
- Automatic network interface detection (skips loopback/virtual interfaces)
- Counter initialization validation with user feedback

### Performance Notes
- Counter values update every 2 seconds
- First `NextValue()` call initializes the counter (often returns 0)
- Real-time monitoring with console clearing for better UX

## 🔍 Understanding the Metrics

### CPU Metrics
- Values are percentages (0-100%)
- User + Kernel time should approximately equal Total CPU time
- High kernel time may indicate system bottlenecks

### Memory Metrics  
- Available Memory shows free RAM
- High page faults indicate memory pressure
- Nonpaged Pool growth can indicate kernel memory leaks

### Disk Metrics
- Queue Length > 2 consistently indicates disk bottleneck
- High disk time with low reads/writes suggests slow storage

### Network Metrics
- Throughput shows actual data transfer
- Compare with Link Speed to see utilization percentage

## ⚠️ Important Notes

1. **Windows Only**: This uses Windows Performance Counter API
2. **Permissions**: Some counters require elevated privileges
3. **Counter Availability**: Not all counters exist on every system
4. **Network Interface**: Auto-selects first non-loopback interface

## 🔗 Related Resources

- [Windows Performance Counters Documentation](https://docs.microsoft.com/en-us/windows/win32/perfctrs/performance-counters-portal)
- [.NET PerformanceCounter Class](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter)
- [Performance Monitor (PerfMon)](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/perfmon) 
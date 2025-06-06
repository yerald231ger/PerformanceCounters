using PerformanceCounterPublisher;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Performance Counter Publisher Demo";
        
        var publisher = new PerformanceCounterPublisher.PerformanceCounterPublisher();
        await publisher.RunAsync();
    }
} 
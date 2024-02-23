namespace WorkerExample
{
    public class Worker(IHostApplicationLifetime hostApplicationLifetime, IService1 service1) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(service1.Method1());
            hostApplicationLifetime.StopApplication();
        }
    }
}

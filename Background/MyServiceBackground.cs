namespace Library.Background;

public class MyServiceBackground : BackgroundService
{
	private readonly IServiceProvider serviceProvider;

	public MyServiceBackground(IServiceProvider serviceProvider) 
	{
		this.serviceProvider = serviceProvider;
    }

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		// code...
		return base.StartAsync(cancellationToken);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = serviceProvider.CreateScope();
		var service = serviceProvider.GetService<ITestScopedService>();
		// service code..
		await Task.Delay(1000);
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		// code...
		return base.StopAsync(cancellationToken);
	}
}

public interface ITestScopedService { }
public record TestScopedService : ITestScopedService { }
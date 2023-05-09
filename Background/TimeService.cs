namespace Library.Background;

public interface ITimeService
{
	public void PrintTime();
}

public class TimeService : ITimeService
{
	public void PrintTime()
	{
        Console.WriteLine(DateTime.Now.ToString("O"));
    }
}

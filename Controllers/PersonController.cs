using Hangfire;
using Library.API.DbContexts;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/person")]
public class PersonController : ControllerBase
{
	private readonly LibraryContext database;
	private readonly IBackgroundJobClient backgroundJobClient;

    public PersonController(LibraryContext database, IBackgroundJobClient backgroundJobClient)
    {
        this.database = database;
		this.backgroundJobClient = backgroundJobClient;
    }

 //   [HttpPost]
	//[Route("{personName}")]
	//public IActionResult Enqueue(string personName)
	//{
	//	backgroundJobClient.Enqueue(() => Test(personName));
	//	return Ok();
	//}

	[HttpPost]
	[Route("schedule/{personName}")]
	public IActionResult Schedule(string personName)
	{
		backgroundJobClient.Schedule(() => Console.WriteLine(personName), TimeSpan.FromSeconds(1));
		return Ok();
	}

	//public async Task Test(string name)
	//{
	//	await Task.Delay(100000);
	//	Console.WriteLine(name);
	//}
}

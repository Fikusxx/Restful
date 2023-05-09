using Hangfire;
using HangfireBasicAuthenticationFilter;
using Library.API.DbContexts;
using Library.API.Services;
using Library.Background;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddOptions<Person>()
	.Bind(configuration.GetSection("Person"))
	.ValidateDataAnnotations()
	.ValidateOnStart()
	.Validate(x =>
	{
		return x.Age != 50;
	});

//services.AddHttpCacheHeaders(
//	expiration =>
//{
//	expiration.MaxAge = 100;
//	expiration.CacheLocation = Marvin.Cache.Headers.CacheLocation.Public;
//},
//	validation =>
//{
//	validation.MustRevalidate = true;
//});
//services.AddResponseCaching();
services.AddHostedService<MyServiceBackground>();
services.AddControllers(options =>
{
	options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson(options =>
{
	options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
})
.AddXmlDataContractSerializerFormatters()
.ConfigureApiBehaviorOptions(options =>
{
	options.InvalidModelStateResponseFactory = context =>
	{
		var problemDetails = new ValidationProblemDetails(context.ModelState)
		{
			Type = "https://library.com/modelvalidationproblem",
			Title = "One or more validation errors occured",
			Status = StatusCodes.Status422UnprocessableEntity,
			Detail = "See the errors property or details",
			Instance = context.HttpContext.Request.Path
		};

		problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

		return new UnprocessableEntityObjectResult(problemDetails)
		{
			ContentTypes = { "application/json" }
		};
	};
});

// Default configuration from Hangfire docs
var connection = @"Server=(localdb)\mssqllocaldb;Database=Restful;Trusted_Connection=True;";
services.AddHangfire(config =>
{
	config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseSqlServerStorage(connection, new Hangfire.SqlServer.SqlServerStorageOptions()
	{
		CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
		SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
		QueuePollInterval = TimeSpan.Zero,
		UseRecommendedIsolationLevel = true,
		DisableGlobalLocks = true
	});
});
services.AddHangfireServer();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddScoped<ILibraryRepository, LibraryRepository>();
services.AddTransient<IPropertyMappingService, PropertyMappingService>();
services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
services.AddTransient<ITimeService, TimeService>();

services.AddDbContext<LibraryContext>(options =>
{
	options.UseSqlServer(
		@"Server=(localdb)\mssqllocaldb;Database=Restful;Trusted_Connection=True;");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler(builder =>
	{
		builder.Run(async context =>
		{
			context.Response.StatusCode = 500;
			await context.Response.WriteAsync("An error occured, sorry ^_^");
		});
	});
}

//app.UseResponseCaching();
//app.UseHttpCacheHeaders();
app.UseRouting();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
	DashboardTitle = "Home",
	Authorization = new[]
	{
		new HangfireCustomBasicAuthenticationFilter()
		{
			User = "Fikus",
			Pass = "Qwerty123"
		}
	}
});

RecurringJob.AddOrUpdate<ITimeService>("KEKW", service => service.PrintTime(), Cron.Minutely);

app.MapDefaultControllerRoute();

app.Run();

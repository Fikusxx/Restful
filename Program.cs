using Library.API.DbContexts;
using Library.API.Services;
using Library.Helpers;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;


services.AddHttpCacheHeaders(
	expiration =>
{
	expiration.MaxAge = 100;
	expiration.CacheLocation = Marvin.Cache.Headers.CacheLocation.Public;
},
	validation =>
{
	validation.MustRevalidate = true;
});
services.AddResponseCaching();
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

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddScoped<ILibraryRepository, LibraryRepository>();
services.AddTransient<IPropertyMappingService, PropertyMappingService>();
services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

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
app.UseHttpCacheHeaders();
app.UseRouting();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();

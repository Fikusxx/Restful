using Hangfire;
using HangfireBasicAuthenticationFilter;
using Library.API.DbContexts;
using Library.API.Services;
using Library.Background;
using Library.Caching;
using Library.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Text;

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
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			// указывает, будет ли валидироваться издатель при валидации токена
			ValidateIssuer = true,
			// строка, представляющая издателя
			ValidIssuer = "ISSUER",
			// будет ли валидироваться потребитель токена
			ValidateAudience = true,
			// установка потребителя токена
			ValidAudience = "AUDIENCE",
			// будет ли валидироваться время существования
			ValidateLifetime = true,
			// установка ключа безопасности
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("AWJHDAHDOAHDOAWPDHOAPWHDOAWdHOAAWDHO")),
			// валидация ключа безопасности
			ValidateIssuerSigningKey = true,
		};
	});

services.AddScoped<ILibraryRepository, LibraryRepository>();
services.AddTransient<IPropertyMappingService, PropertyMappingService>();
services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
services.AddTransient<ITimeService, TimeService>();
services.AddScoped<ICacheService, CacheService>();
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer
	.Connect("redis-10599.c239.us-east-1-2.ec2.cloud.redislabs.com:10599,password=oWLCpWVKhlUaAecUq3CPFMgULKQuS7K8"));

services.AddMediatR(opt => opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehaviour<,>));

var openGenericType = typeof(ICachingService<,>);
Assembly.GetExecutingAssembly().GetTypes()
	.Where(x => x.IsAbstract == false && x.IsGenericTypeDefinition == false)
	.ToList()
	.ForEach(type =>
{
	var interfaces = type.GetInterfaces();
	var genericInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == openGenericType);
	var matchingInterface = genericInterfaces.FirstOrDefault();

	if (matchingInterface != null)
	{
		builder.Services.AddTransient(matchingInterface, type);
	}
});
services.AddHttpContextAccessor();



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
app.UseAuthentication();
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

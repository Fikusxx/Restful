using Library.API.DbContexts;
using Library.API.Services;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddScoped<ILibraryRepository, LibraryRepository>();

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

//app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();

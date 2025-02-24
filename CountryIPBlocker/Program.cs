using CountryIPBlocker.Services.Interfaces;
using CountryIPBlocker.Services;
using CountryIPBlocker.BackgroundServices;
using System.Text.Json.Serialization;
using Polly;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});

// Configure Memory Cache
builder.Services.AddMemoryCache(options =>
{
	options.SizeLimit = 1024; // Set cache size limit to 1024 entries
	options.CompactionPercentage = 0.25; // Remove 25% of entries when size limit is reached
	options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Check for expired items every 5 minutes
});

// Register needed services
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<ICountryService, CountryService>();
builder.Services.AddScoped<IIPGeolocationService, IPGeolocationService>();

// Add httpclient for Geolocation service
builder.Services.AddHttpClient<IIPGeolocationService, IPGeolocationService>();

// Add background service for temporal blocks cleanup
builder.Services.AddHostedService<TemporalBlockCleanupService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "IP Validator API",
		Version = "v1",
		Description = "API for managing blocked countries and IP validation",
	});

	// Configure XML comments
	try
	{
		var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
		var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
		if (File.Exists(xmlPath))
		{
			c.IncludeXmlComments(xmlPath);
		}
	}
	catch
	{
		// XML documentation file not found, continue without it
	}
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger(c =>
	{
		c.SerializeAsV2 = false;
	});

	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "IP Validator API V1");
		c.RoutePrefix = string.Empty; // Serve the Swagger UI at the app's root
	});
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

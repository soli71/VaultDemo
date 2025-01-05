var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Development.json", optional: false);
}

// Production Vault configuration
else
{
    builder.Services.Configure<VaultSettings>(
        builder.Configuration.GetSection("Vault"));
    builder.Services.AddSingleton<VaultService>();
}

// Register secret provider
builder.Services.AddSingleton<ISecretProvider>(sp => 
    SecretProviderFactory.Create(sp, builder.Environment));

// Example DB context registration
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    var secretProvider = provider.GetRequiredService<ISecretProvider>();
    var connectionString = secretProvider.GetConnectionString("DefaultConnection").Result;
    options.UseSqlServer(connectionString);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

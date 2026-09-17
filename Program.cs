using Scalar.AspNetCore;
using Api.Domain.Feed;
using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add validation to minimal api endpoints via service extension: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/validation?view=aspnetcore-10.0
// Needs to be done like if endpoint mappings are not in the same assembly where 'AddValidation' is called
builder.Services.AddApiValidation();

// Configure exceptions
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppContext>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception switch
    {
        BadHttpRequestException => StatusCodes.Status400BadRequest,
        // Voit lisätä tähän muitakin:
        // KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    }
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
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

app.MapFeedEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
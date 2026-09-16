using Scalar.AspNetCore;
using Api.Domain.Feed;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure exceptions
builder.Services.AddProblemDetails();

var app = builder.Build();

// Show better status on request errors
// app.UseStatusCodePages(async statusCodeContext 
//     => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
//                  .ExecuteAsync(statusCodeContext.HttpContext));
// app.UseExceptionHandler(exceptoinAppHandler =>
// {
//     exceptoinAppHandler.Run(async context =>
//     {
//         var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
//         if (exceptionHandlerPathFeature?.Error is BadHttpRequestException)
//         {
//             context.Response.StatusCode = StatusCodes.Status400BadRequest;
//         }
//         await context.Response.CompleteAsync();
//     });
// });
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
// app.UseStatusCodePages();
// Configure exceptions
// app.UseExceptionHandler(exceptionHandlerApp 
//     => exceptionHandlerApp.Run(async context 
//         => await Results.Problem()
//                      .ExecuteAsync(context)));

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

// Controller.MapFeedEndpoints(app);
app.MapFeedEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

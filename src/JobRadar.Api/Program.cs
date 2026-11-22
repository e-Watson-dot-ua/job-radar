using JobRadar.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Unhandled exceptions and empty error responses become RFC 9457 ProblemDetails.
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapDefaultEndpoints();

app.Run();

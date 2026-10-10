var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("/api");

api.MapGet("info", () => new ApiInfo(
        "CpmProvViz.Api",
        typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown"))
    .WithName("GetApiInfo");

app.MapDefaultEndpoints();

app.Run();

record ApiInfo(string Name, string Version);

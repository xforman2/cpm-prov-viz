var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "CpmProvViz API v1"));
}

var api = app.MapGroup("/api");

api.MapGet("info", () => new ApiInfo(
        "CpmProvViz.Api",
        typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown"))
    .WithName("GetApiInfo")
    .WithSummary("Returns the API name and version.");

app.MapDefaultEndpoints();

app.Run();

record ApiInfo(string Name, string Version);

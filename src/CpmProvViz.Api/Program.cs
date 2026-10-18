using System.Text.Json;
using CpmProvViz.Application;
using CpmProvViz.Application.Graph;
using CpmProvViz.Infrastructure.ProvJson;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IProvDocumentReader, ProvJsonReader>();
builder.Services.AddSingleton<ReadGraph>();

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

api.MapPost("graph", async Task<IResult> (HttpRequest request, ReadGraph readGraph, CancellationToken cancellationToken) =>
    {
        try
        {
            return TypedResults.Ok(await readGraph.ExecuteAsync(request.Body, cancellationToken));
        }
        catch (UnreadableDocumentException e)
        {
            return TypedResults.Problem(
                title: "The document cannot be read.", detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    })
    .Accepts<JsonElement>("application/json")
    .Produces<Graph>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .WithName("ReadGraph")
    .WithSummary("Reads a PROV-JSON document and returns its graph (docs/graph-contract.md).");

app.MapDefaultEndpoints();

app.Run();

record ApiInfo(string Name, string Version);

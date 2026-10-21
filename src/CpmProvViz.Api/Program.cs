using System.Text.Json;
using System.Text.Json.Serialization;
using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Application.Graph.ReadGraph;
using CpmProvViz.Application.Ports;
using CpmProvViz.Infrastructure.ProvJson;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IProvDocumentReader, ProvJsonReader>();
builder.Services.AddSingleton<IReadGraphUseCase, ReadGraphUseCase>();

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

api.MapPost("graph", async Task<IResult> (
        HttpRequest request, IReadGraphUseCase readGraph, CancellationToken cancellationToken) =>
    {
        try
        {
            return TypedResults.Ok(await readGraph.ExecuteAsync(new ReadGraphRequest(request.Body), cancellationToken));
        }
        catch (UnreadableDocumentException e)
        {
            return TypedResults.Problem(
                title: "The document cannot be read.", detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    })
    .Accepts<JsonElement>("application/json", "*/*")
    .Produces<GraphDto>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .WithName("ReadGraph")
    .WithSummary("Reads a PROV-JSON document and returns its graph.")
    .WithDescription(
        "Takes the raw PROV-JSON document as the request body, whatever its content type. Nothing is " +
        "stored. The document is read leniently; it is rejected with 400 only when it is not JSON or not " +
        "shaped like a PROV-JSON document: its root is not an object; its root has keys but none is a " +
        "PROV-JSON section " +
        "(prefix, bundle, entity, activity, agent or a relation section); prefix, bundle, a bundle's " +
        "content or a known section is not an object; or a prefix maps to something other than a " +
        "string. An empty object {} is an empty document.");

app.MapDefaultEndpoints();

app.Run();

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

internal static class GraphApi
{
    public static Task<HttpResponseMessage> Post(this WebApplicationFactory<Program> factory, string document) =>
        factory.CreateClient().PostAsync(
            "/api/graph", new StringContent(document, Encoding.UTF8, "application/json"));

    /// <summary>Posts a document that must be read, and returns its graph.</summary>
    public static async Task<JsonNode> PostGraph(this WebApplicationFactory<Program> factory, string document)
    {
        var response = await factory.Post(document);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }
}

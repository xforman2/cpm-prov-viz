using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

/// <summary>
/// The graph contract is specified by the comments on its records, published in the OpenAPI document.
/// </summary>
public class ContractDocumentationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    public static TheoryData<string> ContractSchemas { get; } =
    [
        "Graph", "GraphBundle", "GraphElement", "GraphRelation", "GraphAttribute", "GraphAttributeValue",
        "ParseNote", "ValidationFinding",
    ];

    [Theory]
    [MemberData(nameof(ContractSchemas))]
    public async Task Every_contract_field_is_described(string name)
    {
        var schema = (await Schemas())[name]!;

        Assert.False(string.IsNullOrWhiteSpace((string?)schema["description"]), $"{name} has no description.");
        foreach (var (field, fieldSchema) in schema["properties"]!.AsObject())
        {
            Assert.False(
                string.IsNullOrWhiteSpace((string?)fieldSchema!["description"]), $"{name}.{field} has no description.");
        }
    }

    [Fact]
    public async Task Element_kinds_and_relation_types_are_listed_as_they_are_serialized()
    {
        var schemas = await Schemas();

        Assert.Equal(["entity", "activity", "agent"], Values(schemas["ElementKind"]!));
        Assert.Contains("wasGeneratedBy", Values(schemas["RelationType"]!));
        Assert.Contains("mentionOf", Values(schemas["RelationType"]!));
    }

    private async Task<JsonNode> Schemas() =>
        JsonNode.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json"))!["components"]!["schemas"]!;

    private static string[] Values(JsonNode enumSchema) =>
        [.. enumSchema["enum"]!.AsArray().Select(v => (string)v!)];
}

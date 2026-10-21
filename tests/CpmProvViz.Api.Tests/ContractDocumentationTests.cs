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
        "GraphDto", "GraphBundleDto", "GraphElementDto", "GraphRelationDto", "GraphAttributeDto", "GraphAttributeValueDto",
        "GraphParticipantDto", "ParseNoteDto", "ValidationFindingDto",
    ];

    [Theory]
    [MemberData(nameof(ContractSchemas))]
    public async Task Every_contract_field_is_described(string name)
    {
        var schema = (await Schemas())[name]!;

        Assert.False(string.IsNullOrWhiteSpace((string?)schema["description"]), $"{name} has no description.");
        foreach (var (field, fieldSchema) in schema["properties"]!.AsObject())
        {
            Assert.False(string.IsNullOrWhiteSpace(Description(fieldSchema!)), $"{name}.{field} has no description.");
        }
    }

    [Fact]
    public async Task Enum_values_are_listed_as_they_are_serialized()
    {
        var schemas = await Schemas();

        Assert.Equal(["entity", "activity", "agent"], Values(schemas["ElementKind"]!));
        Assert.Contains("wasGeneratedBy", Values(schemas["RelationType"]!));
        Assert.Contains("mentionOf", Values(schemas["RelationType"]!));
        Assert.Equal(
            ["mainActivity", "forwardConnector", "backwardConnector", "specForwardConnector", "senderAgent", "receiverAgent", "currentAgent"],
            Values(schemas["CpmRole"]!));
        Assert.Equal(["traversalInformation", "domainSpecificInformation"], Values(schemas["ElementMembership"]!));
        Assert.Equal(["traversalInformation", "domainSpecificInformation", "joining"], Values(schemas["RelationMembership"]!));
    }

    private async Task<JsonNode> Schemas() =>
        JsonNode.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json"))!["components"]!["schemas"]!;

    // A nullable field that refers to another schema is a oneOf of null and that schema, and its
    // description sits on the reference.
    private static string? Description(JsonNode fieldSchema) =>
        (string?)fieldSchema["description"]
        ?? fieldSchema["oneOf"]?.AsArray().Select(s => (string?)s!["description"]).FirstOrDefault(d => d is not null);

    // An enum used by a nullable field also lists null.
    private static string[] Values(JsonNode enumSchema) =>
        enumSchema["enum"]!.AsArray().OfType<JsonNode>().Select(v => v.GetValue<string>()).ToArray();
}

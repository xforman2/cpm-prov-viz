using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

/// <summary>
/// The graphs of a few test documents are committed under <c>testdata/contract/</c> as fixtures for
/// the frontend tests, so the two sides of the contract can't drift apart unnoticed. After an
/// intended change, review the diff and regenerate with <c>UPDATE_CONTRACT_FIXTURES=1 dotnet test</c>.
/// </summary>
public class ContractFixtureTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static TheoryData<string> Documents { get; } =
    [
        "embrc/dataset1/SamplingBundle_V0.json",
        "embrc/dataset2/ProcessingBundle_V1.json",
        "handmade/two-bundles.json",
    ];

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task The_graph_matches_the_committed_fixture(string document)
    {
        var graph = await factory.PostGraph(await File.ReadAllTextAsync(TestData.PathOf(document)));
        var fixture = TestData.PathOf(Path.Combine("contract", document));

        if (Environment.GetEnvironmentVariable("UPDATE_CONTRACT_FIXTURES") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fixture)!);
            await File.WriteAllTextAsync(fixture, graph.ToJsonString(Indented) + "\n");
        }

        Assert.True(File.Exists(fixture), $"Missing fixture {fixture}. Generate it with UPDATE_CONTRACT_FIXTURES=1.");
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(await File.ReadAllTextAsync(fixture)), graph),
            $"The graph of {document} differs from {fixture}. If the change is intended, regenerate the fixture with UPDATE_CONTRACT_FIXTURES=1.");
    }
}

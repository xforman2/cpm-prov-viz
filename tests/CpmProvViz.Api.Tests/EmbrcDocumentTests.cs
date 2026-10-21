using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

public class EmbrcDocumentTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string[] ElementSections = ["entity", "activity", "agent"];

    public static TheoryData<string> Documents { get; } = new(
        Directory.GetFiles(TestData.PathOf("embrc"), "*.json", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(TestData.Root, path))
            .Order());

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task Every_declared_element_and_relation_is_in_the_graph(string document)
    {
        var json = await File.ReadAllTextAsync(TestData.PathOf(document));
        var input = JsonNode.Parse(json)!;
        var graph = await factory.PostGraph(json);

        var (bundleId, content) = Assert.Single(input["bundle"]!.AsObject());
        var bundle = Assert.Single(graph["bundles"]!.AsArray())!;
        Assert.Equal(bundleId, (string?)bundle["id"]);

        var declaredElements = ElementSections
            .SelectMany(kind => content![kind]?.AsObject().Select(e => (kind, e.Key)) ?? [])
            .Order();
        var elements = graph["elements"]!.AsArray()
            .Select(e => ((string)e!["kind"]!, (string)e["id"]!))
            .Order();
        Assert.Equal(declaredElements, elements);

        var declaredRelations = content!.AsObject()
            .Where(s => s.Key != "prefix" && !ElementSections.Contains(s.Key))
            .SelectMany(s => s.Value!.AsObject().Select(_ => s.Key))
            .Order();
        var relations = graph["relations"]!.AsArray().Select(r => (string)r!["type"]!).Order();
        Assert.Equal(declaredRelations, relations);

        // Every relation in these documents names declared elements on both sides.
        Assert.All(graph["relations"]!.AsArray(), r =>
        {
            Assert.NotNull((string?)r!["source"]);
            Assert.NotNull((string?)r["target"]);
        });
    }

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task Each_document_is_one_provenance_component_with_one_main_activity(string document)
    {
        var graph = await factory.PostGraph(await File.ReadAllTextAsync(TestData.PathOf(document)));

        Assert.True((bool)Assert.Single(graph["bundles"]!.AsArray())!["isProvenanceComponent"]!);
        Assert.Single(graph["elements"]!.AsArray(), e => e!["cpmRoles"]!.AsArray().Any(r => (string?)r == "mainActivity"));
    }

    [Fact]
    public async Task Processing_v1_has_the_traversal_information_its_readme_describes()
    {
        var graph = await factory.PostGraph(await File.ReadAllTextAsync(TestData.PathOf("embrc/dataset2/ProcessingBundle_V1.json")));

        var traversal = graph["elements"]!.AsArray()
            .Where(e => (string?)e!["membership"] == "traversalInformation")
            .Select(e => ((string?)e!["id"], string.Join(' ', e["cpmRoles"]!.AsArray().Select(r => (string?)r))))
            .Order();
        Assert.Equal(
            [
                ("blank:NiceMarineStation", "senderAgent receiverAgent"),
                ("blank:ProcessedSampleCon", "forwardConnector"),
                ("blank:ProcessedSampleConSpec", "specForwardConnector"),
                ("blank:Processing", "mainActivity"),
                ("blank:StoredSampleCon_r1", "backwardConnector"),
            ],
            traversal);
        Assert.All(
            graph["elements"]!.AsArray().Where(e => (string?)e!["membership"] != "traversalInformation"),
            e =>
            {
                Assert.Equal("domainSpecificInformation", (string?)e!["membership"]);
                Assert.Empty(e["cpmRoles"]!.AsArray());
            });
        Assert.Contains(graph["relations"]!.AsArray(), r => (string?)r!["membership"] == "joining");
    }
}

using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

/// <summary>
/// CPM is interpreted, not validated: each element's CPM roles, which part of a provenance component
/// each element and relation belongs to, and which bundles are provenance components.
/// </summary>
public class CpmInterpretationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string CpmNamespace = "https://www.commonprovenancemodel.org/cpm-namespace-v1-0/";

    [Fact]
    public async Task Reads_cpm_roles_by_their_namespace_iri_whatever_the_prefix()
    {
        var graph = await PostGraph($$"""
            {
              "prefix": { "ex": "http://example.org/", "c": "{{CpmNamespace}}" },
              "bundle": {
                "ex:bundle1": {
                  "activity": { "ex:main": { "prov:type": { "$": "c:mainActivity", "type": "prov:QUALIFIED_NAME" } } },
                  "entity": {
                    "ex:connector": {
                      "prov:type": [
                        { "$": "ex:Sample", "type": "prov:QUALIFIED_NAME" },
                        { "$": "c:forwardConnector", "type": "xsd:QName" }
                      ]
                    },
                    "ex:record": { "prov:type": { "$": "c:id", "type": "prov:QUALIFIED_NAME" } }
                  },
                  "agent": {
                    "ex:station": {
                      "prov:type": [
                        { "$": "c:senderAgent", "type": "prov:QUALIFIED_NAME" },
                        { "$": "c:receiverAgent", "type": "prov:QUALIFIED_NAME" },
                        { "$": "c:senderAgent", "type": "prov:QUALIFIED_NAME" }
                      ]
                    }
                  }
                }
              }
            }
            """);

        Assert.True((bool)Single(graph["bundles"])["isProvenanceComponent"]!);
        Assert.Equal(
            [
                ("ex:connector", "forwardConnector", "traversalInformation"),
                ("ex:record", "", "domainSpecificInformation"),
                ("ex:main", "mainActivity", "traversalInformation"),
                ("ex:station", "senderAgent receiverAgent", "traversalInformation"),
            ],
            Interpretations(graph));
    }

    [Fact]
    public async Task Reads_an_older_cpm_namespace_and_untyped_strings_as_plain_prov()
    {
        // Any namespace but the current one stands in for an older CPM version.
        var graph = await PostGraph($$"""
            {
              "prefix": { "ex": "http://example.org/", "cpm": "{{CpmNamespace}}" },
              "activity": { "ex:topMain": { "prov:type": { "$": "cpm:mainActivity", "type": "prov:QUALIFIED_NAME" } } },
              "bundle": {
                "ex:older": {
                  "prefix": { "cpm": "https://www.commonprovenancemodel.org/cpm-namespace-v0-9/" },
                  "activity": { "ex:main": { "prov:type": { "$": "cpm:mainActivity", "type": "prov:QUALIFIED_NAME" } } }
                },
                "ex:untyped": {
                  "activity": { "ex:main": { "prov:type": "cpm:mainActivity" } },
                  "entity": { "ex:connector": { "prov:type": { "$": "cpm:forwardConnector", "type": "xsd:string" } } }
                }
              }
            }
            """);

        Assert.All(graph["bundles"]!.AsArray(), b => Assert.False((bool)b!["isProvenanceComponent"]!));
        Assert.Equal(
            [("ex:topMain", "mainActivity", null), ("ex:main", "", null), ("ex:connector", "", null), ("ex:main", "", null)],
            Interpretations(graph));
    }

    [Fact]
    public async Task Sorts_a_provenance_components_relations_by_the_membership_of_the_elements_they_join()
    {
        var graph = await PostGraph($$"""
            {
              "prefix": { "ex": "http://example.org/", "cpm": "{{CpmNamespace}}" },
              "entity": { "ex:top": {}, "ex:topToo": {} },
              "wasDerivedFrom": { "_:top": { "prov:generatedEntity": "ex:top", "prov:usedEntity": "ex:topToo" } },
              "bundle": {
                "ex:bundle1": {
                  "activity": { "ex:main": { "prov:type": { "$": "cpm:mainActivity", "type": "prov:QUALIFIED_NAME" } } },
                  "entity": {
                    "ex:connector": { "prov:type": { "$": "cpm:forwardConnector", "type": "prov:QUALIFIED_NAME" } },
                    "ex:sample": {},
                    "ex:reading": {}
                  },
                  "wasGeneratedBy": {
                    "_:g1": { "prov:entity": "ex:connector", "prov:activity": "ex:main" },
                    "_:g2": { "prov:entity": "ex:sample", "prov:activity": "ex:main" },
                    "_:g3": { "prov:entity": "ex:connector" },
                    "_:g4": { "prov:entity": "ex:sample" },
                    "_:g5": { "prov:entity": "ex:elsewhere", "prov:activity": "ex:main" }
                  },
                  "wasDerivedFrom": {
                    "_:d1": { "prov:generatedEntity": "ex:reading", "prov:usedEntity": "ex:sample", "prov:activity": "ex:main" }
                  }
                }
              }
            }
            """);

        Assert.Equal(
            [null, "traversalInformation", "joining", "traversalInformation", "domainSpecificInformation",
             "traversalInformation", "domainSpecificInformation"],
            graph["relations"]!.AsArray().Select(r => (string?)r!["membership"]));
    }

    private Task<JsonNode> PostGraph(string document) => factory.PostGraph(document);

    private static JsonNode Single(JsonNode? array) => Assert.Single(array!.AsArray())!;

    // Each element's identifier, its CPM roles separated by spaces, and its membership.
    private static IEnumerable<(string?, string, string?)> Interpretations(JsonNode graph) =>
        graph["elements"]!.AsArray().Select(e => (
            (string?)e!["id"],
            string.Join(' ', e["cpmRoles"]!.AsArray().Select(r => (string?)r)),
            (string?)e["membership"]));
}

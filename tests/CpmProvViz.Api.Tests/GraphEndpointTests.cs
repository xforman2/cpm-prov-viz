using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CpmProvViz.Api.Tests;

public class GraphEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Reads_an_entity_inside_a_bundle()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "bundle": {
                "ex:bundle1": {
                  "entity": { "ex:sample": {} }
                }
              }
            }
            """);

        var bundle = Single(graph["bundles"]);
        Assert.Equal("ex:bundle1", (string?)bundle["id"]);
        Assert.Equal("http://example.org/bundle1", (string?)bundle["iri"]);

        var element = Single(graph["elements"]);
        Assert.Equal("ex:sample", (string?)element["id"]);
        Assert.Equal("http://example.org/sample", (string?)element["iri"]);
        Assert.Equal("entity", (string?)element["kind"]);
        Assert.Equal((string?)bundle["key"], (string?)element["bundle"]);
    }

    [Fact]
    public async Task Reads_a_relation_as_an_edge_from_its_first_to_its_second_argument()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "bundle": {
                "ex:bundle1": {
                  "entity": { "ex:report": {} },
                  "activity": { "ex:writing": {} },
                  "wasGeneratedBy": {
                    "_:g1": { "prov:entity": "ex:report", "prov:activity": "ex:writing" }
                  },
                  "used": {
                    "ex:u1": { "prov:activity": "ex:writing", "prov:entity": "ex:report" }
                  }
                }
              }
            }
            """);

        var bundleKey = (string?)Single(graph["bundles"])["key"];
        var report = ElementKey(graph, "ex:report");
        var writing = ElementKey(graph, "ex:writing");
        var relations = graph["relations"]!.AsArray();
        Assert.Equal(2, relations.Count);

        var generation = relations.Single(r => (string?)r!["type"] == "wasGeneratedBy")!;
        Assert.Null(generation["id"]);
        Assert.Null(generation["iri"]);
        Assert.Equal(bundleKey, (string?)generation["bundle"]);
        Assert.Equal(report, (string?)generation["source"]);
        Assert.Equal(writing, (string?)generation["target"]);

        var usage = relations.Single(r => (string?)r!["type"] == "used")!;
        Assert.Equal("ex:u1", (string?)usage["id"]);
        Assert.Equal("http://example.org/u1", (string?)usage["iri"]);
        Assert.Equal(writing, (string?)usage["source"]);
        Assert.Equal(report, (string?)usage["target"]);
    }

    [Theory]
    [InlineData("wasGeneratedBy", "prov:entity", "prov:activity")]
    [InlineData("used", "prov:activity", "prov:entity")]
    [InlineData("wasInformedBy", "prov:informed", "prov:informant")]
    [InlineData("wasStartedBy", "prov:activity", "prov:trigger")]
    [InlineData("wasEndedBy", "prov:activity", "prov:trigger")]
    [InlineData("wasInvalidatedBy", "prov:entity", "prov:activity")]
    [InlineData("wasDerivedFrom", "prov:generatedEntity", "prov:usedEntity")]
    [InlineData("wasAttributedTo", "prov:entity", "prov:agent")]
    [InlineData("wasAssociatedWith", "prov:activity", "prov:agent")]
    [InlineData("actedOnBehalfOf", "prov:delegate", "prov:responsible")]
    [InlineData("wasInfluencedBy", "prov:influencee", "prov:influencer")]
    [InlineData("specializationOf", "prov:specificEntity", "prov:generalEntity")]
    [InlineData("alternateOf", "prov:alternate1", "prov:alternate2")]
    [InlineData("hadMember", "prov:collection", "prov:entity")]
    [InlineData("mentionOf", "prov:specificEntity", "prov:generalEntity")]
    public async Task Takes_source_and_target_from_the_relations_two_main_arguments(
        string type, string sourceKey, string targetKey)
    {
        var graph = await PostGraph($$"""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": { "ex:a": {}, "ex:b": {} },
              "{{type}}": { "_:r": { "{{targetKey}}": "ex:b", "{{sourceKey}}": "ex:a" } }
            }
            """);

        var relation = Single(graph["relations"]);
        Assert.Equal(type, (string?)relation["type"]);
        Assert.Equal(ElementKey(graph, "ex:a"), (string?)relation["source"]);
        Assert.Equal(ElementKey(graph, "ex:b"), (string?)relation["target"]);
    }

    [Fact]
    public async Task Reads_top_level_statements_outside_any_bundle()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": { "ex:bundle1": { "prov:type": { "$": "prov:Bundle", "type": "prov:QUALIFIED_NAME" } } },
              "agent": { "ex:alice": {} },
              "wasAttributedTo": { "_:a1": { "prov:entity": "ex:bundle1", "prov:agent": "ex:alice" } },
              "bundle": {
                "ex:bundle1": { "entity": { "ex:report": {} } }
              }
            }
            """);

        var bundle = (string?)Single(graph["bundles"])["key"];
        var elements = graph["elements"]!.AsArray();
        Assert.Equal(3, elements.Count);
        Assert.Null(Element(graph, "ex:bundle1")["bundle"]);
        Assert.Null(Element(graph, "ex:alice")["bundle"]);
        Assert.Equal("agent", (string?)Element(graph, "ex:alice")["kind"]);
        Assert.Equal(bundle, (string?)Element(graph, "ex:report")["bundle"]);

        var attribution = Single(graph["relations"]);
        Assert.Null(attribution["bundle"]);
        Assert.Equal(ElementKey(graph, "ex:bundle1"), (string?)attribution["source"]);
        Assert.Equal(ElementKey(graph, "ex:alice"), (string?)attribution["target"]);
    }

    [Fact]
    public async Task Leaves_an_endpoint_empty_when_it_is_missing_or_names_no_element_in_the_bundle()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": { "ex:outside": {} },
              "bundle": {
                "ex:bundle1": {
                  "activity": { "ex:writing": {} },
                  "used": {
                    "_:u1": { "prov:activity": "ex:writing" },
                    "_:u2": { "prov:activity": "ex:writing", "prov:entity": "ex:outside" }
                  }
                }
              }
            }
            """);

        var relations = graph["relations"]!.AsArray();
        Assert.Equal(2, relations.Count);
        Assert.All(relations, r =>
        {
            Assert.Equal(ElementKey(graph, "ex:writing"), (string?)r!["source"]);
            Assert.Null(r["target"]);
        });
    }

    [Fact]
    public async Task Resolves_a_bundle_with_its_own_prefixes_first_then_the_documents()
    {
        var graph = await PostGraph("""
            {
              "prefix": {
                "doc": "http://example.org/document/",
                "ex": "http://example.org/shadowed/"
              },
              "entity": { "ex:top": {} },
              "bundle": {
                "ex:bundle1": {
                  "prefix": { "ex": "http://example.org/bundle/", "default": "http://example.org/default/" },
                  "entity": { "ex:a": {}, "doc:b": {}, "c": {} },
                  "wasDerivedFrom": { "_:d1": { "prov:generatedEntity": "ex:a", "prov:usedEntity": "doc:b" } }
                }
              }
            }
            """);

        Assert.Equal(
            """{"doc":"http://example.org/document/","ex":"http://example.org/shadowed/"}""",
            graph["prefixes"]!.ToJsonString());
        var bundle = Single(graph["bundles"]);
        Assert.Equal("http://example.org/bundle/bundle1", (string?)bundle["iri"]);
        Assert.Equal(
            """{"ex":"http://example.org/bundle/","default":"http://example.org/default/"}""",
            bundle["prefixes"]!.ToJsonString());

        Assert.Equal("http://example.org/shadowed/top", (string?)Element(graph, "ex:top")["iri"]);
        Assert.Equal("http://example.org/bundle/a", (string?)Element(graph, "ex:a")["iri"]);
        Assert.Equal("http://example.org/document/b", (string?)Element(graph, "doc:b")["iri"]);
        Assert.Equal("http://example.org/default/c", (string?)Element(graph, "c")["iri"]);

        var derivation = Single(graph["relations"]);
        Assert.Equal(ElementKey(graph, "ex:a"), (string?)derivation["source"]);
        Assert.Equal(ElementKey(graph, "doc:b"), (string?)derivation["target"]);
    }

    [Fact]
    public async Task Knows_prov_and_xsd_and_keeps_an_undeclared_prefix_as_written()
    {
        var graph = await PostGraph("""
            {
              "entity": { "prov:thing": {}, "xsd:thing": {}, "nope:thing": {}, "http://example.org/thing": {} }
            }
            """);

        Assert.Equal("http://www.w3.org/ns/prov#thing", (string?)Element(graph, "prov:thing")["iri"]);
        Assert.Equal("http://www.w3.org/2001/XMLSchema#thing", (string?)Element(graph, "xsd:thing")["iri"]);
        Assert.Equal("nope:thing", (string?)Element(graph, "nope:thing")["iri"]);
        Assert.Equal("http://example.org/thing", (string?)Element(graph, "http://example.org/thing")["iri"]);
    }

    [Fact]
    public async Task Reads_attribute_values_given_bare_or_as_an_array()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": {
                "ex:sample": {
                  "prov:label": "Water sample",
                  "ex:depth": 10.5,
                  "ex:filtered": true,
                  "ex:count": { "$": "1034", "type": "xsd:positiveInteger" },
                  "ex:place": { "$": "Londres", "lang": "fr" },
                  "prov:type": [
                    { "$": "ex:Sample", "type": "prov:QUALIFIED_NAME" },
                    "a sample",
                    2
                  ]
                }
              }
            }
            """);

        Assert.Equal(
            """
            [{"name":"prov:label","iri":"http://www.w3.org/ns/prov#label","values":[{"value":"Water sample"}]},{"name":"ex:depth","iri":"http://example.org/depth","values":[{"value":"10.5"}]},{"name":"ex:filtered","iri":"http://example.org/filtered","values":[{"value":"true"}]},{"name":"ex:count","iri":"http://example.org/count","values":[{"value":"1034"}]},{"name":"ex:place","iri":"http://example.org/place","values":[{"value":"Londres"}]},{"name":"prov:type","iri":"http://www.w3.org/ns/prov#type","values":[{"value":"ex:Sample"},{"value":"a sample"},{"value":"2"}]}]
            """,
            Element(graph, "ex:sample")["attributes"]!.ToJsonString());
    }

    [Fact]
    public async Task Lists_every_key_of_a_relation_except_its_two_main_arguments_as_attributes()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": { "ex:a": {}, "ex:b": {} },
              "wasDerivedFrom": {
                "_:d1": {
                  "prov:generatedEntity": "ex:a",
                  "prov:usedEntity": "ex:b",
                  "prov:activity": "ex:copying",
                  "prov:type": [{ "$": "prov:Revision", "type": "xsd:QName" }]
                }
              }
            }
            """);

        var attributes = Single(graph["relations"])["attributes"]!.AsArray();
        Assert.Equal(["prov:activity", "prov:type"], attributes.Select(a => (string?)a!["name"]));
        Assert.Equal("ex:copying", (string?)attributes[0]!["values"]![0]!["value"]);
        Assert.Equal("prov:Revision", (string?)attributes[1]!["values"]![0]!["value"]);
    }

    [Fact]
    public async Task Keeps_a_main_argument_that_is_not_a_qualified_name_string_as_an_attribute()
    {
        var graph = await PostGraph("""
            {
              "prefix": { "ex": "http://example.org/" },
              "entity": { "ex:collection": {}, "ex:a": {}, "ex:b": {} },
              "hadMember": { "_:m1": { "prov:collection": "ex:collection", "prov:entity": ["ex:a", "ex:b"] } }
            }
            """);

        var membership = Single(graph["relations"]);
        Assert.Equal(ElementKey(graph, "ex:collection"), (string?)membership["source"]);
        Assert.Null(membership["target"]);
        Assert.Equal(
            """[{"name":"prov:entity","iri":"http://www.w3.org/ns/prov#entity","values":[{"value":"ex:a"},{"value":"ex:b"}]}]""",
            membership["attributes"]!.ToJsonString());
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{ "entity": """)]
    [InlineData("""[{ "entity": {} }]""")]
    [InlineData("\"entity\"")]
    [InlineData("""{ "name": "a shopping list", "items": ["milk"] }""")]
    [InlineData("""{ "entity": ["ex:sample"] }""")]
    [InlineData("""{ "prefix": "ex" }""")]
    [InlineData("""{ "prefix": { "ex": 1 } }""")]
    [InlineData("""{ "bundle": [] }""")]
    [InlineData("""{ "bundle": { "ex:b": "nothing" } }""")]
    [InlineData("""{ "bundle": { "ex:b": { "wasGeneratedBy": true } } }""")]
    public async Task Rejects_input_that_is_not_a_prov_json_document(string input)
    {
        var response = await factory.Post(input);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(400, (int?)problem["status"]);
        Assert.False(string.IsNullOrWhiteSpace((string?)problem["detail"]));
    }

    [Fact]
    public async Task Reads_an_empty_object_as_an_empty_document()
    {
        var graph = await PostGraph("{}");

        Assert.Empty(graph["bundles"]!.AsArray());
        Assert.Empty(graph["elements"]!.AsArray());
        Assert.Empty(graph["parseNotes"]!.AsArray());
        Assert.Empty(graph["validationFindings"]!.AsArray());
    }

    private Task<JsonNode> PostGraph(string document) => factory.PostGraph(document);

    private static JsonNode Single(JsonNode? array) => Assert.Single(array!.AsArray())!;

    private static JsonNode Element(JsonNode graph, string id) =>
        Assert.Single(graph["elements"]!.AsArray(), e => (string?)e!["id"] == id)!;

    private static string? ElementKey(JsonNode graph, string id) => (string?)Element(graph, id)["key"];
}

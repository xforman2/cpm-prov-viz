using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Domain.Prov;
using Attribute = CpmProvViz.Domain.Prov.Attribute;

namespace CpmProvViz.Application.Graph.ReadGraph;

/// <summary>
/// Builds the graph of one document, keying bundles, elements and relations in reading order.
/// Create one per document: it is not shared between calls.
/// </summary>
internal sealed class GraphBuilder
{
    private readonly List<GraphBundleDto> bundles = [];
    private readonly List<GraphElementDto> elements = [];
    private readonly List<GraphRelationDto> relations = [];

    /// <summary>Adds the document's statements that sit outside any bundle.</summary>
    public void AddTopLevel(ProvDocument document) =>
        AddStatements(document.Elements, document.Relations, bundleKey: null);

    /// <summary>Adds a bundle and its statements.</summary>
    public void AddBundle(Bundle bundle)
    {
        var key = $"b{bundles.Count + 1}";
        bundles.Add(new GraphBundleDto(key, bundle.Id.Text, bundle.Id.Iri, bundle.Prefixes));
        AddStatements(bundle.Elements, bundle.Relations, key);
    }

    public GraphDto Build(IReadOnlyDictionary<string, string> prefixes) =>
        new(prefixes, bundles, elements, relations, [], []);

    // Statements in one bundle (or at the top level) refer only to elements in the same place:
    // PROV handles every bundle independently.
    private void AddStatements(
        IReadOnlyList<Element> scopeElements, IReadOnlyList<Relation> scopeRelations, string? bundleKey)
    {
        var keysByIri = new Dictionary<string, string>();
        foreach (var element in scopeElements)
        {
            var key = $"e{elements.Count + 1}";
            keysByIri.TryAdd(element.Id.Iri, key);
            elements.Add(new GraphElementDto(
                key, element.Id.Text, element.Id.Iri, element.Kind, bundleKey, Attributes(element.Attributes)));
        }

        foreach (var relation in scopeRelations)
        {
            relations.Add(new GraphRelationDto(
                $"r{relations.Count + 1}",
                relation.Type,
                relation.Id?.Text,
                relation.Id?.Iri,
                bundleKey,
                ElementKey(keysByIri, relation.Source),
                ElementKey(keysByIri, relation.Target),
                Attributes(relation.Attributes)));
        }
    }

    private static string? ElementKey(IReadOnlyDictionary<string, string> keysByIri, QualifiedName? name) =>
        name is not null && keysByIri.TryGetValue(name.Iri, out var key) ? key : null;

    private static List<GraphAttributeDto> Attributes(IReadOnlyList<Attribute> attributes) =>
        [.. attributes.Select(a => new GraphAttributeDto(
            a.Name.Text, a.Name.Iri, [.. a.Values.Select(v => new GraphAttributeValueDto(v.Value))]))];
}

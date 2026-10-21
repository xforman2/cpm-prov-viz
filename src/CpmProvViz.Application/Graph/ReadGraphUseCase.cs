using CpmProvViz.Domain.Prov;
using Attribute = CpmProvViz.Domain.Prov.Attribute;

namespace CpmProvViz.Application.Graph;

/// <inheritdoc />
public sealed class ReadGraphUseCase(IProvDocumentReader reader) : IReadGraphUseCase
{
    public async Task<Graph> ExecuteAsync(Stream input, CancellationToken cancellationToken)
    {
        var document = await reader.ReadAsync(input, cancellationToken);
        var bundles = new List<GraphBundle>();
        var elements = new List<GraphElement>();
        var relations = new List<GraphRelation>();

        AddStatements(document.Elements, document.Relations, bundle: null);
        foreach (var bundle in document.Bundles)
        {
            var key = $"b{bundles.Count + 1}";
            bundles.Add(new GraphBundle(key, bundle.Id.Text, bundle.Id.Iri, bundle.Prefixes));
            AddStatements(bundle.Elements, bundle.Relations, key);
        }

        return new Graph(document.Prefixes, bundles, elements, relations, [], []);

        // Statements in one bundle (or at the top level) refer only to elements in the same place:
        // PROV handles every bundle independently.
        void AddStatements(IReadOnlyList<Element> scopeElements, IReadOnlyList<Relation> scopeRelations, string? bundle)
        {
            var keysByIri = new Dictionary<string, string>();
            foreach (var element in scopeElements)
            {
                var key = $"e{elements.Count + 1}";
                keysByIri.TryAdd(element.Id.Iri, key);
                elements.Add(new GraphElement(
                    key, element.Id.Text, element.Id.Iri, element.Kind, bundle, Attributes(element.Attributes)));
            }

            foreach (var relation in scopeRelations)
            {
                relations.Add(new GraphRelation(
                    $"r{relations.Count + 1}",
                    relation.Type,
                    relation.Id?.Text,
                    relation.Id?.Iri,
                    bundle,
                    ElementKey(relation.Source),
                    ElementKey(relation.Target),
                    Attributes(relation.Attributes)));
            }

            string? ElementKey(QualifiedName? name) =>
                name is not null && keysByIri.TryGetValue(name.Iri, out var key) ? key : null;
        }
    }

    private static List<GraphAttribute> Attributes(IReadOnlyList<Attribute> attributes) =>
        [.. attributes.Select(a => new GraphAttribute(
            a.Name.Text, a.Name.Iri, [.. a.Values.Select(v => new GraphAttributeValue(v.Value))]))];
}

using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Application.Ports;
using CpmProvViz.Domain.Prov;
using Attribute = CpmProvViz.Domain.Prov.Attribute;

namespace CpmProvViz.Application.Graph.ReadGraph;

/// <inheritdoc />
public sealed class ReadGraphUseCase(IProvDocumentReader reader) : IReadGraphUseCase
{
    public async Task<GraphDto> ExecuteAsync(ReadGraphRequest request, CancellationToken cancellationToken)
    {
        var document = await reader.ReadAsync(request.Document, cancellationToken);
        var bundles = new List<GraphBundleDto>();
        var elements = new List<GraphElementDto>();
        var relations = new List<GraphRelationDto>();

        AddStatements(document.Elements, document.Relations, bundle: null);
        foreach (var bundle in document.Bundles)
        {
            var key = $"b{bundles.Count + 1}";
            bundles.Add(new GraphBundleDto(key, bundle.Id.Text, bundle.Id.Iri, bundle.Prefixes));
            AddStatements(bundle.Elements, bundle.Relations, key);
        }

        return new GraphDto(document.Prefixes, bundles, elements, relations, [], []);

        // Statements in one bundle (or at the top level) refer only to elements in the same place:
        // PROV handles every bundle independently.
        void AddStatements(IReadOnlyList<Element> scopeElements, IReadOnlyList<Relation> scopeRelations, string? bundle)
        {
            var keysByIri = new Dictionary<string, string>();
            foreach (var element in scopeElements)
            {
                var key = $"e{elements.Count + 1}";
                keysByIri.TryAdd(element.Id.Iri, key);
                elements.Add(new GraphElementDto(
                    key, element.Id.Text, element.Id.Iri, element.Kind, bundle, Attributes(element.Attributes)));
            }

            foreach (var relation in scopeRelations)
            {
                relations.Add(new GraphRelationDto(
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

    private static List<GraphAttributeDto> Attributes(IReadOnlyList<Attribute> attributes) =>
        [.. attributes.Select(a => new GraphAttributeDto(
            a.Name.Text, a.Name.Iri, [.. a.Values.Select(v => new GraphAttributeValueDto(v.Value))]))];
}

using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Domain.Cpm;
using CpmProvViz.Domain.Prov;
using Attribute = CpmProvViz.Domain.Prov.Attribute;

namespace CpmProvViz.Application.Graph.ReadGraph;

/// <summary>
/// Builds the graph of one document, keying bundles, elements and relations in reading order.
/// Create one per document: it is not shared between calls.
/// </summary>
internal sealed class GraphBuilder(ICpmInterpreter cpm)
{
    private const string BundleKeyPrefix = "b";
    private const string ElementKeyPrefix = "e";
    private const string RelationKeyPrefix = "r";

    private readonly List<GraphBundleDto> bundles = [];
    private readonly List<GraphElementDto> elements = [];
    private readonly List<GraphRelationDto> relations = [];

    /// <summary>Adds the document's statements that sit outside any bundle.</summary>
    public void AddTopLevel(ProvDocument document) =>
        AddStatements(document.Elements, document.Relations, bundleKey: null, isProvenanceComponent: false);

    /// <summary>Adds a bundle and its statements.</summary>
    public void AddBundle(Bundle bundle)
    {
        var key = Key(BundleKeyPrefix, bundles.Count);
        var isProvenanceComponent = cpm.IsProvenanceComponent(bundle);
        bundles.Add(new GraphBundleDto(key, bundle.Id.Text, bundle.Id.Iri, bundle.Prefixes, isProvenanceComponent));
        AddStatements(bundle.Elements, bundle.Relations, key, isProvenanceComponent);
    }

    public GraphDto Build(IReadOnlyDictionary<string, string> prefixes) =>
        new(prefixes, bundles, elements, relations, [], []);

    // Statements in one bundle (or at the top level) refer only to elements in the same place:
    // PROV handles every bundle independently. Only a provenance component's statements have a
    // membership.
    private void AddStatements(
        IReadOnlyList<Element> scopeElements,
        IReadOnlyList<Relation> scopeRelations,
        string? bundleKey,
        bool isProvenanceComponent)
    {
        var elementKeys = KeysByIri(scopeElements.Select(e => e.Id), ElementKeyPrefix, elements.Count);
        var relationKeys = KeysByIri(scopeRelations.Select(r => r.Id), RelationKeyPrefix, relations.Count);
        var memberships = new Dictionary<string, ElementMembership>();
        foreach (var element in scopeElements)
        {
            var roles = cpm.Roles(element);
            var membership = isProvenanceComponent ? cpm.Membership(roles) : (ElementMembership?)null;
            if (membership is { } known)
            {
                memberships.TryAdd(element.Id.Iri, known);
            }

            elements.Add(new GraphElementDto(
                Key(ElementKeyPrefix, elements.Count),
                element.Id.Text,
                element.Id.Iri,
                element.Kind,
                bundleKey,
                Attributes(element.Attributes, elementKeys),
                roles,
                membership));
        }

        foreach (var relation in scopeRelations)
        {
            relations.Add(new GraphRelationDto(
                Key(RelationKeyPrefix, relations.Count),
                relation.Type,
                relation.Id?.Text,
                relation.Id?.Iri,
                bundleKey,
                KeyOf(elementKeys, relation.Source),
                KeyOf(elementKeys, relation.Target),
                relation.Participants.Select(p => Participant(p, elementKeys, relationKeys)).ToList(),
                Attributes(relation.Attributes, elementKeys),
                isProvenanceComponent
                    ? cpm.Membership(MembershipOf(memberships, relation.Source), MembershipOf(memberships, relation.Target))
                    : null));
        }
    }

    // The key of the statement that comes after the given number of already keyed ones.
    private static string Key(string prefix, int keyed) => $"{prefix}{keyed + 1}";

    // Maps each identifier's IRI to the key its statement will get, the first one when it repeats.
    private static Dictionary<string, string> KeysByIri(IEnumerable<QualifiedName?> ids, string prefix, int keyed)
    {
        var keys = new Dictionary<string, string>();
        foreach (var id in ids)
        {
            if (id is not null)
            {
                keys.TryAdd(id.Iri, Key(prefix, keyed));
            }

            keyed++;
        }

        return keys;
    }

    private static ElementMembership? MembershipOf(
        IReadOnlyDictionary<string, ElementMembership> membershipsByIri, QualifiedName? name) =>
        name is not null && membershipsByIri.TryGetValue(name.Iri, out var membership) ? membership : null;

    private static string? KeyOf(IReadOnlyDictionary<string, string> keysByIri, QualifiedName? name) =>
        name is not null && keysByIri.TryGetValue(name.Iri, out var key) ? key : null;

    // A participant names an element, or else a relation, as prov:generation and prov:usage do.
    private static GraphParticipantDto Participant(
        Participant participant,
        IReadOnlyDictionary<string, string> elementKeys,
        IReadOnlyDictionary<string, string> relationKeys)
    {
        var element = KeyOf(elementKeys, participant.Id);
        return new GraphParticipantDto(
            participant.Name.Text,
            participant.Name.Iri,
            participant.Id.Text,
            participant.Id.Iri,
            element,
            element is null ? KeyOf(relationKeys, participant.Id) : null);
    }

    private static List<GraphAttributeDto> Attributes(
        IReadOnlyList<Attribute> attributes, IReadOnlyDictionary<string, string> elementKeys) =>
        attributes
            .Select(a => new GraphAttributeDto(
                a.Name.Text, a.Name.Iri, a.Values.Select(v => Value(v, elementKeys)).ToList()))
            .ToList();

    private static GraphAttributeValueDto Value(AttributeValue value, IReadOnlyDictionary<string, string> elementKeys) =>
        new(
            value.Value,
            value.Datatype?.Text,
            value.Datatype?.Iri,
            value.Language,
            value.QualifiedName?.Iri,
            KeyOf(elementKeys, value.QualifiedName));
}

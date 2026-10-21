namespace CpmProvViz.Application.Graph.Contract;

// The graph contract between the backend and the frontend: this record and the …Dto records it
// holds. Their comments are its specification, published in the OpenAPI document. Change them
// first, then the frontend.

/// <summary>
/// The graph of a PROV document, as the frontend draws it. It describes meaning only, never styling.
/// Everything refers to a bundle, element or relation by its key, never by its identifier: the same
/// identifier can appear in several bundles (a connector does exactly that), and a relation can have
/// no identifier at all. Keys are unique in the graph and the same every time the same document is
/// read: bundles are keyed <c>b1</c>, <c>b2</c>, …, elements <c>e1</c>, <c>e2</c>, … and relations
/// <c>r1</c>, <c>r2</c>, …, in reading order: the top-level statements first, then each bundle in
/// turn, and within each the entities, activities and agents, then the relations section by section.
/// An <c>id</c> is a qualified name as written in the document, e.g. <c>blank:Sampling</c>. Its
/// <c>iri</c> is the namespace of its prefix followed by the local part, split at the first <c>:</c>;
/// a name without a <c>:</c> uses the <c>default</c> prefix. Prefixes are looked up in the bundle's
/// own <c>prefix</c> (for a bundle's identifier and everything inside it), then the document's
/// <c>prefix</c>, then the implicit <c>prov</c> and <c>xsd</c>. If the prefix is declared nowhere,
/// the <c>iri</c> is the identifier as written.
/// </summary>
/// <param name="Prefixes">The document's own prefix declarations, prefix → namespace IRI, as written.</param>
/// <param name="Bundles">One entry per bundle in the document. Bundles never nest.</param>
/// <param name="Elements">One entry per entity, activity and agent, from every bundle and from the top level of the document.</param>
/// <param name="Relations">One entry per relation record, from every bundle and from the top level of the document.</param>
/// <param name="ParseNotes">Remarks from reading the document leniently. Always empty for now.</param>
/// <param name="ValidationFindings">Violations of PROV-DM or CPM rules. The validators are not built yet, so this list is always empty.</param>
public sealed record GraphDto(
    IReadOnlyDictionary<string, string> Prefixes,
    IReadOnlyList<GraphBundleDto> Bundles,
    IReadOnlyList<GraphElementDto> Elements,
    IReadOnlyList<GraphRelationDto> Relations,
    IReadOnlyList<ParseNoteDto> ParseNotes,
    IReadOnlyList<ValidationFindingDto> ValidationFindings);

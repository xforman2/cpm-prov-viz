using CpmProvViz.Domain.Cpm;
using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Graph.Contract;

/// <summary>An entity, activity or agent.</summary>
/// <param name="Key">Key of the element.</param>
/// <param name="Id">Identifier as written.</param>
/// <param name="Iri">Expanded IRI.</param>
/// <param name="Kind">PROV element kind.</param>
/// <param name="Bundle">Key of the bundle holding the element, or <c>null</c> for a top-level element.</param>
/// <param name="Attributes">The element's attributes, in document order.</param>
/// <param name="CpmRoles">The element's CPM roles, in the order of its <c>prov:type</c> values, each once. A <c>prov:type</c> value is a CPM role only when it is a qualified name (see <c>iri</c> of an attribute value) whose IRI is <c>https://www.commonprovenancemodel.org/cpm-namespace-v1-0/</c> followed by the role's name, whatever prefix the document writes for it. Other names in that namespace, such as <c>cpm:id</c>, are not roles, and neither is a plain string such as <c>"cpm:mainActivity"</c>. Older CPM namespaces are not interpreted. Empty when the element has none.</param>
/// <param name="Membership">For an element in a provenance component, <c>traversalInformation</c> when it has at least one CPM role, whatever other types it has, otherwise <c>domainSpecificInformation</c>. <c>null</c> for an element outside any provenance component, at the top level or in a bundle that is not one.</param>
public sealed record GraphElementDto(
    string Key,
    string Id,
    string Iri,
    ElementKind Kind,
    string? Bundle,
    IReadOnlyList<GraphAttributeDto> Attributes,
    IReadOnlyList<CpmRole> CpmRoles,
    ElementMembership? Membership);

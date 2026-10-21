using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Graph.Contract;

/// <summary>An entity, activity or agent.</summary>
/// <param name="Key">Key of the element.</param>
/// <param name="Id">Identifier as written.</param>
/// <param name="Iri">Expanded IRI.</param>
/// <param name="Kind">PROV element kind.</param>
/// <param name="Bundle">Key of the bundle holding the element, or <c>null</c> for a top-level element.</param>
/// <param name="Attributes">The element's attributes, in document order.</param>
public sealed record GraphElementDto(
    string Key,
    string Id,
    string Iri,
    ElementKind Kind,
    string? Bundle,
    IReadOnlyList<GraphAttributeDto> Attributes);

namespace CpmProvViz.Application.Graph.Contract;

/// <summary>An attribute of an element or a relation.</summary>
/// <param name="Name">Attribute name as written, e.g. <c>prov:type</c>.</param>
/// <param name="Iri">Expanded IRI of the name.</param>
/// <param name="Values">The values, in order: one per item when the attribute's value is an array, a single one when it is bare. A value of a shape not listed for <c>value</c> is left out.</param>
public sealed record GraphAttributeDto(string Name, string Iri, IReadOnlyList<GraphAttributeValueDto> Values);

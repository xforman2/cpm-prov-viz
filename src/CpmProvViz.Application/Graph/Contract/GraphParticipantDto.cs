namespace CpmProvViz.Application.Graph.Contract;

/// <summary>
/// An argument of a relation other than its two main ones, such as a derivation's activity or an
/// association's plan. It is not drawn as an edge.
/// </summary>
/// <param name="Name">The argument's key as written, e.g. <c>prov:plan</c>.</param>
/// <param name="NameIri">Expanded IRI of <c>name</c>.</param>
/// <param name="Id">The identifier the argument names, as written.</param>
/// <param name="Iri">Expanded IRI of <c>id</c>.</param>
/// <param name="Element">Key of the element in the same bundle (or at the top level, for a top-level relation) whose <c>iri</c> equals <c>iri</c>, or <c>null</c> when there is none.</param>
/// <param name="Relation">Key of the relation in the same place whose <c>iri</c> equals <c>iri</c>, as <c>prov:generation</c> and <c>prov:usage</c> name relations; <c>null</c> when there is none or <c>element</c> is set.</param>
public sealed record GraphParticipantDto(
    string Name,
    string NameIri,
    string Id,
    string Iri,
    string? Element,
    string? Relation);

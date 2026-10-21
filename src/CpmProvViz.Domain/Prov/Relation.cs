namespace CpmProvViz.Domain.Prov;

/// <summary>
/// A PROV relation. <see cref="Source"/> and <see cref="Target"/> are its two main arguments in
/// PROV-DM (e.g. the entity and the activity of a generation), or <c>null</c> when absent. Its
/// other arguments that name something are its <see cref="Participants"/>; an argument given in
/// another shape is among its attributes.
/// </summary>
public sealed record Relation(
    RelationType Type,
    QualifiedName? Id,
    QualifiedName? Source,
    QualifiedName? Target,
    IReadOnlyList<Participant> Participants,
    IReadOnlyList<Attribute> Attributes);

namespace CpmProvViz.Domain.Prov;

/// <summary>A PROV entity, activity or agent.</summary>
public sealed record Element(QualifiedName Id, ElementKind Kind, IReadOnlyList<Attribute> Attributes);

namespace CpmProvViz.Domain.Prov;

/// <summary>An attribute of an element or relation, with all of its values in document order.</summary>
public sealed record Attribute(QualifiedName Name, IReadOnlyList<AttributeValue> Values);

/// <summary>An attribute value, as the text of the literal.</summary>
public sealed record AttributeValue(string Value);

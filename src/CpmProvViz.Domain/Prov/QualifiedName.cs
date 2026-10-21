namespace CpmProvViz.Domain.Prov;

/// <summary>A qualified name as written in the document, with the IRI it expands to.</summary>
public sealed record QualifiedName(string Text, string Iri);

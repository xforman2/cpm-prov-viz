namespace CpmProvViz.Domain.Prov;

/// <summary>A named set of PROV statements inside a document, with its own prefix declarations.</summary>
public sealed record Bundle(
    QualifiedName Id,
    IReadOnlyDictionary<string, string> Prefixes,
    IReadOnlyList<Element> Elements,
    IReadOnlyList<Relation> Relations);

namespace CpmProvViz.Domain.Prov;

/// <summary>A PROV document: its prefix declarations, its top-level statements and its bundles.</summary>
/// <remarks>
/// Temporary: PROV and CPM handling moves to the Java provenance service (CPF-Toolbox). Don't
/// extend this.
/// </remarks>
public sealed record ProvDocument(
    IReadOnlyDictionary<string, string> Prefixes,
    IReadOnlyList<Element> Elements,
    IReadOnlyList<Relation> Relations,
    IReadOnlyList<Bundle> Bundles);

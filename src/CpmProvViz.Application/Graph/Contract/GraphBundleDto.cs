namespace CpmProvViz.Application.Graph.Contract;

/// <summary>A bundle: a named set of PROV statements in the document.</summary>
/// <param name="Key">Key of the bundle.</param>
/// <param name="Id">Identifier as written.</param>
/// <param name="Iri">Expanded IRI, resolved with the bundle's own prefixes first.</param>
/// <param name="Prefixes">The bundle's own prefix declarations, as written. Empty if it has none.</param>
/// <param name="IsProvenanceComponent">Whether the bundle is read as a CPM provenance component: <c>true</c> when at least one of its elements has a CPM role (see <c>cpmRoles</c>). This is an interpretation, not a validation: a bundle with a main activity but no connectors is still one.</param>
public sealed record GraphBundleDto(
    string Key,
    string Id,
    string Iri,
    IReadOnlyDictionary<string, string> Prefixes,
    bool IsProvenanceComponent);

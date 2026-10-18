namespace CpmProvViz.Application.Graph;

// The graph contract, specified in docs/graph-contract.md. Change that document first.

public sealed record Graph(
    IReadOnlyDictionary<string, string> Prefixes,
    IReadOnlyList<GraphBundle> Bundles,
    IReadOnlyList<GraphElement> Elements,
    IReadOnlyList<GraphRelation> Relations,
    IReadOnlyList<ParseNote> ParseNotes,
    IReadOnlyList<ValidationFinding> ValidationFindings);

public sealed record GraphBundle(
    string Key,
    string Id,
    string Iri,
    IReadOnlyDictionary<string, string> Prefixes);

public sealed record GraphElement(
    string Key,
    string Id,
    string Iri,
    string Kind,
    string? Bundle,
    IReadOnlyList<GraphAttribute> Attributes);

public sealed record GraphRelation(
    string Key,
    string Type,
    string? Id,
    string? Iri,
    string? Bundle,
    string? Source,
    string? Target,
    IReadOnlyList<GraphAttribute> Attributes);

public sealed record GraphAttribute(string Name, string Iri, IReadOnlyList<GraphAttributeValue> Values);

public sealed record GraphAttributeValue(string Value);

public sealed record ParseNote(string Message, string? Subject);

public sealed record ValidationFinding(string Message, string? Subject);

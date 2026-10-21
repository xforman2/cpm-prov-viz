namespace CpmProvViz.Application.Graph.ReadGraph;

/// <summary>The input of <see cref="IReadGraphUseCase"/>.</summary>
/// <param name="Document">The serialised PROV document to read.</param>
public sealed record ReadGraphRequest(Stream Document);

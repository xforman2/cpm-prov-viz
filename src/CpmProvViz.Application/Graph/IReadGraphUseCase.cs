namespace CpmProvViz.Application.Graph;

/// <summary>Reads a PROV document and maps it to the graph contract.</summary>
public interface IReadGraphUseCase
{
    /// <exception cref="UnreadableDocumentException">The input cannot be read as a PROV document.</exception>
    Task<Graph> ExecuteAsync(Stream input, CancellationToken cancellationToken);
}

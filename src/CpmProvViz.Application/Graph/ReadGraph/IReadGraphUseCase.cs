using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Application.Ports;

namespace CpmProvViz.Application.Graph.ReadGraph;

/// <summary>Reads a PROV document and maps it to the graph contract.</summary>
public interface IReadGraphUseCase
{
    /// <exception cref="UnreadableDocumentException">The input cannot be read as a PROV document.</exception>
    Task<GraphDto> ExecuteAsync(ReadGraphRequest request, CancellationToken cancellationToken);
}

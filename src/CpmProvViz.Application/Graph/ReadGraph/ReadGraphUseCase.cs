using CpmProvViz.Application.Graph.Contract;
using CpmProvViz.Application.Ports;

namespace CpmProvViz.Application.Graph.ReadGraph;

/// <inheritdoc />
public sealed class ReadGraphUseCase(IProvDocumentReader reader) : IReadGraphUseCase
{
    public async Task<GraphDto> ExecuteAsync(ReadGraphRequest request, CancellationToken cancellationToken)
    {
        var document = await reader.ReadAsync(request.Document, cancellationToken);
        var builder = new GraphBuilder();
        builder.AddTopLevel(document);
        foreach (var bundle in document.Bundles)
        {
            builder.AddBundle(bundle);
        }

        return builder.Build(document.Prefixes);
    }
}

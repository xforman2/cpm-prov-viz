using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Ports;

/// <summary>Reads a serialised PROV document into the PROV document model.</summary>
/// <remarks>
/// Temporary: PROV and CPM handling moves to the Java provenance service (CPF-Toolbox). Don't
/// extend this.
/// </remarks>
public interface IProvDocumentReader
{
    /// <exception cref="UnreadableDocumentException">
    /// The input is not in the reader's format or not shaped like a PROV document.
    /// </exception>
    Task<ProvDocument> ReadAsync(Stream input, CancellationToken cancellationToken);
}

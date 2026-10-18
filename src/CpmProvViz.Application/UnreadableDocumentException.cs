namespace CpmProvViz.Application;

/// <summary>
/// The input cannot be read as a PROV document at all. Says nothing about whether a readable
/// document is valid.
/// </summary>
public sealed class UnreadableDocumentException(string message, Exception? innerException = null)
    : Exception(message, innerException);

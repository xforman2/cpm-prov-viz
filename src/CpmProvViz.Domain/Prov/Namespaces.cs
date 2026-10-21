namespace CpmProvViz.Domain.Prov;

/// <summary>
/// The prefix declarations in scope for a document or a bundle. A bundle's declarations shadow
/// the document's (PROV-N §3.7.4); <c>prov</c> and <c>xsd</c> are always defined.
/// </summary>
public sealed class Namespaces(IReadOnlyDictionary<string, string> declared, Namespaces? outer = null)
{
    public const string Prov = "http://www.w3.org/ns/prov#";
    public const string Xsd = "http://www.w3.org/2001/XMLSchema#";

    /// <summary>The older XML Schema namespace, which some tools still write for <c>xsd</c>.</summary>
    public const string XsdLegacy = "http://www.w3.org/2000/10/XMLSchema#";

    private const string DefaultPrefix = "default";

    public IReadOnlyDictionary<string, string> Declared { get; } = declared;

    /// <summary>
    /// Expands a qualified name, splitting at the first <c>:</c>. A name without a prefix uses the
    /// default namespace. A name whose prefix is declared nowhere keeps its text as its IRI.
    /// </summary>
    public QualifiedName Resolve(string text)
    {
        var colon = text.IndexOf(':');
        var (prefix, local) = colon < 0 ? (DefaultPrefix, text) : (text[..colon], text[(colon + 1)..]);
        return new QualifiedName(text, Lookup(prefix) is { } ns ? ns + local : text);
    }

    private string? Lookup(string prefix) =>
        Declared.TryGetValue(prefix, out var ns) ? ns
        : outer is not null ? outer.Lookup(prefix)
        : prefix switch
        {
            "prov" => Prov,
            "xsd" => Xsd,
            _ => null,
        };
}

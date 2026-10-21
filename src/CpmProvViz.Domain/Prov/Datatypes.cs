namespace CpmProvViz.Domain.Prov;

/// <summary>The datatypes of literals that change how a value is read.</summary>
public static class Datatypes
{
    private static readonly HashSet<string> QualifiedNameIris =
    [
        Namespaces.Prov + "QUALIFIED_NAME",
        Namespaces.Xsd + "QName",
        Namespaces.XsdLegacy + "QName",
    ];

    /// <summary>
    /// Whether a literal of this datatype is a qualified name: <c>prov:QUALIFIED_NAME</c> or
    /// <c>xsd:QName</c>, with either XML Schema namespace.
    /// </summary>
    public static bool IsQualifiedName(QualifiedName datatype) => QualifiedNameIris.Contains(datatype.Iri);
}

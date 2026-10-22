using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Domain.Cpm;

/// <summary>
/// Reads PROV statements as CPM. It interprets and never validates: a provenance component missing
/// its connectors is still read as one.
/// </summary>
public interface ICpmInterpreter
{
    /// <summary>
    /// The element's CPM roles, in the order of its <c>prov:type</c> values, each once. Only a value
    /// typed as a qualified name counts, matched by its IRI, so the prefix the document uses doesn't
    /// matter and a plain string never counts.
    /// </summary>
    IReadOnlyList<CpmRole> Roles(Element element);

    /// <summary>A bundle is a provenance component when at least one of its elements has a CPM role.</summary>
    bool IsProvenanceComponent(Bundle bundle);

    /// <summary>
    /// An element of a provenance component with at least one CPM role is traversal information,
    /// whatever its other types; every other one is domain-specific information.
    /// </summary>
    ElementMembership Membership(IReadOnlyList<CpmRole> roles);

    /// <summary>
    /// A relation of a provenance component, by the membership of the elements its two main
    /// arguments name, <c>null</c> for an argument that names none.
    /// </summary>
    RelationMembership Membership(ElementMembership? source, ElementMembership? target);
}

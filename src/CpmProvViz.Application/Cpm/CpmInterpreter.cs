using CpmProvViz.Domain.Cpm;
using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Cpm;

/// <inheritdoc />
/// <remarks>Follows the CPF preprint and CPF-Toolbox's default strategy.</remarks>
public sealed class CpmInterpreter : ICpmInterpreter
{
    /// <summary>The current CPM namespace. Terms in older CPM namespaces are not interpreted.</summary>
    public const string Namespace = "https://www.commonprovenancemodel.org/cpm-namespace-v1-0/";

    private const string ProvType = Namespaces.Prov + "type";

    // Other names in the namespace, such as cpm:id, are not roles.
    private static readonly Dictionary<string, CpmRole> RolesByIri = new()
    {
        [Namespace + "mainActivity"] = CpmRole.MainActivity,
        [Namespace + "forwardConnector"] = CpmRole.ForwardConnector,
        [Namespace + "backwardConnector"] = CpmRole.BackwardConnector,
        [Namespace + "specForwardConnector"] = CpmRole.SpecForwardConnector,
        [Namespace + "senderAgent"] = CpmRole.SenderAgent,
        [Namespace + "receiverAgent"] = CpmRole.ReceiverAgent,
        [Namespace + "currentAgent"] = CpmRole.CurrentAgent,
    };

    public IReadOnlyList<CpmRole> Roles(Element element) =>
        element.Attributes
            .Where(attribute => attribute.Name.Iri == ProvType)
            .SelectMany(attribute => attribute.Values)
            .Select(value => value.QualifiedName is { } name && RolesByIri.TryGetValue(name.Iri, out var role)
                ? role
                : (CpmRole?)null)
            .OfType<CpmRole>()
            .Distinct()
            .ToList();

    public bool IsProvenanceComponent(Bundle bundle) => bundle.Elements.Any(e => Roles(e).Count > 0);

    public ElementMembership Membership(IReadOnlyList<CpmRole> roles) =>
        roles.Count > 0 ? ElementMembership.TraversalInformation : ElementMembership.DomainSpecificInformation;

    public RelationMembership Membership(ElementMembership? source, ElementMembership? target) =>
        (source, target) switch
        {
            (ElementMembership.TraversalInformation, ElementMembership.DomainSpecificInformation)
                or (ElementMembership.DomainSpecificInformation, ElementMembership.TraversalInformation)
                => RelationMembership.Joining,
            (ElementMembership.TraversalInformation, ElementMembership.TraversalInformation or null)
                or (null, ElementMembership.TraversalInformation)
                => RelationMembership.TraversalInformation,
            _ => RelationMembership.DomainSpecificInformation,
        };
}

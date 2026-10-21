namespace CpmProvViz.Domain.Cpm;

/// <summary>
/// Which part of a provenance component a relation belongs to, or that it joins the two parts.
/// </summary>
public enum RelationMembership
{
    TraversalInformation,
    DomainSpecificInformation,
    Joining,
}

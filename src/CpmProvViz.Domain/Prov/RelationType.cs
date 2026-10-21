namespace CpmProvViz.Domain.Prov;

/// <summary>The PROV-DM relations, plus <c>mentionOf</c> from PROV-LINKS.</summary>
public enum RelationType
{
    WasGeneratedBy,
    Used,
    WasInformedBy,
    WasStartedBy,
    WasEndedBy,
    WasInvalidatedBy,
    WasDerivedFrom,
    WasAttributedTo,
    WasAssociatedWith,
    ActedOnBehalfOf,
    WasInfluencedBy,
    SpecializationOf,
    AlternateOf,
    HadMember,
    MentionOf,
}

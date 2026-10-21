using CpmProvViz.Domain.Cpm;
using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Graph.Contract;

/// <summary>
/// A relation record. <c>source</c> and <c>target</c> are its two main arguments in PROV-DM, so an
/// edge from <c>source</c> to <c>target</c> points the way PROV defines it: towards the past. By
/// <c>type</c>, they are: <c>wasGeneratedBy</c> <c>prov:entity</c> → <c>prov:activity</c>,
/// <c>used</c> <c>prov:activity</c> → <c>prov:entity</c>, <c>wasInformedBy</c> <c>prov:informed</c>
/// → <c>prov:informant</c>, <c>wasStartedBy</c> and <c>wasEndedBy</c> <c>prov:activity</c> →
/// <c>prov:trigger</c>, <c>wasInvalidatedBy</c> <c>prov:entity</c> → <c>prov:activity</c>,
/// <c>wasDerivedFrom</c> <c>prov:generatedEntity</c> → <c>prov:usedEntity</c>,
/// <c>wasAttributedTo</c> <c>prov:entity</c> → <c>prov:agent</c>, <c>wasAssociatedWith</c>
/// <c>prov:activity</c> → <c>prov:agent</c>, <c>actedOnBehalfOf</c> <c>prov:delegate</c> →
/// <c>prov:responsible</c>, <c>wasInfluencedBy</c> <c>prov:influencee</c> → <c>prov:influencer</c>,
/// <c>specializationOf</c> and <c>mentionOf</c> <c>prov:specificEntity</c> →
/// <c>prov:generalEntity</c>, <c>alternateOf</c> <c>prov:alternate1</c> → <c>prov:alternate2</c>,
/// <c>hadMember</c> <c>prov:collection</c> → <c>prov:entity</c>.
/// </summary>
/// <param name="Key">Key of the relation.</param>
/// <param name="Type">The relation type, named like its PROV-JSON section.</param>
/// <param name="Id">Identifier as written, or <c>null</c> when the relation has none. Blank identifiers (<c>_:…</c>) carry no meaning in PROV-JSON and are <c>null</c>.</param>
/// <param name="Iri">Expanded IRI of <c>id</c>.</param>
/// <param name="Bundle">Key of the bundle holding the relation, or <c>null</c> at the top level.</param>
/// <param name="Source">Key of the element named by the first main argument, in the same bundle (or at the top level, for a top-level relation). <c>null</c> when the argument is missing, is not a string, or names no element there.</param>
/// <param name="Target">Key of the element named by the second main argument, under the same rules as <c>source</c>.</param>
/// <param name="Participants">The relation's other arguments that are given as a string, in PROV-DM order: <c>wasStartedBy</c> <c>prov:starter</c>, <c>wasEndedBy</c> <c>prov:ender</c>, <c>wasDerivedFrom</c> <c>prov:activity</c>, <c>prov:generation</c>, <c>prov:usage</c>, <c>wasAssociatedWith</c> <c>prov:plan</c>, <c>actedOnBehalfOf</c> <c>prov:activity</c>, <c>mentionOf</c> <c>prov:bundle</c>. Empty for the other types.</param>
/// <param name="Attributes">Every key of the record other than its main arguments and its participants, in document order. An argument that is not a string, such as the array of members in <c>hadMember</c>, is listed here, and so is <c>prov:time</c>.</param>
/// <param name="Membership">For a relation in a provenance component, by the <c>membership</c> of the elements <c>source</c> and <c>target</c> name: <c>traversalInformation</c> when it names at least one element and every one it names is traversal information, <c>joining</c> when one is traversal information and the other domain-specific information, otherwise <c>domainSpecificInformation</c>, also when it names no element. Participants don't count. <c>null</c> for a relation outside any provenance component.</param>
public sealed record GraphRelationDto(
    string Key,
    RelationType Type,
    string? Id,
    string? Iri,
    string? Bundle,
    string? Source,
    string? Target,
    IReadOnlyList<GraphParticipantDto> Participants,
    IReadOnlyList<GraphAttributeDto> Attributes,
    RelationMembership? Membership);

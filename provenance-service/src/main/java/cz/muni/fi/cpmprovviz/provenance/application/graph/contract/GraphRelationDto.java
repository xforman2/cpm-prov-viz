package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.List;

/**
 * A relation. {@code source} and {@code target} are its two main arguments in PROV-DM, the effect and the cause as
 * ProvToolbox and CPF-Toolbox call them, so an edge from {@code source} to {@code target} points the way PROV defines
 * it: towards the past. By {@code type}, they are: {@code wasGeneratedBy} {@code prov:entity} → {@code prov:activity},
 * {@code used} {@code prov:activity} → {@code prov:entity}, {@code wasInformedBy} {@code prov:informed} →
 * {@code prov:informant}, {@code wasStartedBy} and {@code wasEndedBy} {@code prov:activity} → {@code prov:trigger},
 * {@code wasInvalidatedBy} {@code prov:entity} → {@code prov:activity}, {@code wasDerivedFrom}
 * {@code prov:generatedEntity} → {@code prov:usedEntity}, {@code wasAttributedTo} {@code prov:entity} →
 * {@code prov:agent}, {@code wasAssociatedWith} {@code prov:activity} → {@code prov:agent}, {@code actedOnBehalfOf}
 * {@code prov:delegate} → {@code prov:responsible}, {@code wasInfluencedBy} {@code prov:influencee} →
 * {@code prov:influencer}, {@code specializationOf} and {@code mentionOf} {@code prov:specificEntity} →
 * {@code prov:generalEntity}, {@code alternateOf} {@code prov:alternate1} → {@code prov:alternate2}, {@code hadMember}
 * {@code prov:collection} → its first {@code prov:entity}.
 *
 * @param key key of the relation
 * @param type the relation type, named like its PROV-JSON section
 * @param id identifier, or {@code null} when ProvToolbox gives the relation none, as for a blank identifier
 *     ({@code _:…})
 * @param iri expanded IRI of {@code id}, or {@code null}
 * @param bundle key of the bundle holding the relation. Never {@code null} for now (see {@link GraphDto}); {@code null}
 *     is kept for a relation outside any bundle.
 * @param source key of the element CPF-Toolbox links as the first main argument: the element in the same bundle with
 *     that identifier and the kind the relation expects there. {@code null} when the argument is missing or names no
 *     such element.
 * @param target key of the element CPF-Toolbox links as the second main argument, under the same rules as
 *     {@code source}
 * @param participants the relation's other arguments, in PROV-DM order: {@code wasStartedBy} {@code prov:starter},
 *     {@code wasEndedBy} {@code prov:ender}, {@code wasDerivedFrom} {@code prov:activity}, {@code prov:generation},
 *     {@code prov:usage}, {@code wasAssociatedWith} {@code prov:plan}, {@code actedOnBehalfOf} {@code prov:activity},
 *     {@code mentionOf} {@code prov:bundle}, and {@code hadMember} {@code prov:entity} for each member after the first.
 *     Empty for the other types.
 * @param attributes the relation's attributes, as ProvToolbox gives them ({@code getAttributes()}). {@code prov:time}
 *     is not among them: ProvToolbox keeps it apart.
 * @param membership by the {@code membership} of the elements at its two ends: {@code traversalInformation} when both
 *     are traversal information, {@code joining} when one is traversal information and the other domain-specific
 *     information, otherwise {@code domainSpecificInformation}, also when an end is missing from the bundle.
 *     Participants don't count. Never {@code null} for now.
 */
public record GraphRelationDto(
        String key,
        RelationType type,
        String id,
        String iri,
        String bundle,
        String source,
        String target,
        List<GraphParticipantDto> participants,
        List<GraphAttributeDto> attributes,
        RelationMembership membership) {}

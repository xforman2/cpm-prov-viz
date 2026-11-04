package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import com.fasterxml.jackson.annotation.JsonProperty;

/** The PROV-DM relations, plus {@code mentionOf} from PROV-LINKS, named like their PROV-JSON sections. */
public enum RelationType {
    @JsonProperty("wasGeneratedBy")
    WAS_GENERATED_BY,
    @JsonProperty("used")
    USED,
    @JsonProperty("wasInformedBy")
    WAS_INFORMED_BY,
    @JsonProperty("wasStartedBy")
    WAS_STARTED_BY,
    @JsonProperty("wasEndedBy")
    WAS_ENDED_BY,
    @JsonProperty("wasInvalidatedBy")
    WAS_INVALIDATED_BY,
    @JsonProperty("wasDerivedFrom")
    WAS_DERIVED_FROM,
    @JsonProperty("wasAttributedTo")
    WAS_ATTRIBUTED_TO,
    @JsonProperty("wasAssociatedWith")
    WAS_ASSOCIATED_WITH,
    @JsonProperty("actedOnBehalfOf")
    ACTED_ON_BEHALF_OF,
    @JsonProperty("wasInfluencedBy")
    WAS_INFLUENCED_BY,
    @JsonProperty("specializationOf")
    SPECIALIZATION_OF,
    @JsonProperty("alternateOf")
    ALTERNATE_OF,
    @JsonProperty("hadMember")
    HAD_MEMBER,
    @JsonProperty("mentionOf")
    MENTION_OF
}

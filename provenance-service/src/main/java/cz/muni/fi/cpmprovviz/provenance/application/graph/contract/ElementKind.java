package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import com.fasterxml.jackson.annotation.JsonProperty;

/** The PROV element kinds. */
public enum ElementKind {
    @JsonProperty("entity")
    ENTITY,
    @JsonProperty("activity")
    ACTIVITY,
    @JsonProperty("agent")
    AGENT
}

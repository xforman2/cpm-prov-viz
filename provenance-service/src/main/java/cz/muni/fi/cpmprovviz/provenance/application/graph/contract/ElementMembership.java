package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import com.fasterxml.jackson.annotation.JsonProperty;

/** Which part of a bundle an element belongs to, as CPF-Toolbox's TI strategy decides. */
public enum ElementMembership {
    @JsonProperty("traversalInformation")
    TRAVERSAL_INFORMATION,
    @JsonProperty("domainSpecificInformation")
    DOMAIN_SPECIFIC_INFORMATION
}

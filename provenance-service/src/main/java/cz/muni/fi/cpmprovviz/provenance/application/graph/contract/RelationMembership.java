package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import com.fasterxml.jackson.annotation.JsonProperty;

/** Which part of a bundle a relation belongs to, or that it joins the two parts. */
public enum RelationMembership {
    @JsonProperty("traversalInformation")
    TRAVERSAL_INFORMATION,
    @JsonProperty("domainSpecificInformation")
    DOMAIN_SPECIFIC_INFORMATION,
    @JsonProperty("joining")
    JOINING
}

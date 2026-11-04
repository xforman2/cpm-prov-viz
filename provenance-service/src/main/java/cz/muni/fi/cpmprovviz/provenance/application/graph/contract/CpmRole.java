package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * The CPM types that are roles an element plays in the traversal information: CPF-Toolbox's {@code CpmType} without
 * {@code IDENTIFIER} ({@code cpm:id}).
 */
public enum CpmRole {
    @JsonProperty("mainActivity")
    MAIN_ACTIVITY,
    @JsonProperty("forwardConnector")
    FORWARD_CONNECTOR,
    @JsonProperty("backwardConnector")
    BACKWARD_CONNECTOR,
    @JsonProperty("specForwardConnector")
    SPEC_FORWARD_CONNECTOR,
    @JsonProperty("senderAgent")
    SENDER_AGENT,
    @JsonProperty("receiverAgent")
    RECEIVER_AGENT,
    @JsonProperty("currentAgent")
    CURRENT_AGENT
}

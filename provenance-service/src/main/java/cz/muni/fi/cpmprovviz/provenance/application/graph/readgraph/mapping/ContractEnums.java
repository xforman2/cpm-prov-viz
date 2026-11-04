package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping;

import cz.muni.fi.cpm.constants.CpmType;
import cz.muni.fi.cpm.model.CpmUtilities;
import cz.muni.fi.cpm.model.INode;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.CpmRole;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.ElementKind;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.RelationType;
import java.util.Arrays;
import java.util.List;
import org.openprovenance.prov.model.StatementOrBundle.Kind;

/** Turns ProvToolbox's kinds and CPF-Toolbox's CPM types into the contract's enums. Stateless. */
final class ContractEnums {

    private ContractEnums() {}

    static ElementKind elementKind(Kind kind) {
        return switch (kind) {
            case PROV_ENTITY -> ElementKind.ENTITY;
            case PROV_ACTIVITY -> ElementKind.ACTIVITY;
            case PROV_AGENT -> ElementKind.AGENT;
            default -> throw new IllegalArgumentException(kind + " is not an element kind");
        };
    }

    static RelationType relationType(Kind kind) {
        return switch (kind) {
            case PROV_GENERATION -> RelationType.WAS_GENERATED_BY;
            case PROV_USAGE -> RelationType.USED;
            case PROV_COMMUNICATION -> RelationType.WAS_INFORMED_BY;
            case PROV_START -> RelationType.WAS_STARTED_BY;
            case PROV_END -> RelationType.WAS_ENDED_BY;
            case PROV_INVALIDATION -> RelationType.WAS_INVALIDATED_BY;
            case PROV_DERIVATION -> RelationType.WAS_DERIVED_FROM;
            case PROV_ATTRIBUTION -> RelationType.WAS_ATTRIBUTED_TO;
            case PROV_ASSOCIATION -> RelationType.WAS_ASSOCIATED_WITH;
            case PROV_DELEGATION -> RelationType.ACTED_ON_BEHALF_OF;
            case PROV_INFLUENCE -> RelationType.WAS_INFLUENCED_BY;
            case PROV_SPECIALIZATION -> RelationType.SPECIALIZATION_OF;
            case PROV_ALTERNATE -> RelationType.ALTERNATE_OF;
            case PROV_MEMBERSHIP -> RelationType.HAD_MEMBER;
            case PROV_MENTION -> RelationType.MENTION_OF;
            default -> throw new IllegalArgumentException(kind + " has no relation type in the graph contract");
        };
    }

    /** Every role CPF-Toolbox finds on the node ({@code CpmUtilities.hasCpmType}), in {@link CpmRole} order. */
    static List<CpmRole> cpmRoles(INode node) {
        return Arrays.stream(CpmRole.values())
                .filter(role -> CpmUtilities.hasCpmType(node, cpmType(role)))
                .toList();
    }

    private static CpmType cpmType(CpmRole role) {
        return switch (role) {
            case MAIN_ACTIVITY -> CpmType.MAIN_ACTIVITY;
            case FORWARD_CONNECTOR -> CpmType.FORWARD_CONNECTOR;
            case BACKWARD_CONNECTOR -> CpmType.BACKWARD_CONNECTOR;
            case SPEC_FORWARD_CONNECTOR -> CpmType.SPEC_FORWARD_CONNECTOR;
            case SENDER_AGENT -> CpmType.SENDER_AGENT;
            case RECEIVER_AGENT -> CpmType.RECEIVER_AGENT;
            case CURRENT_AGENT -> CpmType.CURRENT_AGENT;
        };
    }
}

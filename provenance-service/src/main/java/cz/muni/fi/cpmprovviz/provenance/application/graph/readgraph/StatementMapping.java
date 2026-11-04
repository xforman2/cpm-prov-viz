package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpm.constants.CpmType;
import cz.muni.fi.cpm.model.CpmUtilities;
import cz.muni.fi.cpm.model.INode;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.CpmRole;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.ElementKind;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphAttributeDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphAttributeValueDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphParticipantDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.RelationType;
import java.util.AbstractMap.SimpleEntry;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import org.openprovenance.prov.model.ActedOnBehalfOf;
import org.openprovenance.prov.model.Attribute;
import org.openprovenance.prov.model.HadMember;
import org.openprovenance.prov.model.LangString;
import org.openprovenance.prov.model.MentionOf;
import org.openprovenance.prov.model.Namespace;
import org.openprovenance.prov.model.QualifiedName;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.Statement;
import org.openprovenance.prov.model.StatementOrBundle.Kind;
import org.openprovenance.prov.model.WasAssociatedWith;
import org.openprovenance.prov.model.WasDerivedFrom;
import org.openprovenance.prov.model.WasEndedBy;
import org.openprovenance.prov.model.WasStartedBy;
import org.openprovenance.prov.vanilla.HasAttributes;

/** Maps ProvToolbox's and CPF-Toolbox's values to the contract's. Stateless. */
final class StatementMapping {

    private static final String PROV_PREFIX = "prov";
    private static final String PROV_NAMESPACE = "http://www.w3.org/ns/prov#";

    private StatementMapping() {}

    /** Prefix → namespace IRI, with a default namespace under {@code default}, as PROV-JSON writes it. */
    static Map<String, String> prefixes(Namespace namespace) {
        Map<String, String> prefixes = new LinkedHashMap<>();
        if (namespace == null) {
            return prefixes;
        }
        if (namespace.getDefaultNamespace() != null) {
            prefixes.put("default", namespace.getDefaultNamespace());
        }
        prefixes.putAll(namespace.getPrefixes());
        return prefixes;
    }

    /** The qualified name as {@code prefix:local}, or just the local part without a prefix. */
    static String qualifiedName(QualifiedName name) {
        if (name == null) {
            return null;
        }
        String prefix = name.getPrefix();
        return prefix == null || prefix.isEmpty() ? name.getLocalPart() : prefix + ":" + name.getLocalPart();
    }

    static String iri(QualifiedName name) {
        return name == null ? null : name.getUri();
    }

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

    /** Every role CPF-Toolbox finds on the node, in {@link CpmRole} order. */
    static List<CpmRole> cpmRoles(INode node) {
        return Arrays.stream(CpmRole.values())
                .filter(role -> CpmUtilities.hasCpmType(node, cpmType(role)))
                .toList();
    }

    /**
     * The attributes of the statements, one entry per name in the order each name first comes.
     *
     * @param elementKeysByIri key of each element in the same place by its IRI
     */
    static List<GraphAttributeDto> attributes(
            List<? extends Statement> statements, Map<String, String> elementKeysByIri) {
        Map<QualifiedName, List<Attribute>> byName = new LinkedHashMap<>();
        for (Statement statement : statements) {
            if (statement instanceof HasAttributes hasAttributes) {
                for (Attribute attribute : hasAttributes.getAttributes()) {
                    byName.computeIfAbsent(attribute.getElementName(), _ -> new ArrayList<>())
                            .add(attribute);
                }
            }
        }
        return byName.entrySet().stream()
                .map(entry -> new GraphAttributeDto(
                        qualifiedName(entry.getKey()),
                        iri(entry.getKey()),
                        entry.getValue().stream()
                                .map(attribute -> value(attribute, elementKeysByIri))
                                .toList()))
                .toList();
    }

    /**
     * The relation's arguments other than its effect and cause, in PROV-DM order.
     *
     * @param elementKeysByIri key of each element in the same place by its IRI
     * @param relationKeysByIri key of each relation in the same place by its IRI
     */
    static List<GraphParticipantDto> participants(
            Relation relation, Map<String, String> elementKeysByIri, Map<String, String> relationKeysByIri) {
        List<SimpleEntry<String, QualifiedName>> arguments = switch (relation) {
            case WasStartedBy start -> List.of(argument("starter", start.getStarter()));
            case WasEndedBy end -> List.of(argument("ender", end.getEnder()));
            case WasDerivedFrom derivation ->
                List.of(
                        argument("activity", derivation.getActivity()),
                        argument("generation", derivation.getGeneration()),
                        argument("usage", derivation.getUsage()));
            case WasAssociatedWith association -> List.of(argument("plan", association.getPlan()));
            case ActedOnBehalfOf delegation -> List.of(argument("activity", delegation.getActivity()));
            case MentionOf mention -> List.of(argument("bundle", mention.getBundle()));
            case HadMember membership ->
                membership.getEntity().stream()
                        .skip(1)
                        .map(member -> argument("entity", member))
                        .toList();
            default -> List.of();
        };
        return arguments.stream()
                .filter(argument -> argument.getValue() != null)
                .map(argument ->
                        participant(argument.getKey(), argument.getValue(), elementKeysByIri, relationKeysByIri))
                .toList();
    }

    /** A relation's argument: its local name in the PROV namespace and the identifier it names, if any. */
    private static SimpleEntry<String, QualifiedName> argument(String name, QualifiedName value) {
        return new SimpleEntry<>(name, value);
    }

    private static GraphParticipantDto participant(
            String name,
            QualifiedName value,
            Map<String, String> elementKeysByIri,
            Map<String, String> relationKeysByIri) {
        String element = elementKeysByIri.get(iri(value));
        return new GraphParticipantDto(
                PROV_PREFIX + ":" + name,
                PROV_NAMESPACE + name,
                qualifiedName(value),
                iri(value),
                element,
                element == null ? relationKeysByIri.get(iri(value)) : null);
    }

    private static GraphAttributeValueDto value(Attribute attribute, Map<String, String> elementKeysByIri) {
        Object value = attribute.getValue();
        String text = switch (value) {
            case LangString langString -> langString.getValue();
            case QualifiedName name -> qualifiedName(name);
            case null -> null;
            default -> String.valueOf(value);
        };
        String valueIri = value instanceof QualifiedName name ? iri(name) : null;
        return new GraphAttributeValueDto(
                text,
                qualifiedName(attribute.getType()),
                iri(attribute.getType()),
                value instanceof LangString langString ? langString.getLang() : null,
                valueIri,
                valueIri == null ? null : elementKeysByIri.get(valueIri));
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

package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping;

import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.IEdge;
import cz.muni.fi.cpm.model.INode;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.ElementMembership;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphAttributeDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphAttributeValueDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphParticipantDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphRelationDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.RelationMembership;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.IdentityHashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import org.openprovenance.prov.model.ActedOnBehalfOf;
import org.openprovenance.prov.model.Attribute;
import org.openprovenance.prov.model.Element;
import org.openprovenance.prov.model.HadMember;
import org.openprovenance.prov.model.Identifiable;
import org.openprovenance.prov.model.LangString;
import org.openprovenance.prov.model.MentionOf;
import org.openprovenance.prov.model.QualifiedName;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.Statement;
import org.openprovenance.prov.model.WasAssociatedWith;
import org.openprovenance.prov.model.WasDerivedFrom;
import org.openprovenance.prov.model.WasEndedBy;
import org.openprovenance.prov.model.WasStartedBy;
import org.openprovenance.prov.vanilla.HasAttributes;

/**
 * Maps CPF-Toolbox's view of one bundle to the contract: one element per node and one relation per relation on its
 * edges. Created per bundle: it holds that bundle's keys, because a statement refers only to elements in its own
 * bundle.
 */
final class BundleMapper {

    private static final String PROV_PREFIX = "prov";
    private static final String PROV_NAMESPACE = "http://www.w3.org/ns/prov#";

    private final String bundleKey;
    private final ITIStrategy tiStrategy;

    private final List<INode> nodes = new ArrayList<>();
    private final Map<INode, String> nodeKeys = new IdentityHashMap<>();
    private final Map<String, String> elementKeysByIri = new HashMap<>();

    private final List<Relation> relations = new ArrayList<>();
    private final Map<Relation, String> relationKeys = new IdentityHashMap<>();
    private final Map<String, String> relationKeysByIri = new HashMap<>();
    /** The first edge each relation is on. */
    private final Map<Relation, IEdge> relationEdges = new IdentityHashMap<>();

    /**
     * Keys the view's nodes in the order of their first declaration and its relations in the order of its edges.
     *
     * @param bundleKey key of the bundle
     * @param statements the statements the view was built from, in ProvToolbox's reading order
     * @param view CPF-Toolbox's view
     * @param tiStrategy the view's TI strategy
     * @param keyedElements how many elements the graph has keyed before
     * @param keyedRelations how many relations the graph has keyed before
     */
    BundleMapper(
            String bundleKey,
            List<Statement> statements,
            CpmDocument view,
            ITIStrategy tiStrategy,
            int keyedElements,
            int keyedRelations) {
        this.bundleKey = bundleKey;
        this.tiStrategy = tiStrategy;

        for (Statement statement : statements) {
            if (statement instanceof Element element) {
                INode node = view.getNode(element.getId(), element.getKind());
                if (node != null && !nodeKeys.containsKey(node)) {
                    String key = "e" + (keyedElements + nodes.size() + 1);
                    nodes.add(node);
                    nodeKeys.put(node, key);
                    elementKeysByIri.putIfAbsent(ProvNames.iri(node.getId()), key);
                }
            }
        }

        for (IEdge edge : view.getEdges()) {
            for (Relation relation : edge.getRelations()) {
                if (!relationEdges.containsKey(relation)) {
                    String key = "r" + (keyedRelations + relations.size() + 1);
                    relations.add(relation);
                    relationEdges.put(relation, edge);
                    relationKeys.put(relation, key);
                    QualifiedName id = id(relation);
                    if (id != null) {
                        relationKeysByIri.putIfAbsent(ProvNames.iri(id), key);
                    }
                }
            }
        }
    }

    List<GraphElementDto> elements() {
        return nodes.stream().map(this::element).toList();
    }

    List<GraphRelationDto> relations() {
        return relations.stream().map(this::relation).toList();
    }

    private GraphElementDto element(INode node) {
        return new GraphElementDto(
                nodeKeys.get(node),
                ProvNames.qualifiedName(node.getId()),
                ProvNames.iri(node.getId()),
                ContractEnums.elementKind(node.getKind()),
                bundleKey,
                attributes(node.getElements()),
                ContractEnums.cpmRoles(node),
                membership(node));
    }

    private GraphRelationDto relation(Relation relation) {
        IEdge edge = relationEdges.get(relation);
        QualifiedName id = id(relation);
        return new GraphRelationDto(
                relationKeys.get(relation),
                ContractEnums.relationType(relation.getKind()),
                ProvNames.qualifiedName(id),
                ProvNames.iri(id),
                bundleKey,
                key(edge.getEffect()),
                key(edge.getCause()),
                participants(relation),
                attributes(List.of(relation)),
                membership(edge));
    }

    private String key(INode node) {
        return node == null ? null : nodeKeys.get(node);
    }

    /** The node's membership, or {@code null} for a missing node. */
    private ElementMembership membership(INode node) {
        if (node == null) {
            return null;
        }
        return tiStrategy.belongsToTraversalInformation(node)
                ? ElementMembership.TRAVERSAL_INFORMATION
                : ElementMembership.DOMAIN_SPECIFIC_INFORMATION;
    }

    /** By the membership of the edge's two ends; an end missing from the bundle has none. */
    private RelationMembership membership(IEdge edge) {
        ElementMembership effect = membership(edge.getEffect());
        ElementMembership cause = membership(edge.getCause());
        if (effect == ElementMembership.TRAVERSAL_INFORMATION && cause == ElementMembership.TRAVERSAL_INFORMATION) {
            return RelationMembership.TRAVERSAL_INFORMATION;
        }
        if (effect != null && cause != null && effect != cause) {
            return RelationMembership.JOINING;
        }
        return RelationMembership.DOMAIN_SPECIFIC_INFORMATION;
    }

    /** The attributes of the statements, one entry per name in the order each name first comes. */
    private List<GraphAttributeDto> attributes(List<? extends Statement> statements) {
        Map<QualifiedName, List<Attribute>> byName = new LinkedHashMap<>();
        for (Statement statement : statements) {
            if (statement instanceof HasAttributes hasAttributes) {
                for (Attribute attribute : hasAttributes.getAttributes()) {
                    byName.computeIfAbsent(attribute.getElementName(), name -> new ArrayList<>())
                            .add(attribute);
                }
            }
        }
        return byName.entrySet().stream()
                .map(entry -> new GraphAttributeDto(
                        ProvNames.qualifiedName(entry.getKey()),
                        ProvNames.iri(entry.getKey()),
                        entry.getValue().stream().map(this::value).toList()))
                .toList();
    }

    private GraphAttributeValueDto value(Attribute attribute) {
        Object value = attribute.getValue();
        String text = switch (value) {
            case LangString langString -> langString.getValue();
            case QualifiedName name -> ProvNames.qualifiedName(name);
            case null -> null;
            default -> String.valueOf(value);
        };
        String valueIri = value instanceof QualifiedName name ? ProvNames.iri(name) : null;
        return new GraphAttributeValueDto(
                text,
                ProvNames.qualifiedName(attribute.getType()),
                ProvNames.iri(attribute.getType()),
                value instanceof LangString langString ? langString.getLang() : null,
                valueIri,
                valueIri == null ? null : elementKeysByIri.get(valueIri));
    }

    /** The relation's arguments other than its effect and cause, in PROV-DM order. */
    private List<GraphParticipantDto> participants(Relation relation) {
        List<RelationArgument> arguments = switch (relation) {
            case WasStartedBy start -> List.of(new RelationArgument("starter", start.getStarter()));
            case WasEndedBy end -> List.of(new RelationArgument("ender", end.getEnder()));
            case WasDerivedFrom derivation ->
                List.of(
                        new RelationArgument("activity", derivation.getActivity()),
                        new RelationArgument("generation", derivation.getGeneration()),
                        new RelationArgument("usage", derivation.getUsage()));
            case WasAssociatedWith association -> List.of(new RelationArgument("plan", association.getPlan()));
            case ActedOnBehalfOf delegation -> List.of(new RelationArgument("activity", delegation.getActivity()));
            case MentionOf mention -> List.of(new RelationArgument("bundle", mention.getBundle()));
            case HadMember membership ->
                membership.getEntity().stream()
                        .skip(1)
                        .map(member -> new RelationArgument("entity", member))
                        .toList();
            default -> List.of();
        };
        return arguments.stream()
                .filter(argument -> argument.value() != null)
                .map(this::participant)
                .toList();
    }

    /** A participant names an element, or else a relation, as {@code prov:generation} and {@code prov:usage} do. */
    private GraphParticipantDto participant(RelationArgument argument) {
        String iri = ProvNames.iri(argument.value());
        String element = elementKeysByIri.get(iri);
        return new GraphParticipantDto(
                PROV_PREFIX + ":" + argument.name(),
                PROV_NAMESPACE + argument.name(),
                ProvNames.qualifiedName(argument.value()),
                iri,
                element,
                element == null ? relationKeysByIri.get(iri) : null);
    }

    private static QualifiedName id(Relation relation) {
        return relation instanceof Identifiable identifiable ? identifiable.getId() : null;
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.IEdge;
import cz.muni.fi.cpm.model.INode;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.ElementMembership;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphRelationDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.RelationMembership;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Map;
import org.openprovenance.prov.model.Element;
import org.openprovenance.prov.model.Identifiable;
import org.openprovenance.prov.model.QualifiedName;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.Statement;

/**
 * Maps CPF-Toolbox's view of one bundle, or of the statements outside any bundle, to the contract: one element per node
 * and one relation per relation on its edges. Created per bundle, holds that bundle's keys.
 */
final class ScopeMapper {

    private final String bundleKey;
    private final ITIStrategy tiStrategy;
    private final List<INode> nodes = new ArrayList<>();
    private final Map<INode, String> nodeKeys = new IdentityHashMap<>();
    private final Map<String, String> elementKeysByIri = new HashMap<>();
    /** Each relation with the first edge it is on, in the order of the edges. */
    private final Map<Relation, IEdge> relationEdges = new IdentityHashMap<>();

    private final List<Relation> relations = new ArrayList<>();
    private final Map<Relation, String> relationKeys = new IdentityHashMap<>();
    private final Map<String, String> relationKeysByIri = new HashMap<>();

    /**
     * Keys the view's nodes in the order of their first declaration and its relations in the order of its edges.
     *
     * @param bundleKey key of the bundle, or {@code null} outside any bundle
     * @param statements the statements the view was built from, in ProvToolbox's reading order
     * @param view CPF-Toolbox's view
     * @param tiStrategy the view's TI strategy, or {@code null} outside any bundle, where nothing has a membership
     * @param keyedElements how many elements the graph has keyed before
     * @param keyedRelations how many relations the graph has keyed before
     */
    ScopeMapper(
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
                    elementKeysByIri.putIfAbsent(StatementMapping.iri(node.getId()), key);
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
                        relationKeysByIri.putIfAbsent(StatementMapping.iri(id), key);
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
                StatementMapping.qualifiedName(node.getId()),
                StatementMapping.iri(node.getId()),
                StatementMapping.elementKind(node.getKind()),
                bundleKey,
                StatementMapping.attributes(node.getElements(), elementKeysByIri),
                StatementMapping.cpmRoles(node),
                membership(node));
    }

    private GraphRelationDto relation(Relation relation) {
        IEdge edge = relationEdges.get(relation);
        QualifiedName id = id(relation);
        return new GraphRelationDto(
                relationKeys.get(relation),
                StatementMapping.relationType(relation.getKind()),
                StatementMapping.qualifiedName(id),
                StatementMapping.iri(id),
                bundleKey,
                key(edge.getEffect()),
                key(edge.getCause()),
                StatementMapping.participants(relation, elementKeysByIri, relationKeysByIri),
                StatementMapping.attributes(List.of(relation), elementKeysByIri),
                membership(edge));
    }

    private String key(INode node) {
        return node == null ? null : nodeKeys.get(node);
    }

    private ElementMembership membership(INode node) {
        if (tiStrategy == null || node == null) {
            return null;
        }
        return tiStrategy.belongsToTraversalInformation(node)
                ? ElementMembership.TRAVERSAL_INFORMATION
                : ElementMembership.DOMAIN_SPECIFIC_INFORMATION;
    }

    /** By the membership of the edge's two ends; an end missing from the bundle has none. */
    private RelationMembership membership(IEdge edge) {
        if (tiStrategy == null) {
            return null;
        }
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

    private static QualifiedName id(Relation relation) {
        return relation instanceof Identifiable identifiable ? identifiable.getId() : null;
    }
}

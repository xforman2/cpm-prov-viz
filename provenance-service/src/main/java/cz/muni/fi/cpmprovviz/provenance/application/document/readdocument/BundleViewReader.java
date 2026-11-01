package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpm.constants.CpmType;
import cz.muni.fi.cpm.divided.ordered.CpmOrderedFactory;
import cz.muni.fi.cpm.model.Component;
import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.CpmUtilities;
import cz.muni.fi.cpm.model.ICpmFactory;
import cz.muni.fi.cpm.model.IEdge;
import cz.muni.fi.cpm.model.INode;
import cz.muni.fi.cpm.vanilla.CpmProvFactory;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.BundleDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ElementPart;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.RelationDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.RelationPart;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Element;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.Statement;
import org.openprovenance.prov.vanilla.ProvFactory;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

/**
 * Builds CPF-Toolbox's CPM view of one bundle and flattens it. Created per bundle, holds that
 * bundle's view.
 */
final class BundleViewReader {

    private final Bundle bundle;
    private final ICpmFactory cpmFactory;
    private final CpmDocument cpmDocument;
    private final Set<NodeKey> traversalInformationNodes;
    private final Set<NodeKey> domainSpecificNodes;
    private final Set<EdgeKey> traversalInformationEdges;
    private final Set<EdgeKey> domainSpecificEdges;
    private final Set<IEdge> crossPartEdges;

    /** Builds the view with {@link CpmOrderedFactory} and the default TI strategy. */
    BundleViewReader(Bundle bundle) {
        this.bundle = bundle;
        ProvFactory provFactory = new ProvFactory();
        this.cpmFactory = new CpmOrderedFactory(provFactory);
        this.cpmDocument = new CpmDocument(
                new ArrayList<>(bundle.getStatement()),
                bundle.getId(),
                provFactory,
                new CpmProvFactory(provFactory),
                cpmFactory);

        List<INode> traversalInformationPart = cpmDocument.getTraversalInformationPart();
        List<INode> domainSpecificPart = cpmDocument.getDomainSpecificPart();
        this.traversalInformationNodes = traversalInformationPart.stream()
                .map(NodeKey::of)
                .collect(Collectors.toSet());
        this.domainSpecificNodes = domainSpecificPart.stream()
                .map(NodeKey::of)
                .collect(Collectors.toSet());
        this.traversalInformationEdges = edgeKeys(traversalInformationPart);
        this.domainSpecificEdges = edgeKeys(domainSpecificPart);
        this.crossPartEdges = Collections.newSetFromMap(new IdentityHashMap<>());
        this.crossPartEdges.addAll(cpmDocument.getCrossPartEdges());
    }

    /** The bundle, its nodes and its relations, in the order the toolbox read them. */
    BundleDto read() {
        List<ElementDto> elements = new ArrayList<>();
        List<RelationDto> relations = new ArrayList<>();
        Set<INode> readNodes = Collections.newSetFromMap(new IdentityHashMap<>());
        Set<Relation> readRelations = Collections.newSetFromMap(new IdentityHashMap<>());

        for (Statement statement : statements()) {
            if (statement instanceof Element element) {
                INode node = cpmDocument.getNode(element.getId(), element.getKind());
                if (node != null && readNodes.add(node)) {
                    elements.add(element(node));
                }
            } else if (statement instanceof Relation relation && readRelations.add(relation)) {
                relations.add(StatementMapping.relation(relation, part(relation)));
            }
        }

        return new BundleDto(
                StatementMapping.qualifiedName(bundle.getId()),
                StatementMapping.iri(bundle.getId()),
                StatementMapping.prefixes(bundle.getNamespace()),
                elements,
                relations);
    }

    /**
     * The view's statements, each once, in the order the toolbox read them: what
     * {@code CpmDocument.toDocument()} puts in its bundle, without gathering namespaces, which fails
     * on some bundles (e.g. {@code ex:processingBundle} in {@code testdata/handmade/two-bundles.json}).
     */
    private List<Statement> statements() {
        List<Component> components = new ArrayList<>(cpmDocument.getNodes());
        components.addAll(cpmDocument.getEdges());
        return cpmFactory.getComponentsTransformer().apply(components);
    }

    private ElementDto element(INode node) {
        ElementPart part = part(node);
        List<CpmType> cpmRoles = Arrays.stream(CpmType.values())
                .filter(type -> CpmUtilities.hasCpmType(node, type))
                .toList();
        return StatementMapping.element(node.getElements(), part, cpmRoles);
    }

    private ElementPart part(INode node) {
        NodeKey key = NodeKey.of(node);
        if (traversalInformationNodes.contains(key)) {
            return ElementPart.TRAVERSAL_INFORMATION;
        }
        if (domainSpecificNodes.contains(key)) {
            return ElementPart.DOMAIN_SPECIFIC;
        }
        throw new IllegalStateException("CPF-Toolbox put node " + key + " in neither part");
    }

    /** The part of the first toolbox edge that holds the relation. */
    private RelationPart part(Relation relation) {
        return cpmDocument.getEdges(relation).stream()
                .findFirst()
                .map(this::part)
                .orElse(RelationPart.NONE);
    }

    private RelationPart part(IEdge edge) {
        if (edge.getEffect() == null || edge.getCause() == null) {
            return RelationPart.NONE;
        }
        if (crossPartEdges.contains(edge)) {
            return RelationPart.JOINING;
        }
        EdgeKey key = EdgeKey.of(edge);
        if (traversalInformationEdges.contains(key)) {
            return RelationPart.TRAVERSAL_INFORMATION;
        }
        if (domainSpecificEdges.contains(key)) {
            return RelationPart.DOMAIN_SPECIFIC;
        }
        return RelationPart.NONE;
    }

    private static Set<EdgeKey> edgeKeys(List<INode> part) {
        return part.stream()
                .flatMap(node -> node.getCauseEdges().stream())
                .map(EdgeKey::of)
                .collect(Collectors.toSet());
    }
}

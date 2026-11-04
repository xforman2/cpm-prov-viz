package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphBundleDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphRelationDto;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Statement;

/** Builds the graph of one document, keying bundles in reading order. Created per document. */
final class GraphBuilder {

    private final List<GraphBundleDto> bundles = new ArrayList<>();
    private final List<GraphElementDto> elements = new ArrayList<>();
    private final List<GraphRelationDto> relations = new ArrayList<>();

    /**
     * Adds the statements outside any bundle.
     *
     * @param statements the statements, in ProvToolbox's reading order
     * @param view CPF-Toolbox's view of them
     */
    void addTopLevel(List<Statement> statements, CpmDocument view) {
        add(new ScopeMapper(null, statements, view, null, elements.size(), relations.size()));
    }

    /**
     * Adds a bundle and its statements.
     *
     * @param view CPF-Toolbox's view of the bundle
     * @param tiStrategy the view's TI strategy
     */
    void addBundle(Bundle bundle, CpmDocument view, ITIStrategy tiStrategy) {
        String key = "b" + (bundles.size() + 1);
        bundles.add(new GraphBundleDto(
                key,
                StatementMapping.qualifiedName(bundle.getId()),
                StatementMapping.iri(bundle.getId()),
                StatementMapping.prefixes(bundle.getNamespace())));
        add(new ScopeMapper(key, bundle.getStatement(), view, tiStrategy, elements.size(), relations.size()));
    }

    /** @param prefixes the document's prefixes */
    GraphDto build(Map<String, String> prefixes) {
        return new GraphDto(prefixes, bundles, elements, relations, List.of(), List.of());
    }

    private void add(ScopeMapper scope) {
        elements.addAll(scope.elements());
        relations.addAll(scope.relations());
    }
}

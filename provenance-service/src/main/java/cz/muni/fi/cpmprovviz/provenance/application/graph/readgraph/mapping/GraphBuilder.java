package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping;

import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphBundleDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphRelationDto;
import java.util.ArrayList;
import java.util.List;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Namespace;

/**
 * Builds the graph of one document from CPF-Toolbox's views of its bundles, one {@link BundleMapper} per bundle. Keys
 * bundles in the order they are added and numbers elements and relations across the whole document. The service reads
 * only single-bundle documents for now, so it adds one bundle. Created per document.
 */
public final class GraphBuilder {

    private final List<GraphBundleDto> bundles = new ArrayList<>();
    private final List<GraphElementDto> elements = new ArrayList<>();
    private final List<GraphRelationDto> relations = new ArrayList<>();

    /**
     * Adds a bundle and its statements.
     *
     * @param view CPF-Toolbox's view of the bundle
     * @param tiStrategy the view's TI strategy, which {@link CpmDocument} doesn't give out
     */
    public void addBundle(Bundle bundle, CpmDocument view, ITIStrategy tiStrategy) {
        String key = "b" + (bundles.size() + 1);
        bundles.add(new GraphBundleDto(
                key,
                ProvNames.qualifiedName(bundle.getId()),
                ProvNames.iri(bundle.getId()),
                ProvNames.prefixes(bundle.getNamespace())));
        add(new BundleMapper(key, bundle.getStatement(), view, tiStrategy, elements.size(), relations.size()));
    }

    /** @param namespace the document's namespace */
    public GraphDto build(Namespace namespace) {
        return new GraphDto(ProvNames.prefixes(namespace), bundles, elements, relations, List.of(), List.of());
    }

    private void add(BundleMapper bundle) {
        elements.addAll(bundle.elements());
        relations.addAll(bundle.relations());
    }
}

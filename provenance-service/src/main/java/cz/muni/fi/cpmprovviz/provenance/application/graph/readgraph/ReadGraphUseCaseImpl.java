package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpm.divided.ordered.CpmOrderedFactory;
import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpm.strategy.CpmTypeHasTIStrategy;
import cz.muni.fi.cpm.vanilla.CpmProvFactory;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.NotOneBundleException;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.UnbuildableCpmViewException;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping.GraphBuilder;
import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping.ProvNames;
import cz.muni.fi.cpmprovviz.provenance.application.ports.ProvDocumentReader;
import java.util.ArrayList;
import java.util.List;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Document;
import org.openprovenance.prov.model.StatementOrBundle;
import org.openprovenance.prov.vanilla.ProvFactory;

public class ReadGraphUseCaseImpl implements ReadGraphUseCase {

    /** CPF-Toolbox's default TI strategy, the one a {@link CpmDocument} starts with. Stateless. */
    private static final ITIStrategy TI_STRATEGY = new CpmTypeHasTIStrategy();

    private final ProvDocumentReader documentReader;

    public ReadGraphUseCaseImpl(ProvDocumentReader documentReader) {
        this.documentReader = documentReader;
    }

    @Override
    public GraphDto execute(ReadGraphRequest request) {
        Document document = documentReader.read(request.document());
        Bundle bundle = onlyBundle(document);

        GraphBuilder graph = new GraphBuilder();
        graph.addBundle(bundle, bundleView(bundle), TI_STRATEGY);
        return graph.build(document.getNamespace());
    }

    /**
     * The document's bundle. CPF-Toolbox reads a document as CPM only when it is one bundle and nothing else, and the
     * viewer works with a single bundle for now.
     */
    private static Bundle onlyBundle(Document document) {
        List<StatementOrBundle> contents = document.getStatementOrBundle();
        if (contents.size() == 1 && contents.getFirst() instanceof Bundle bundle) {
            return bundle;
        }
        int bundles = (int) contents.stream().filter(Bundle.class::isInstance).count();
        throw new NotOneBundleException(bundles, contents.size() - bundles);
    }

    /** The bundle's {@link CpmDocument}, with {@link CpmOrderedFactory} and the default TI strategy. */
    private static CpmDocument bundleView(Bundle bundle) {
        try {
            ProvFactory provFactory = new ProvFactory();
            CpmDocument view = new CpmDocument(
                    new ArrayList<>(bundle.getStatement()),
                    bundle.getId(),
                    provFactory,
                    new CpmProvFactory(provFactory),
                    new CpmOrderedFactory(provFactory));
            view.setTIStrategy(TI_STRATEGY);
            return view;
        } catch (RuntimeException e) {
            throw new UnbuildableCpmViewException(ProvNames.qualifiedName(bundle.getId()), e);
        }
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpm.divided.ordered.CpmOrderedFactory;
import cz.muni.fi.cpm.model.CpmDocument;
import cz.muni.fi.cpm.model.ITIStrategy;
import cz.muni.fi.cpm.strategy.CpmTypeHasTIStrategy;
import cz.muni.fi.cpm.vanilla.CpmProvFactory;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.ports.ProvDocumentReader;
import java.util.ArrayList;
import java.util.List;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Document;
import org.openprovenance.prov.model.ProvUtilities;
import org.openprovenance.prov.model.Statement;
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

        List<Statement> topLevel = new ArrayList<>();
        List<Bundle> bundles = new ArrayList<>();
        for (StatementOrBundle statementOrBundle : document.getStatementOrBundle()) {
            switch (statementOrBundle) {
                case Bundle bundle -> bundles.add(bundle);
                case Statement statement -> topLevel.add(statement);
                default -> {}
            }
        }

        GraphBuilder graph = new GraphBuilder();
        graph.addTopLevel(topLevel, topLevelView(topLevel));
        for (Bundle bundle : bundles) {
            graph.addBundle(bundle, bundleView(bundle), TI_STRATEGY);
        }
        return graph.build(StatementMapping.prefixes(document.getNamespace()));
    }

    /** One {@link CpmDocument} per bundle, with {@link CpmOrderedFactory} and the default TI strategy. */
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
            throw new UnbuildableCpmViewException(StatementMapping.qualifiedName(bundle.getId()), e);
        }
    }

    /**
     * The statements outside any bundle, read like a bundle's so that they are linked the same way. A
     * {@link CpmDocument} without a bundle identifier, since they have none.
     */
    private static CpmDocument topLevelView(List<Statement> statements) {
        try {
            ProvFactory provFactory = new ProvFactory();
            CpmDocument view =
                    new CpmDocument(provFactory, new CpmProvFactory(provFactory), new CpmOrderedFactory(provFactory));
            new ProvUtilities().forAllStatement(statements, view);
            return view;
        } catch (RuntimeException e) {
            throw new UnbuildableCpmViewException(null, e);
        }
    }
}

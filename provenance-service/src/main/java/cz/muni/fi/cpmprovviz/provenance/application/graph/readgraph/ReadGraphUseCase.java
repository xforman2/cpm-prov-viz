package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpmprovviz.provenance.application.exceptions.NotOneBundleException;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.UnbuildableCpmViewException;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.UnreadableDocumentException;
import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;

/**
 * Reads a PROV document with CPF-Toolbox and returns its graph. Nothing is stored. The document must be exactly one
 * bundle for now.
 */
public interface ReadGraphUseCase {

    /**
     * @throws UnreadableDocumentException when the document can't be read
     * @throws NotOneBundleException when the document is not exactly one bundle
     * @throws UnbuildableCpmViewException when CPF-Toolbox can't build the CPM view of the bundle
     */
    GraphDto execute(ReadGraphRequest request);
}

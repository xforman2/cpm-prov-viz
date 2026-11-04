package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.ports.UnreadableDocumentException;

/** Reads a PROV document with CPF-Toolbox and returns its graph. Nothing is stored. */
public interface ReadGraphUseCase {

    /**
     * @throws UnreadableDocumentException when the document can't be read
     * @throws UnbuildableCpmViewException when CPF-Toolbox can't build the CPM view of a bundle or of the statements
     *     outside any bundle
     */
    GraphDto execute(ReadGraphRequest request);
}

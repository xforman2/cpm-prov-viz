package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpmprovviz.provenance.application.document.contract.DocumentDto;
import cz.muni.fi.cpmprovviz.provenance.application.ports.UnreadableDocumentException;

/** Reads a PROV document with CPF-Toolbox and returns what the toolbox makes of it. Nothing is stored. */
public interface IReadDocumentUseCase {

    /**
     * @throws UnreadableDocumentException   when the document can't be read
     * @throws UnbuildableCpmViewException when the toolbox can't build a bundle's CPM view
     */
    DocumentDto execute(ReadDocumentRequest request);
}

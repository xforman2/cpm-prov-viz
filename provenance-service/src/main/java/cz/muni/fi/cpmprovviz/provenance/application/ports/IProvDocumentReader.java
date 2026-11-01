package cz.muni.fi.cpmprovviz.provenance.application.ports;

import org.openprovenance.prov.model.Document;

/** Reads a serialised PROV document into ProvToolbox's model. */
public interface IProvDocumentReader {

    /**
     * Reads the document.
     *
     * @param document the serialised document, as received
     * @return the document as ProvToolbox reads it
     * @throws UnreadableDocumentException when the document can't be read
     */
    Document read(byte[] document);
}

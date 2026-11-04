package cz.muni.fi.cpmprovviz.provenance.infrastructure;

import cz.muni.fi.cpmprovviz.provenance.application.ports.ProvDocumentReader;
import cz.muni.fi.cpmprovviz.provenance.application.ports.UnreadableDocumentException;
import java.io.ByteArrayInputStream;
import org.openprovenance.prov.core.json.serialization.ProvDeserialiser;
import org.openprovenance.prov.model.Document;

/** Reads PROV-JSON with ProvToolbox's reader, as patched by CPF-Toolbox's cpm-template. */
public class ProvJsonDocumentReader implements ProvDocumentReader {

    @Override
    public Document read(byte[] document) {
        try {
            return new ProvDeserialiser().deserialiseDocument(new ByteArrayInputStream(document));
        } catch (RuntimeException e) {
            throw new UnreadableDocumentException("ProvToolbox could not read the document as PROV-JSON.", e);
        }
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.exceptions;

import cz.muni.fi.cpmprovviz.provenance.application.ports.ProvDocumentReader;
import java.io.Serial;

/** Thrown by {@link ProvDocumentReader} when a document can't be read. */
public class UnreadableDocumentException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    public UnreadableDocumentException(String message, Throwable cause) {
        super(message, cause);
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.ports;

import java.io.Serial;

/** Thrown by {@link ProvDocumentReader} when a document can't be read. */
public class UnreadableDocumentException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    public UnreadableDocumentException(String message, Throwable cause) {
        super(message, cause);
    }
}

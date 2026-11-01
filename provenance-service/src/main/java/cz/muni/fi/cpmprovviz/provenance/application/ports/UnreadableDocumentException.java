package cz.muni.fi.cpmprovviz.provenance.application.ports;

/** Thrown by {@link IProvDocumentReader} when a document can't be read. */
public class UnreadableDocumentException extends RuntimeException {

    public UnreadableDocumentException(String message, Throwable cause) {
        super(message, cause);
    }
}

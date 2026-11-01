package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

/**
 * @param document the serialised PROV-JSON document, as received
 */
public record ReadDocumentRequest(byte[] document) {
}

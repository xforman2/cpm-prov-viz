package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

/** @param document the serialised PROV-JSON document, as received */
public record ReadGraphRequest(byte[] document) {}

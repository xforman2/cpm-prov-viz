package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

/** The CPM part of a bundle a relation is in, by {@code CpmDocument}'s part getters. */
public enum RelationPart {
    /** On an edge of {@code getTraversalInformationPart()}. */
    TRAVERSAL_INFORMATION,
    /** On an edge of {@code getDomainSpecificPart()}. */
    DOMAIN_SPECIFIC,
    /** On an edge of {@code getCrossPartEdges()}. */
    JOINING,
    /** On an edge with an end missing from the bundle. */
    NONE
}

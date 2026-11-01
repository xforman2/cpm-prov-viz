package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

/** The CPM part of a bundle an element is in, by {@code CpmDocument}'s part getters. */
public enum ElementPart {
    /** In {@code getTraversalInformationPart()}. */
    TRAVERSAL_INFORMATION,
    /** In {@code getDomainSpecificPart()}. */
    DOMAIN_SPECIFIC
}

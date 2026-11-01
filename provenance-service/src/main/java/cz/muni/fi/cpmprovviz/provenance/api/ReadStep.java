package cz.muni.fi.cpmprovviz.provenance.api;

/** The step of reading a document that failed, as a problem response names it in {@code step}. */
public enum ReadStep {
    /** Reading the PROV-JSON. */
    READ_JSON,
    /** Building a bundle's CPM view with CPF-Toolbox. */
    BUILD_CPM_VIEW
}

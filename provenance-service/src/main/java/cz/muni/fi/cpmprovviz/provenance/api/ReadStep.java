package cz.muni.fi.cpmprovviz.provenance.api;

import com.fasterxml.jackson.annotation.JsonProperty;

/** The step of reading a document that failed, as a problem response names it in {@code step}. */
public enum ReadStep {
    /** Reading the PROV-JSON with ProvToolbox. */
    @JsonProperty("readJson")
    READ_JSON,
    /** Checking that the document is exactly one bundle. */
    @JsonProperty("checkBundle")
    CHECK_BUNDLE,
    /** Building the CPM view of the document's bundle with CPF-Toolbox. */
    @JsonProperty("buildCpmView")
    BUILD_CPM_VIEW
}

package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph;

import java.io.Serial;

/** Thrown when CPF-Toolbox can't build the CPM view of a bundle or of the statements outside any bundle. */
public class UnbuildableCpmViewException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    private final String bundleId;

    /** @param bundleId qualified name of the bundle, or {@code null} for the statements outside any bundle */
    public UnbuildableCpmViewException(String bundleId, Throwable cause) {
        super(
                bundleId == null
                        ? "Building the CPM view of the statements outside any bundle failed."
                        : "Building the CPM view of bundle " + bundleId + " failed.",
                cause);
        this.bundleId = bundleId;
    }

    /** Qualified name of the bundle, or {@code null} for the statements outside any bundle. */
    public String getBundleId() {
        return bundleId;
    }
}

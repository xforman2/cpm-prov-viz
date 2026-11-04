package cz.muni.fi.cpmprovviz.provenance.application.exceptions;

import java.io.Serial;

/** Thrown when CPF-Toolbox can't build the CPM view of the document's bundle. */
public class UnbuildableCpmViewException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    private final String bundleId;

    /** @param bundleId qualified name of the bundle */
    public UnbuildableCpmViewException(String bundleId, Throwable cause) {
        super("Building the CPM view of bundle " + bundleId + " failed.", cause);
        this.bundleId = bundleId;
    }

    /** Qualified name of the bundle. */
    public String getBundleId() {
        return bundleId;
    }
}

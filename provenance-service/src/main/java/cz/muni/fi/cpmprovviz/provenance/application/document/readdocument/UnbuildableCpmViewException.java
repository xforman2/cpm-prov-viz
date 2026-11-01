package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

/** Thrown when CPF-Toolbox can't build the CPM view of a bundle. */
public class UnbuildableCpmViewException extends RuntimeException {

    private final String bundleId;

    public UnbuildableCpmViewException(String bundleId, Throwable cause) {
        super("CPF-Toolbox could not build the CPM view of bundle " + bundleId, cause);
        this.bundleId = bundleId;
    }

    /** Qualified name of the bundle. */
    public String getBundleId() {
        return bundleId;
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.exceptions;

import java.io.Serial;

/**
 * Thrown when a document is not exactly one bundle. CPF-Toolbox reads a document as CPM only when it is one bundle and
 * nothing else ({@code CpmDocument}), and the viewer works with a single bundle for now.
 */
public class NotOneBundleException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * @param bundles how many bundles the document has
     * @param statementsOutside how many statements it has outside any bundle
     */
    public NotOneBundleException(int bundles, int statementsOutside) {
        super("The document has " + count(bundles, "bundle") + " and " + count(statementsOutside, "statement")
                + " outside any bundle; it must be exactly one bundle and nothing else.");
    }

    private static String count(int count, String noun) {
        return count + " " + noun + (count == 1 ? "" : "s");
    }
}

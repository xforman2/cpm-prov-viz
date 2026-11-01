package cz.muni.fi.cpmprovviz.provenance.api;

import cz.muni.fi.cpmprovviz.provenance.application.document.readdocument.UnbuildableCpmViewException;
import cz.muni.fi.cpmprovviz.provenance.application.ports.UnreadableDocumentException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ProblemDetail;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

/**
 * Turns a failed read into a 400 problem response. {@code step} names the step that failed,
 * {@code bundle} the bundle for {@link ReadStep#BUILD_CPM_VIEW}, and {@code detail} says why.
 */
@RestControllerAdvice
public class ProblemHandler {

    @ExceptionHandler(UnreadableDocumentException.class)
    public ProblemDetail unreadableDocument(UnreadableDocumentException exception) {
        ProblemDetail problem = ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST, reason(exception));
        problem.setTitle("The document could not be read as PROV-JSON");
        problem.setProperty("step", ReadStep.READ_JSON);
        return problem;
    }

    @ExceptionHandler(UnbuildableCpmViewException.class)
    public ProblemDetail unbuildableCpmView(UnbuildableCpmViewException exception) {
        ProblemDetail problem = ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST, reason(exception));
        problem.setTitle("The CPM view of a bundle could not be built");
        problem.setProperty("step", ReadStep.BUILD_CPM_VIEW);
        problem.setProperty("bundle", exception.getBundleId());
        return problem;
    }

    /** The exception's message followed by each cause's, as {@code message <- Type: message}. */
    private static String reason(Exception exception) {
        StringBuilder reason = new StringBuilder(String.valueOf(exception.getMessage()));
        Throwable cause = exception.getCause();
        while (cause != null && cause != cause.getCause()) {
            reason.append(" <- ").append(cause.getClass().getSimpleName()).append(": ").append(cause.getMessage());
            cause = cause.getCause();
        }
        return reason.toString();
    }
}

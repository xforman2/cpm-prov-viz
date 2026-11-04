package cz.muni.fi.cpmprovviz.provenance.api;

import cz.muni.fi.cpmprovviz.provenance.application.exceptions.NotOneBundleException;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.UnbuildableCpmViewException;
import cz.muni.fi.cpmprovviz.provenance.application.exceptions.UnreadableDocumentException;
import java.util.ArrayList;
import java.util.List;
import org.springframework.http.HttpStatus;
import org.springframework.http.ProblemDetail;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

/**
 * Turns a failed read into a 400 problem response, which the frontend shows as its {@code title} followed by its
 * {@code detail}. {@code title} names the step that failed, {@code detail} says why, {@code step} names the step for a
 * program and {@code bundle} the bundle for {@link ReadStep#BUILD_CPM_VIEW}.
 */
@RestControllerAdvice
public class ProblemHandler {

    @ExceptionHandler(UnreadableDocumentException.class)
    public ProblemDetail unreadableDocument(UnreadableDocumentException exception) {
        ProblemDetail problem = ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST, reason(exception));
        problem.setTitle("Reading the document as PROV-JSON failed.");
        problem.setProperty("step", ReadStep.READ_JSON);
        return problem;
    }

    @ExceptionHandler(NotOneBundleException.class)
    public ProblemDetail notOneBundle(NotOneBundleException exception) {
        ProblemDetail problem = ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST, exception.getMessage());
        problem.setTitle("The document is not a single bundle.");
        problem.setProperty("step", ReadStep.CHECK_BUNDLE);
        return problem;
    }

    @ExceptionHandler(UnbuildableCpmViewException.class)
    public ProblemDetail unbuildableCpmView(UnbuildableCpmViewException exception) {
        ProblemDetail problem = ProblemDetail.forStatusAndDetail(HttpStatus.BAD_REQUEST, reason(exception));
        problem.setTitle(exception.getMessage());
        problem.setProperty("step", ReadStep.BUILD_CPM_VIEW);
        problem.setProperty("bundle", exception.getBundleId());
        return problem;
    }

    /** Each cause of the exception as {@code Type: message}, outermost first, or its own message if it has none. */
    private static String reason(Exception exception) {
        List<String> causes = new ArrayList<>();
        Throwable cause = exception.getCause();
        while (cause != null) {
            causes.add(cause.getClass().getSimpleName() + ": " + cause.getMessage());
            cause = cause.getCause() == cause ? null : cause.getCause();
        }
        return causes.isEmpty() ? exception.getMessage() : String.join(", caused by ", causes);
    }
}

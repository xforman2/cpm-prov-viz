package cz.muni.fi.cpmprovviz.provenance.api;

import cz.muni.fi.cpmprovviz.provenance.application.document.contract.DocumentDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.readdocument.IReadDocumentUseCase;
import cz.muni.fi.cpmprovviz.provenance.application.document.readdocument.ReadDocumentRequest;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.IOException;
import java.io.InputStream;

@RestController
@RequestMapping("/documents")
public class DocumentController {

    private final IReadDocumentUseCase readDocumentUseCase;

    public DocumentController(IReadDocumentUseCase readDocumentUseCase) {
        this.readDocumentUseCase = readDocumentUseCase;
    }

    /**
     * Reads the raw PROV-JSON document in the body, whatever its content type. Nothing is stored.
     * The body is read from the request stream itself, because {@code @RequestBody} rebuilds a
     * form-encoded body from its parameters.
     */
    @PostMapping(path = "/read", produces = MediaType.APPLICATION_JSON_VALUE)
    public DocumentDto read(InputStream body) throws IOException {
        return readDocumentUseCase.execute(new ReadDocumentRequest(body.readAllBytes()));
    }
}

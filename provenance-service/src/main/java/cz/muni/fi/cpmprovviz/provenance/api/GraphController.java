package cz.muni.fi.cpmprovviz.provenance.api;

import cz.muni.fi.cpmprovviz.provenance.application.graph.contract.GraphDto;
import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.ReadGraphRequest;
import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.ReadGraphUseCase;
import java.io.IOException;
import java.io.InputStream;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api")
public class GraphController {

    private final ReadGraphUseCase readGraphUseCase;

    public GraphController(ReadGraphUseCase readGraphUseCase) {
        this.readGraphUseCase = readGraphUseCase;
    }

    /**
     * Reads the raw PROV-JSON document in the body, whatever its content type, and returns its graph. Nothing is
     * stored. The body is read from the request stream itself, because {@code @RequestBody} rebuilds a form-encoded
     * body from its parameters.
     */
    @PostMapping(path = "/graph", produces = MediaType.APPLICATION_JSON_VALUE)
    public GraphDto readGraph(InputStream body) throws IOException {
        return readGraphUseCase.execute(new ReadGraphRequest(body.readAllBytes()));
    }
}

package cz.muni.fi.cpmprovviz.provenance.application;

import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.ReadGraphUseCase;
import cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.ReadGraphUseCaseImpl;
import cz.muni.fi.cpmprovviz.provenance.application.ports.ProvDocumentReader;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/** Registers the application layer's use cases, each by its interface. */
@Configuration
public class ApplicationConfiguration {

    @Bean
    public ReadGraphUseCase readGraphUseCase(ProvDocumentReader documentReader) {
        return new ReadGraphUseCaseImpl(documentReader);
    }
}

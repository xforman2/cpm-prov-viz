package cz.muni.fi.cpmprovviz.provenance.application;

import cz.muni.fi.cpmprovviz.provenance.application.document.readdocument.IReadDocumentUseCase;
import cz.muni.fi.cpmprovviz.provenance.application.document.readdocument.ReadDocumentUseCase;
import cz.muni.fi.cpmprovviz.provenance.application.ports.IProvDocumentReader;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/** Registers the application layer's use cases, each by its interface. */
@Configuration
public class ApplicationConfiguration {

    @Bean
    public IReadDocumentUseCase readDocumentUseCase(IProvDocumentReader documentReader) {
        return new ReadDocumentUseCase(documentReader);
    }
}

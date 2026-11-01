package cz.muni.fi.cpmprovviz.provenance.infrastructure;

import cz.muni.fi.cpmprovviz.provenance.application.ports.IProvDocumentReader;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/** Registers the infrastructure layer's implementations of the application's ports, each by its port. */
@Configuration
public class InfrastructureConfiguration {

    @Bean
    public IProvDocumentReader provDocumentReader() {
        return new ProvJsonDocumentReader();
    }
}

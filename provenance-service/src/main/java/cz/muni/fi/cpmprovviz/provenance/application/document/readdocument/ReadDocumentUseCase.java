package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpmprovviz.provenance.application.document.contract.BundleDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.DocumentDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.RelationDto;
import cz.muni.fi.cpmprovviz.provenance.application.ports.IProvDocumentReader;
import org.openprovenance.prov.model.Bundle;
import org.openprovenance.prov.model.Document;
import org.openprovenance.prov.model.Element;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.StatementOrBundle;

import java.util.ArrayList;
import java.util.List;

public class ReadDocumentUseCase implements IReadDocumentUseCase {

    private final IProvDocumentReader documentReader;

    public ReadDocumentUseCase(IProvDocumentReader documentReader) {
        this.documentReader = documentReader;
    }

    @Override
    public DocumentDto execute(ReadDocumentRequest request) {
        Document document = documentReader.read(request.document());

        List<ElementDto> elements = new ArrayList<>();
        List<RelationDto> relations = new ArrayList<>();
        List<BundleDto> bundles = new ArrayList<>();
        for (StatementOrBundle statementOrBundle : document.getStatementOrBundle()) {
            switch (statementOrBundle) {
                case Bundle bundle -> bundles.add(readBundle(bundle));
                case Element element -> elements.add(StatementMapping.element(List.of(element), null, null));
                case Relation relation -> relations.add(StatementMapping.relation(relation, null));
                default -> {
                }
            }
        }

        return new DocumentDto(StatementMapping.prefixes(document.getNamespace()), elements, relations, bundles);
    }

    private static BundleDto readBundle(Bundle bundle) {
        BundleViewReader reader;
        try {
            reader = new BundleViewReader(bundle);
        } catch (RuntimeException e) {
            throw new UnbuildableCpmViewException(StatementMapping.qualifiedName(bundle.getId()), e);
        }
        return reader.read();
    }
}

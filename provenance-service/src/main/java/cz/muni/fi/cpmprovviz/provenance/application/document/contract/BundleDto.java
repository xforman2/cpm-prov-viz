package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

import java.util.List;
import java.util.Map;

/**
 * A bundle, as the CPM view CPF-Toolbox builds of it (one {@code CpmDocument} per bundle).
 *
 * @param id        qualified name, e.g. {@code blank:SamplingBundle_V0}
 * @param iri       expanded IRI
 * @param prefixes  the bundle's prefix declarations, prefix → namespace IRI
 * @param elements  one entry per toolbox node (identifier and kind)
 * @param relations one entry per relation, never per toolbox edge
 */
public record BundleDto(
        String id,
        String iri,
        Map<String, String> prefixes,
        List<ElementDto> elements,
        List<RelationDto> relations) {
}

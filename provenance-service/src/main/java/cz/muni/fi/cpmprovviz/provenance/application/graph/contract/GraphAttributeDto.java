package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.List;

/**
 * An attribute of an element or a relation. ProvToolbox gives the attributes by kind ({@code prov:label},
 * {@code prov:type}, …, then the others), not in the order written. There is one entry per name, in the order each name
 * first comes.
 *
 * @param name qualified name of the attribute, e.g. {@code prov:type}
 * @param iri expanded IRI of the name
 * @param values the attribute's values, in the order ProvToolbox gives them, at least one
 */
public record GraphAttributeDto(String name, String iri, List<GraphAttributeValueDto> values) {}

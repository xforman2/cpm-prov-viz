package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

/**
 * One attribute value, e.g. one {@code prov:type}.
 *
 * @param name        qualified name of the attribute, e.g. {@code prov:type}
 * @param nameIri     expanded IRI of the name
 * @param value       the value's text
 * @param valueIri    for a qualified name value, its IRI; otherwise {@code null}
 * @param datatype    qualified name of the datatype, e.g. {@code xsd:string}, or {@code null}
 * @param datatypeIri expanded IRI of the datatype, or {@code null}
 * @param language    the language tag, or {@code null}
 */
public record AttributeDto(
        String name,
        String nameIri,
        String value,
        String valueIri,
        String datatype,
        String datatypeIri,
        String language) {
}

package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

/**
 * One value of an attribute, as ProvToolbox reads it. ProvToolbox gives every value a datatype, a bare JSON value too:
 * a string is an {@code xsd:string}.
 *
 * @param value the value's text: a qualified name as {@code prefix:local}, a language-tagged string without its tag,
 *     anything else as ProvToolbox writes it out
 * @param datatype qualified name of the datatype ProvToolbox gives the value, e.g. {@code xsd:string} or
 *     {@code prov:QUALIFIED_NAME}, or {@code null} when it gives none
 * @param datatypeIri expanded IRI of {@code datatype}, or {@code null} when there is no {@code datatype}
 * @param language the language tag, e.g. {@code en}, or {@code null} when the value has none
 * @param iri for a value ProvToolbox reads as a qualified name, the IRI it expands to; otherwise {@code null}. A plain
 *     string that looks like a qualified name is not one.
 * @param element key of the element in the same bundle whose {@code iri} equals {@code iri}, the first one when several
 *     do, or {@code null} when there is none or the value is not a qualified name
 */
public record GraphAttributeValueDto(
        String value, String datatype, String datatypeIri, String language, String iri, String element) {}

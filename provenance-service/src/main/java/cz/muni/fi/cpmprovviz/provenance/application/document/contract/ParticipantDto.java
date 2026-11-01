package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

/**
 * A participant of a relation other than its effect and cause.
 *
 * @param name the participant's PROV name, e.g. {@code prov:plan}
 * @param iri  IRI of what it names
 */
public record ParticipantDto(String name, String iri) {
}

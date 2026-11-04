package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

/**
 * An argument of a relation other than its two main ones, such as a derivation's activity or an association's plan. It
 * is not drawn as an edge.
 *
 * @param name the argument's PROV name, e.g. {@code prov:plan}
 * @param nameIri expanded IRI of {@code name}
 * @param id the identifier the argument names
 * @param iri expanded IRI of {@code id}
 * @param element key of the element in the same bundle whose {@code iri} equals {@code iri}, the first one when several
 *     do, or {@code null} when there is none
 * @param relation key of the relation in the same place whose {@code iri} equals {@code iri}, as
 *     {@code prov:generation} and {@code prov:usage} name relations; {@code null} when there is none or {@code element}
 *     is set
 */
public record GraphParticipantDto(
        String name, String nameIri, String id, String iri, String element, String relation) {}

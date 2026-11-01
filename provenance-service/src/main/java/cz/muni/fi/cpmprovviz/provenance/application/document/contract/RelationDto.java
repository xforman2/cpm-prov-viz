package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

import org.openprovenance.prov.model.StatementOrBundle.Kind;

import java.util.List;

/**
 * A relation. Effect and cause are ProvToolbox's ({@code ProvUtilities.getEffect} / {@code getCause}),
 * e.g. a generation's entity and activity.
 *
 * @param id           qualified name, or {@code null} when the relation has none
 * @param iri          expanded IRI of the identifier, or {@code null}
 * @param kind         ProvToolbox kind, e.g. {@code PROV_GENERATION}
 * @param effect       IRI of the effect, or {@code null} when not given
 * @param cause        IRI of the cause, or {@code null} when not given
 * @param participants the other participants, e.g. a derivation's activity or an association's plan
 * @param attributes   the attributes, in order ({@code getAttributes()})
 * @param time         the relation's time in XML Schema form, or {@code null}
 * @param part         the toolbox's CPM part; {@code null} outside any bundle
 */
public record RelationDto(
        String id,
        String iri,
        Kind kind,
        String effect,
        String cause,
        List<ParticipantDto> participants,
        List<AttributeDto> attributes,
        String time,
        RelationPart part) {
}

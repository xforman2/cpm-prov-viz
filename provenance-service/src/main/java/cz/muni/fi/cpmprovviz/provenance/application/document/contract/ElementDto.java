package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

import cz.muni.fi.cpm.constants.CpmType;
import org.openprovenance.prov.model.StatementOrBundle.Kind;

import java.util.List;

/**
 * An entity, activity or agent. In a bundle it stands for one toolbox node, which merges every
 * declaration of the same identifier and kind.
 *
 * @param id         qualified name
 * @param iri        expanded IRI
 * @param kind       ProvToolbox kind, e.g. {@code PROV_ENTITY}
 * @param part       the toolbox's CPM part; {@code null} outside any bundle
 * @param cpmRoles   every {@link CpmType} the toolbox finds on the node ({@code CpmUtilities.hasCpmType}),
 *                   in {@link CpmType} order; {@code null} outside any bundle
 * @param attributes the attributes of every declaration, in order ({@code getAttributes()})
 * @param startTime  an activity's start time in XML Schema form, or {@code null}
 * @param endTime    an activity's end time in XML Schema form, or {@code null}
 */
public record ElementDto(
        String id,
        String iri,
        Kind kind,
        ElementPart part,
        List<CpmType> cpmRoles,
        List<AttributeDto> attributes,
        String startTime,
        String endTime) {
}

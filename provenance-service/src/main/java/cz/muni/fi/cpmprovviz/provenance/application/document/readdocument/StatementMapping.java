package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpm.constants.CpmType;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.AttributeDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ElementDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ElementPart;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.ParticipantDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.RelationDto;
import cz.muni.fi.cpmprovviz.provenance.application.document.contract.RelationPart;
import org.openprovenance.prov.model.ActedOnBehalfOf;
import org.openprovenance.prov.model.Activity;
import org.openprovenance.prov.model.Attribute;
import org.openprovenance.prov.model.Element;
import org.openprovenance.prov.model.HadMember;
import org.openprovenance.prov.model.HasTime;
import org.openprovenance.prov.model.Identifiable;
import org.openprovenance.prov.model.LangString;
import org.openprovenance.prov.model.MentionOf;
import org.openprovenance.prov.model.Namespace;
import org.openprovenance.prov.model.ProvUtilities;
import org.openprovenance.prov.model.QualifiedName;
import org.openprovenance.prov.model.Relation;
import org.openprovenance.prov.model.Statement;
import org.openprovenance.prov.model.WasAssociatedWith;
import org.openprovenance.prov.model.WasDerivedFrom;
import org.openprovenance.prov.model.WasEndedBy;
import org.openprovenance.prov.model.WasStartedBy;
import org.openprovenance.prov.vanilla.HasAttributes;

import javax.xml.datatype.XMLGregorianCalendar;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.function.Function;

/** Flattens ProvToolbox statements into the contract. Stateless. */
final class StatementMapping {

    private static final ProvUtilities PROV_UTILITIES = new ProvUtilities();

    private StatementMapping() {
    }

    /**
     * @param declarations every declaration of one element, at least one
     * @param part         the toolbox's part, or {@code null} outside any bundle
     * @param cpmRoles     the toolbox's CPM types, or {@code null} outside any bundle
     */
    static ElementDto element(List<Element> declarations, ElementPart part, List<CpmType> cpmRoles) {
        Element first = declarations.getFirst();
        List<AttributeDto> attributes = declarations.stream()
                .flatMap(declaration -> attributes(declaration).stream())
                .toList();
        return new ElementDto(
                qualifiedName(first.getId()), iri(first.getId()), first.getKind(), part, cpmRoles, attributes,
                firstActivityTime(declarations, Activity::getStartTime),
                firstActivityTime(declarations, Activity::getEndTime));
    }

    /**
     * @param part the toolbox's part, or {@code null} outside any bundle
     */
    static RelationDto relation(Relation relation, RelationPart part) {
        QualifiedName id = relation instanceof Identifiable identifiable ? identifiable.getId() : null;
        return new RelationDto(
                qualifiedName(id),
                iri(id),
                relation.getKind(),
                iri(PROV_UTILITIES.getEffect(relation)),
                iri(PROV_UTILITIES.getCause(relation)),
                participants(relation),
                attributes(relation),
                relation instanceof HasTime hasTime ? time(hasTime.getTime()) : null,
                part);
    }

    /** Prefix → namespace IRI, with a default namespace under {@code default}, as PROV-JSON writes it. */
    static Map<String, String> prefixes(Namespace namespace) {
        Map<String, String> prefixes = new LinkedHashMap<>();
        if (namespace == null) {
            return prefixes;
        }
        if (namespace.getDefaultNamespace() != null) {
            prefixes.put("default", namespace.getDefaultNamespace());
        }
        prefixes.putAll(namespace.getPrefixes());
        return prefixes;
    }

    /** The qualified name as {@code prefix:local}, or just the local part without a prefix. */
    static String qualifiedName(QualifiedName name) {
        if (name == null) {
            return null;
        }
        String prefix = name.getPrefix();
        return prefix == null || prefix.isEmpty() ? name.getLocalPart() : prefix + ":" + name.getLocalPart();
    }

    static String iri(QualifiedName name) {
        return name == null ? null : name.getUri();
    }

    private static List<AttributeDto> attributes(Statement statement) {
        if (!(statement instanceof HasAttributes hasAttributes)) {
            return List.of();
        }
        return hasAttributes.getAttributes().stream().map(StatementMapping::attribute).toList();
    }

    private static AttributeDto attribute(Attribute attribute) {
        Object value = attribute.getValue();
        String text = switch (value) {
            case LangString langString -> langString.getValue();
            case QualifiedName name -> qualifiedName(name);
            case null -> null;
            default -> String.valueOf(value);
        };
        return new AttributeDto(
                qualifiedName(attribute.getElementName()),
                iri(attribute.getElementName()),
                text,
                value instanceof QualifiedName name ? iri(name) : null,
                qualifiedName(attribute.getType()),
                iri(attribute.getType()),
                value instanceof LangString langString ? langString.getLang() : null);
    }

    private static List<ParticipantDto> participants(Relation relation) {
        List<ParticipantDto> participants = new ArrayList<>();
        switch (relation) {
            case WasDerivedFrom derivation -> {
                addParticipant(participants, "prov:activity", derivation.getActivity());
                addParticipant(participants, "prov:generation", derivation.getGeneration());
                addParticipant(participants, "prov:usage", derivation.getUsage());
            }
            case WasStartedBy start -> addParticipant(participants, "prov:starter", start.getStarter());
            case WasEndedBy end -> addParticipant(participants, "prov:ender", end.getEnder());
            case WasAssociatedWith association -> addParticipant(participants, "prov:plan", association.getPlan());
            case ActedOnBehalfOf delegation -> addParticipant(participants, "prov:activity", delegation.getActivity());
            case MentionOf mention -> addParticipant(participants, "prov:bundle", mention.getBundle());
            case HadMember membership -> membership.getEntity().stream()
                    .skip(1)
                    .forEach(member -> addParticipant(participants, "prov:entity", member));
            default -> {
            }
        }
        return participants;
    }

    private static void addParticipant(List<ParticipantDto> participants, String name, QualifiedName value) {
        if (value != null) {
            participants.add(new ParticipantDto(name, iri(value)));
        }
    }

    /** The first time an activity declaration gives, or {@code null}. */
    private static String firstActivityTime(
            List<Element> declarations, Function<Activity, XMLGregorianCalendar> time) {
        return declarations.stream()
                .filter(Activity.class::isInstance)
                .map(declaration -> time.apply((Activity) declaration))
                .filter(Objects::nonNull)
                .findFirst()
                .map(StatementMapping::time)
                .orElse(null);
    }

    private static String time(XMLGregorianCalendar time) {
        return time == null ? null : time.toXMLFormat();
    }
}

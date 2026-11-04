package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.List;

/**
 * An entity, activity or agent: every declaration of one identifier and kind in the same bundle (or outside any
 * bundle), which CPF-Toolbox reads as one node.
 *
 * @param key key of the element
 * @param id identifier
 * @param iri expanded IRI
 * @param kind PROV element kind
 * @param bundle key of the bundle holding the element, or {@code null} for an element outside any bundle
 * @param attributes the attributes of every declaration, as ProvToolbox gives them ({@code getAttributes()}). An
 *     activity's {@code prov:startTime} and {@code prov:endTime} are not among them: ProvToolbox keeps them apart.
 * @param cpmRoles the CPM roles CPF-Toolbox finds on the element ({@code CpmUtilities.hasCpmType}), in the order of
 *     {@link CpmRole}. A role counts when a {@code prov:type} value of any declaration is a qualified name with the
 *     prefix {@code cpm} and the namespace {@code https://www.commonprovenancemodel.org/cpm-namespace-v1-0/} whose
 *     local part names the role, and the role fits the element's kind: connectors are entities, the main activity is an
 *     activity, and the sender, receiver and current agent are agents. {@code cpm:id} is not a role, and neither is a
 *     plain string such as {@code "cpm:mainActivity"}. Empty when the element has none.
 * @param membership for an element in a bundle, {@code traversalInformation} when CPF-Toolbox's default TI strategy
 *     ({@code CpmTypeHasTIStrategy}) puts it in the traversal information, otherwise {@code domainSpecificInformation}.
 *     The strategy counts the CPM types among the {@code prov:type} values (with the {@code cpm} prefix and namespace,
 *     {@code cpm:id} left out) without checking the element's kind: exactly one, or only agent roles, is traversal
 *     information, whatever other types the element has. An element can be traversal information with no
 *     {@code cpmRoles}, e.g. an entity typed {@code cpm:mainActivity}. {@code null} for an element outside any bundle.
 */
public record GraphElementDto(
        String key,
        String id,
        String iri,
        ElementKind kind,
        String bundle,
        List<GraphAttributeDto> attributes,
        List<CpmRole> cpmRoles,
        ElementMembership membership) {}

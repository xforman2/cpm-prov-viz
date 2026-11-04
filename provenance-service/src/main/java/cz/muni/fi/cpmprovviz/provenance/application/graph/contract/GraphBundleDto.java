package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.Map;

/**
 * A bundle: a named set of PROV statements in the document. CPF-Toolbox reads each bundle as one CPM document.
 *
 * @param key key of the bundle
 * @param id identifier
 * @param iri expanded IRI
 * @param prefixes the bundle's own prefixes as ProvToolbox gives them, prefix → namespace IRI, with a default namespace
 *     under {@code default}: the ones written, plus any ProvToolbox adds, such as {@code _} when the bundle has blank
 *     identifiers. Empty if it has none.
 */
public record GraphBundleDto(String key, String id, String iri, Map<String, String> prefixes) {}

package cz.muni.fi.cpmprovviz.provenance.application.document.contract;

import java.util.List;
import java.util.Map;

/**
 * A PROV document as CPF-Toolbox reads it, flattened. Elements are referred to by IRI. Everything
 * comes in the order ProvToolbox reads it, which is not always the order written: it sorts a
 * bundle's statements by section and keeps bundles in a hash map.
 *
 * @param prefixes  the document's prefix declarations, prefix → namespace IRI
 * @param elements  the elements outside any bundle, without CPM part or roles
 * @param relations the relations outside any bundle, without CPM part
 * @param bundles   one entry per bundle
 */
public record DocumentDto(
        Map<String, String> prefixes,
        List<ElementDto> elements,
        List<RelationDto> relations,
        List<BundleDto> bundles) {
}

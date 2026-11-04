package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.List;
import java.util.Map;

/**
 * The graph of a PROV document, as the frontend draws it.
 *
 * <p>The graph contract between the provenance service and the frontend is this record and the records and enums in
 * this package. Their Javadoc is its specification. Change them first, then the frontend.
 *
 * <p>The graph describes meaning only, never styling. Everything refers to a bundle, element or relation by its key,
 * never by its identifier: the same identifier can appear in several bundles (a connector does exactly that), and a
 * relation can have no identifier at all. Keys are unique in the graph and the same every time the same document is
 * read: bundles are keyed {@code b1}, {@code b2}, …, elements {@code e1}, {@code e2}, … and relations {@code r1},
 * {@code r2}, …. They follow ProvToolbox's reading order, which is not the order written: ProvToolbox sorts a bundle's
 * statements by section and keeps bundles in a hash map. The statements outside any bundle come first, then each bundle
 * in turn. Within each, the elements come in the order of their first declaration, then the relations in the order of
 * CPF-Toolbox's edges: a relation with the same kind, identifier (or none) and arguments as an earlier one shares its
 * edge and comes right after it.
 *
 * <p>Every value is the one ProvToolbox and CPF-Toolbox give. An {@code id} is a qualified name as
 * {@code prefix:local}, e.g. {@code blank:Sampling}, or the local part alone when it has no prefix. Its {@code iri} is
 * the IRI ProvToolbox expands it to.
 *
 * <p>The statements outside any bundle and each bundle are read on their own: a statement refers only to elements in
 * the same bundle, or outside any bundle for a statement outside any bundle. Within one, CPF-Toolbox reads every
 * declaration of an element with the same identifier and kind as one element.
 *
 * @param prefixes the document's prefix declarations as ProvToolbox gives them, prefix → namespace IRI. A default
 *     namespace is under {@code default}.
 * @param bundles one entry per bundle in the document. Bundles never nest.
 * @param elements one entry per entity, activity and agent, from every bundle and from outside any bundle
 * @param relations one entry per relation, from every bundle and from outside any bundle
 * @param parseNotes remarks from reading the document leniently. Lenient reading with parse notes is not planned, so
 *     this list is always empty.
 * @param validationFindings violations of PROV-DM or CPM rules. The validators are not built yet, so this list is
 *     always empty.
 */
public record GraphDto(
        Map<String, String> prefixes,
        List<GraphBundleDto> bundles,
        List<GraphElementDto> elements,
        List<GraphRelationDto> relations,
        List<ParseNoteDto> parseNotes,
        List<ValidationFindingDto> validationFindings) {}

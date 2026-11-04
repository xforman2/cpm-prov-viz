package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

import java.util.List;
import java.util.Map;

/**
 * The graph of a PROV document, as the frontend draws it.
 *
 * <p>The graph contract between the provenance service and the frontend is this record and the records and enums in
 * this package. Their Javadoc is its specification. Change them first, then the frontend.
 *
 * <p>For now the document must be exactly one bundle and nothing else, as CPF-Toolbox requires to read a document as
 * CPM ({@code CpmDocument}); any other document is rejected. The contract keeps a list of bundles and a bundle key on
 * every element and relation, so that it can carry several bundles later. Until then, {@code bundles} has one entry,
 * and no element or relation is outside a bundle.
 *
 * <p>The graph describes meaning only, never styling. Everything refers to a bundle, element or relation by its key,
 * never by its identifier: the same identifier can appear in several bundles (a connector does exactly that), and a
 * relation can have no identifier at all. Keys are unique in the graph and the same every time the same document is
 * read: bundles are keyed {@code b1}, {@code b2}, …, elements {@code e1}, {@code e2}, … and relations {@code r1},
 * {@code r2}, …. They follow ProvToolbox's reading order, which is not the order written: ProvToolbox sorts a bundle's
 * statements by section. Within a bundle, the elements come in the order of their first declaration, then the relations
 * in the order of CPF-Toolbox's edges: a relation with the same kind, identifier (or none) and arguments as an earlier
 * one shares its edge and comes right after it.
 *
 * <p>Every value is the one ProvToolbox and CPF-Toolbox give. An {@code id} is a qualified name as
 * {@code prefix:local}, e.g. {@code blank:Sampling}, or the local part alone when it has no prefix. Its {@code iri} is
 * the IRI ProvToolbox expands it to.
 *
 * <p>Each bundle is read on its own: a statement refers only to elements in the same bundle. Within a bundle,
 * CPF-Toolbox reads every declaration of an element with the same identifier and kind as one element.
 *
 * @param prefixes the document's prefix declarations as ProvToolbox gives them, prefix → namespace IRI. A default
 *     namespace is under {@code default}.
 * @param bundles one entry per bundle in the document: exactly one for now. Bundles never nest.
 * @param elements one entry per entity, activity and agent, from every bundle
 * @param relations one entry per relation, from every bundle
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

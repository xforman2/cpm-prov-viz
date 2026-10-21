using CpmProvViz.Domain.Prov;

namespace CpmProvViz.Application.Graph;

// The graph contract between the backend and the frontend. These records and their comments are
// its specification, published in the OpenAPI document. Change them first, then the frontend.

/// <summary>
/// The graph of a PROV document, as the frontend draws it. It describes meaning only, never styling.
/// Everything refers to a bundle, element or relation by its key, never by its identifier: the same
/// identifier can appear in several bundles (a connector does exactly that), and a relation can have
/// no identifier at all. Keys are unique in the graph and the same every time the same document is
/// read: bundles are keyed <c>b1</c>, <c>b2</c>, …, elements <c>e1</c>, <c>e2</c>, … and relations
/// <c>r1</c>, <c>r2</c>, …, in reading order: the top-level statements first, then each bundle in
/// turn, and within each the entities, activities and agents, then the relations section by section.
/// An <c>id</c> is a qualified name as written in the document, e.g. <c>blank:Sampling</c>. Its
/// <c>iri</c> is the namespace of its prefix followed by the local part, split at the first <c>:</c>;
/// a name without a <c>:</c> uses the <c>default</c> prefix. Prefixes are looked up in the bundle's
/// own <c>prefix</c> (for a bundle's identifier and everything inside it), then the document's
/// <c>prefix</c>, then the implicit <c>prov</c> and <c>xsd</c>. If the prefix is declared nowhere,
/// the <c>iri</c> is the identifier as written.
/// </summary>
/// <param name="Prefixes">The document's own prefix declarations, prefix → namespace IRI, as written.</param>
/// <param name="Bundles">One entry per bundle in the document. Bundles never nest.</param>
/// <param name="Elements">One entry per entity, activity and agent, from every bundle and from the top level of the document.</param>
/// <param name="Relations">One entry per relation record, from every bundle and from the top level of the document.</param>
/// <param name="ParseNotes">Remarks from reading the document leniently. Always empty for now.</param>
/// <param name="ValidationFindings">Violations of PROV-DM or CPM rules. The validators are not built yet, so this list is always empty.</param>
public sealed record Graph(
    IReadOnlyDictionary<string, string> Prefixes,
    IReadOnlyList<GraphBundle> Bundles,
    IReadOnlyList<GraphElement> Elements,
    IReadOnlyList<GraphRelation> Relations,
    IReadOnlyList<ParseNote> ParseNotes,
    IReadOnlyList<ValidationFinding> ValidationFindings);

/// <summary>A bundle: a named set of PROV statements in the document.</summary>
/// <param name="Key">Key of the bundle.</param>
/// <param name="Id">Identifier as written.</param>
/// <param name="Iri">Expanded IRI, resolved with the bundle's own prefixes first.</param>
/// <param name="Prefixes">The bundle's own prefix declarations, as written. Empty if it has none.</param>
public sealed record GraphBundle(
    string Key,
    string Id,
    string Iri,
    IReadOnlyDictionary<string, string> Prefixes);

/// <summary>An entity, activity or agent.</summary>
/// <param name="Key">Key of the element.</param>
/// <param name="Id">Identifier as written.</param>
/// <param name="Iri">Expanded IRI.</param>
/// <param name="Kind">PROV element kind.</param>
/// <param name="Bundle">Key of the bundle holding the element, or <c>null</c> for a top-level element.</param>
/// <param name="Attributes">The element's attributes, in document order.</param>
public sealed record GraphElement(
    string Key,
    string Id,
    string Iri,
    ElementKind Kind,
    string? Bundle,
    IReadOnlyList<GraphAttribute> Attributes);

/// <summary>
/// A relation record. <c>source</c> and <c>target</c> are its two main arguments in PROV-DM, so an
/// edge from <c>source</c> to <c>target</c> points the way PROV defines it: towards the past. By
/// <c>type</c>, they are: <c>wasGeneratedBy</c> <c>prov:entity</c> → <c>prov:activity</c>,
/// <c>used</c> <c>prov:activity</c> → <c>prov:entity</c>, <c>wasInformedBy</c> <c>prov:informed</c>
/// → <c>prov:informant</c>, <c>wasStartedBy</c> and <c>wasEndedBy</c> <c>prov:activity</c> →
/// <c>prov:trigger</c>, <c>wasInvalidatedBy</c> <c>prov:entity</c> → <c>prov:activity</c>,
/// <c>wasDerivedFrom</c> <c>prov:generatedEntity</c> → <c>prov:usedEntity</c>,
/// <c>wasAttributedTo</c> <c>prov:entity</c> → <c>prov:agent</c>, <c>wasAssociatedWith</c>
/// <c>prov:activity</c> → <c>prov:agent</c>, <c>actedOnBehalfOf</c> <c>prov:delegate</c> →
/// <c>prov:responsible</c>, <c>wasInfluencedBy</c> <c>prov:influencee</c> → <c>prov:influencer</c>,
/// <c>specializationOf</c> and <c>mentionOf</c> <c>prov:specificEntity</c> →
/// <c>prov:generalEntity</c>, <c>alternateOf</c> <c>prov:alternate1</c> → <c>prov:alternate2</c>,
/// <c>hadMember</c> <c>prov:collection</c> → <c>prov:entity</c>.
/// </summary>
/// <param name="Key">Key of the relation.</param>
/// <param name="Type">The relation type, named like its PROV-JSON section.</param>
/// <param name="Id">Identifier as written, or <c>null</c> when the relation has none. Blank identifiers (<c>_:…</c>) carry no meaning in PROV-JSON and are <c>null</c>.</param>
/// <param name="Iri">Expanded IRI of <c>id</c>.</param>
/// <param name="Bundle">Key of the bundle holding the relation, or <c>null</c> at the top level.</param>
/// <param name="Source">Key of the element named by the first main argument, in the same bundle (or at the top level, for a top-level relation). <c>null</c> when the argument is missing, is not a string, or names no element there.</param>
/// <param name="Target">Key of the element named by the second main argument, under the same rules as <c>source</c>.</param>
/// <param name="Attributes">Every key of the record other than the two main arguments, in document order. Other arguments, such as a derivation's <c>prov:activity</c> or an association's <c>prov:plan</c>, are listed here for now, and so is an argument that is not a string, such as the array of members in <c>hadMember</c>.</param>
public sealed record GraphRelation(
    string Key,
    RelationType Type,
    string? Id,
    string? Iri,
    string? Bundle,
    string? Source,
    string? Target,
    IReadOnlyList<GraphAttribute> Attributes);

/// <summary>An attribute of an element or a relation.</summary>
/// <param name="Name">Attribute name as written, e.g. <c>prov:type</c>.</param>
/// <param name="Iri">Expanded IRI of the name.</param>
/// <param name="Values">The values, in order: one per item when the attribute's value is an array, a single one when it is bare. A value of a shape not listed for <c>value</c> is left out.</param>
public sealed record GraphAttribute(string Name, string Iri, IReadOnlyList<GraphAttributeValue> Values);

/// <summary>One value of an attribute.</summary>
/// <param name="Value">The value's text: a string as is, a number or boolean as its JSON text, and a typed or language-tagged literal (<c>{"$": …, "type": …}</c>, <c>{"$": …, "lang": …}</c>) as the text of its <c>$</c>.</param>
public sealed record GraphAttributeValue(string Value);

/// <summary>
/// A remark from reading the document leniently: something skipped, merged or not declared. It says
/// nothing about whether the document is valid.
/// </summary>
/// <param name="Message">What the reader did, in one sentence.</param>
/// <param name="Subject">Key of the bundle, element or relation the note concerns, or <c>null</c> for the whole document.</param>
public sealed record ParseNote(string Message, string? Subject);

/// <summary>A violation of PROV-DM or CPM rules, reported by a validator.</summary>
/// <param name="Message">The violation, in one sentence.</param>
/// <param name="Subject">Key of the bundle, element or relation the finding concerns, or <c>null</c> for the whole document.</param>
public sealed record ValidationFinding(string Message, string? Subject);

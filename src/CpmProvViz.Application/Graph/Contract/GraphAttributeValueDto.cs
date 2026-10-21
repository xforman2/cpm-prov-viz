namespace CpmProvViz.Application.Graph.Contract;

/// <summary>
/// One value of an attribute, in full form: what the document states, nothing inferred. A bare JSON
/// value (string, number or boolean) has no datatype and no language tag here, whatever it looks like.
/// </summary>
/// <param name="Value">The value's text: a string as is, a number or boolean as its JSON text, and a typed or language-tagged literal (<c>{"$": …, "type": …}</c>, <c>{"$": …, "lang": …}</c>) as the text of its <c>$</c>.</param>
/// <param name="Datatype">The literal's <c>type</c> as written, e.g. <c>xsd:dateTime</c>, or <c>null</c> when it has none.</param>
/// <param name="DatatypeIri">Expanded IRI of <c>datatype</c>, as written: the two <c>xsd</c> namespaces are not unified here. <c>null</c> when there is no <c>datatype</c>.</param>
/// <param name="Language">The literal's <c>lang</c>, e.g. <c>en</c>, or <c>null</c> when it has none.</param>
/// <param name="Iri">For a qualified name, the IRI <c>value</c> expands to, resolved like an <c>id</c> in the same bundle; otherwise <c>null</c>. A value is a qualified name only when its <c>datatype</c> expands to <c>prov:QUALIFIED_NAME</c> or <c>xsd:QName</c>, where <c>http://www.w3.org/2001/XMLSchema#</c> and <c>http://www.w3.org/2000/10/XMLSchema#</c> both count as <c>xsd</c>. A plain string that looks like a qualified name is not one.</param>
/// <param name="Element">Key of the element in the same bundle (or at the top level, for a top-level statement) whose <c>iri</c> equals <c>iri</c>, or <c>null</c> when there is none or the value is not a qualified name.</param>
public sealed record GraphAttributeValueDto(
    string Value,
    string? Datatype,
    string? DatatypeIri,
    string? Language,
    string? Iri,
    string? Element);

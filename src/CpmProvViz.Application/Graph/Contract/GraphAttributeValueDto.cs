namespace CpmProvViz.Application.Graph.Contract;

/// <summary>One value of an attribute.</summary>
/// <param name="Value">The value's text: a string as is, a number or boolean as its JSON text, and a typed or language-tagged literal (<c>{"$": …, "type": …}</c>, <c>{"$": …, "lang": …}</c>) as the text of its <c>$</c>.</param>
public sealed record GraphAttributeValueDto(string Value);

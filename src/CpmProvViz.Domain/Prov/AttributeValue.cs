namespace CpmProvViz.Domain.Prov;

/// <summary>An attribute value as the document states it.</summary>
/// <param name="Value">The text of the literal.</param>
/// <param name="Datatype">The literal's datatype, or <c>null</c> when none is written.</param>
/// <param name="Language">The literal's language tag, or <c>null</c> when none is written.</param>
/// <param name="QualifiedName">
/// The value read as a qualified name, when its <see cref="Datatype"/> makes it one
/// (<see cref="Datatypes.IsQualifiedName"/>); otherwise <c>null</c>.
/// </param>
public sealed record AttributeValue(
    string Value, QualifiedName? Datatype, string? Language, QualifiedName? QualifiedName);

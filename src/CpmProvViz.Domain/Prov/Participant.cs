namespace CpmProvViz.Domain.Prov;

/// <summary>
/// An argument of a relation other than its two main ones, such as a derivation's activity.
/// </summary>
/// <param name="Name">The argument's key, e.g. <c>prov:plan</c>.</param>
/// <param name="Id">The identifier the argument names.</param>
public sealed record Participant(QualifiedName Name, QualifiedName Id);

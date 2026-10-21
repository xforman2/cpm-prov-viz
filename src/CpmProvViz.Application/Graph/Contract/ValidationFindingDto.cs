namespace CpmProvViz.Application.Graph.Contract;

/// <summary>A violation of PROV-DM or CPM rules, reported by a validator.</summary>
/// <param name="Message">The violation, in one sentence.</param>
/// <param name="Subject">Key of the bundle, element or relation the finding concerns, or <c>null</c> for the whole document.</param>
public sealed record ValidationFindingDto(string Message, string? Subject);

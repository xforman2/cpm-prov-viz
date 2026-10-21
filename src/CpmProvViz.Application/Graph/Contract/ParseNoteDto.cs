namespace CpmProvViz.Application.Graph.Contract;

/// <summary>
/// A remark from reading the document leniently: something skipped, merged or not declared. It says
/// nothing about whether the document is valid.
/// </summary>
/// <param name="Message">What the reader did, in one sentence.</param>
/// <param name="Subject">Key of the bundle, element or relation the note concerns, or <c>null</c> for the whole document.</param>
public sealed record ParseNoteDto(string Message, string? Subject);

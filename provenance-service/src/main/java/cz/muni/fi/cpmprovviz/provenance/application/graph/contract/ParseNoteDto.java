package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

/**
 * A remark from reading the document leniently: something skipped, merged or not declared. It says nothing about
 * whether the document is valid.
 *
 * @param message what the reader did, in one sentence
 * @param subject key of the bundle, element or relation the note concerns, or {@code null} for the whole document
 */
public record ParseNoteDto(String message, String subject) {}

package cz.muni.fi.cpmprovviz.provenance.application.graph.contract;

/**
 * A violation of PROV-DM or CPM rules, reported by a validator.
 *
 * @param message the violation, in one sentence
 * @param subject key of the bundle, element or relation the finding concerns, or {@code null} for the whole document
 */
public record ValidationFindingDto(String message, String subject) {}

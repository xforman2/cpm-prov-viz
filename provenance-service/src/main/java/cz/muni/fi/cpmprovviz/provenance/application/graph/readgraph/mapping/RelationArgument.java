package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping;

import org.openprovenance.prov.model.QualifiedName;

/**
 * An argument of a relation other than its effect and cause, e.g. an association's plan.
 *
 * @param name the argument's local name in the PROV namespace, e.g. {@code plan}
 * @param value the identifier it names, or {@code null} when the relation doesn't give it
 */
record RelationArgument(String name, QualifiedName value) {}

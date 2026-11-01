package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpm.model.INode;
import org.openprovenance.prov.model.QualifiedName;
import org.openprovenance.prov.model.StatementOrBundle.Kind;

/** What makes a toolbox node: its identifier and kind. The part getters clone nodes, so they are matched by key. */
record NodeKey(QualifiedName id, Kind kind) {

    static NodeKey of(INode node) {
        return new NodeKey(node.getId(), node.getKind());
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.document.readdocument;

import cz.muni.fi.cpm.model.IEdge;
import org.openprovenance.prov.model.StatementOrBundle.Kind;

/** What makes a toolbox edge between two nodes. The part getters clone edges, so they are matched by key. */
record EdgeKey(Kind kind, NodeKey effect, NodeKey cause) {

    static EdgeKey of(IEdge edge) {
        return new EdgeKey(edge.getKind(), NodeKey.of(edge.getEffect()), NodeKey.of(edge.getCause()));
    }
}

package cz.muni.fi.cpmprovviz.provenance.application.graph.readgraph.mapping;

import java.util.LinkedHashMap;
import java.util.Map;
import org.openprovenance.prov.model.Namespace;
import org.openprovenance.prov.model.QualifiedName;

/** Turns ProvToolbox's qualified names and namespaces into the contract's text. Stateless. */
public final class ProvNames {

    private ProvNames() {}

    /** The qualified name as {@code prefix:local}, or just the local part without a prefix. */
    public static String qualifiedName(QualifiedName name) {
        if (name == null) {
            return null;
        }
        String prefix = name.getPrefix();
        return prefix == null || prefix.isEmpty() ? name.getLocalPart() : prefix + ":" + name.getLocalPart();
    }

    /** The IRI the qualified name expands to. */
    public static String iri(QualifiedName name) {
        return name == null ? null : name.getUri();
    }

    /** Prefix → namespace IRI, with a default namespace under {@code default}, as PROV-JSON writes it. */
    public static Map<String, String> prefixes(Namespace namespace) {
        Map<String, String> prefixes = new LinkedHashMap<>();
        if (namespace == null) {
            return prefixes;
        }
        if (namespace.getDefaultNamespace() != null) {
            prefixes.put("default", namespace.getDefaultNamespace());
        }
        prefixes.putAll(namespace.getPrefixes());
        return prefixes;
    }
}

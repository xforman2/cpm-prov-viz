# Hand-made test documents

Small PROV-JSON documents written by hand to cover cases the EMBRC documents don't.

| File | Covers |
| --- | --- |
| `two-bundles.json` | Two bundles sharing a connector identifier, a bundle with its own prefixes, top-level statements about the bundles, typed and language-tagged values, bare values and arrays. |
| `values-and-participants.json` | Attribute values in full form (datatypes, language tags, several values), `prov:QUALIFIED_NAME` and `xsd:QName` under the older XML Schema namespace, a qualified-name value naming an element in the same bundle and one naming nothing, a plain string that looks like a qualified name, `schema:name` labels, and relations with extra participants (a derivation's activity, generation and usage, an association's plan). |

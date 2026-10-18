# PROV-JSON and the Common Provenance Model: reference for the parser and CPM checker

Research notes gathered on 2026-10-18 for the parser spec and the design interview.
Primary sources only: W3C specifications, the PROV-JSON Member Submission and its schema,
the CPM authors' papers and website, and first-party source code (the commits read are listed in
[Sources](#sources)).

Markers used below:

- **[V]**: verified by reading the primary source (spec text, schema or source code) myself.
- **[R]**: stated by a first-party source (e.g. a CPF test report or README), but I did not check
  the code behind it.
- **[U]**: could not verify, or inferred.

Unmarked statements in the summary repeat marked statements further down.

---

## Summary: the facts a parser and a CPM checker need most

1. **PROV-JSON is a W3C Member Submission (24 April 2013), not a Recommendation.** Its JSON
   Schema (draft-04) disagrees with its own text in several places: it misspells `wasEndedBy`,
   requires `prov:entity` on `used`, and has no `mentionOf` or dictionary sections. Use
   PROV-DM/PROV-N for semantics and the submission text for syntax, and do not validate input
   against the W3C schema as it stands. [V] (§A.9)
2. **Top level:** an object with `prefix`, one map per record type (`entity`, `activity`, `agent`,
   `wasGeneratedBy`, `used`, …, `specializationOf`, `alternateOf`, `hadMember`) keyed by
   identifier, and `bundle` (a map from bundle id to a nested document without `bundle`).
   `mentionOf` is not in the submission, but both reference libraries read and write it. [V] (§A.2, §A.3)
3. **Every record is keyed by an identifier.** Relations without one get blank keys `_:…`. These
   "MUST be created", "do not carry any semantics and SHOULD be ignored". [V] (§A.4)
4. **Attribute values** are a JSON string (`xsd:string`), number, boolean, `{"$", "type"}`,
   `{"$", "lang"}`, or an array of these. Qualified-name values are typed `xsd:QName` in the
   submission, but `prov:QUALIFIED_NAME` in everything the CPM tools emit. Read both. [V] (§A.5)
5. **Repeated identifiers:** the Python `prov` package writes an **array of objects** under one
   key. The submission and its schema do not allow this, and ProvToolbox's current reader rejects
   it. PROV-CONSTRAINTS says that records with the same id are **merged**: attribute sets are
   unioned and the formal arguments must unify. [V] (§A.7)
6. **Bundles** may declare their own `prefix`. Bundle-local declarations shadow the document's,
   and must not nest. A bundle's provenance is described by an `entity` with the same id and
   `prov:type` `prov:Bundle`. PROV-CONSTRAINTS treats every bundle as an independent instance:
   the same id in two bundles is not merged. [V] (§A.8)
7. **CPM has no freely available normative text.** The normative standard is ISO 23494-2:2026,
   published June 2026, 65 pp., paywalled. The best free sources are the CPF preprint (Zenodo,
   Dec 2024; it carries the CPM type and attribute tables) and the reference implementation
   CPF-Toolbox (states that it reflects "the model's state as of Spring 2025"). The CPM website
   is marked "SITE UNDER CONSTRUCTION" and still shows the older 2022 vocabulary. [V] (§B.1)
8. **CPM namespace** used by all current tooling and data:
   `https://www.commonprovenancemodel.org/cpm-namespace-v1-0/`, prefix `cpm`. The preprint only
   says `https://www.commonprovenancemodel.org/`, and older examples use
   `http://commonprovenancemodel.org/` and `http://commonprovenancemodel.org/ns/0.1`. [V] (§B.2)
9. **CPM types** are `prov:type` values. The preprint has `cpm:backwardConnector`,
   `cpm:forwardConnector`, `cpm:mainActivity`, `cpm:senderAgent` and `cpm:receiverAgent`.
   CPF-Toolbox and CPF-Storage add `cpm:specForwardConnector`, `cpm:currentAgent` and `cpm:id`.
   The key attributes are `cpm:referencedBundleId`, `cpm:referencedMetaBundleId`,
   `cpm:referencedBundleHashValue`, `cpm:hashAlg` and `cpm:provenanceServiceUri`, and the
   implementations add `cpm:referencedBundleSpecV` and `cpm:referencedMetaBundleSpecV`.
   `cpm:metabundle`, `cpm:senderBundleId`, `cpm:receiverBundleId` and `cpm:currentConnector` are
   **legacy** terms (2022 paper, website, 2024 Python storage). [V] (§B.3, §B.4)
10. **Linking:** a connector is an entity whose **identifier is shared** between two bundles.
    It is a `cpm:forwardConnector` in the sender's bundle and a `cpm:backwardConnector` in the
    receiver's bundle. The backward connector carries `cpm:referencedBundleId` pointing at the
    sender's bundle. Forward links are optional: they are a separate entity
    `specializationOf` the forward connector (`cpm:specForwardConnector` in the
    implementations) carrying `cpm:referencedBundleId` of the receiver's bundle. [V] (§B.6)
11. **Traversal information vs domain-specific part:** nothing in the data marks the split
    explicitly. Membership follows from `cpm:` types. The reference implementation offers three
    pluggable heuristics, and its default puts a node in the traversal information iff it carries
    exactly one valid CPM type (several are allowed only if all are agent roles). Domain content
    attaches through `specializationOf` (domain entity → connector) and `dct:hasPart` (main
    activity → sub-activities). [V] (§B.5)
12. **No published CPM validator or SHACL shapes were found.** The only executable rules are in
    CPF-Toolbox (structure), CPF-Storage (Java, 2026; per-connector attribute checks and
    resolvability) and the 2024 Python Provenance Storage (legacy vocabulary). [V] (§B.8)

---

## A. PROV-JSON

### A.1 Sources and their status

| Document | Status | What it owns |
|---|---|---|
| PROV-JSON, W3C Member Submission, 24 April 2013 ([SUBM-prov-json-20130424](https://www.w3.org/submissions/2013/SUBM-prov-json-20130424/)) | Member Submission. "Publication … indicates no endorsement of its content by W3C" (Status of This Document) [V] | JSON syntax |
| PROV-JSON schema ([/submissions/prov-json/schema](https://www.w3.org/submissions/prov-json/schema), draft-04, `id` `http://provenance.ecs.soton.ac.uk/prov-json/schema#`) | §2.3 "This section is non-normative." [V] | — |
| PROV-DM, W3C Recommendation, 30 April 2013 | normative | semantics: required vs optional arguments, reserved attributes, values |
| PROV-N, W3C Recommendation, 30 April 2013 | normative | qualified names, literals, namespace scoping, bundles |
| PROV-CONSTRAINTS, W3C Recommendation, 30 April 2013 | normative | merging, key constraints, bundle independence |
| PROV-LINKS, W3C Working Group Note, 30 April 2013 | Note | `prov:mentionOf` |
| [PROV errata](https://www.w3.org/2011/prov/errata.html) | errata | wrong `xsd` namespace in the RECs |

The submission's "Latest editor's draft" link (`http://provenance.ecs.soton.ac.uk/prov-json/`) did
not resolve when fetched on 2026-10-18. [V]

### A.2 Top-level structure

From submission §2 "Overview" [V]:

> "each type of PROV assertions (e.g. entity, activity, generation, usage, etc.) is organised into a
> separate property with the same name in the top-level object"

```json
{
  "prefix":   { "default": "http://example.org/default", "ex": "http://example.org/" },
  "entity":   { "<id>": { "<attr>": <value>, ... } },
  "activity": { ... },
  "agent":    { ... },
  "<relationName>": { "<relationId>": { "prov:<role>": "<qualified name>", ..., "<attr>": <value> } },
  "bundle":   { "<bundleId>": { "prefix": {...}, "entity": {...}, ... } }
}
```

- `prefix` maps prefix → namespace IRI. "There is a special prefix called `default`, which can be
  used to declare the default namespace to which all unprefixed terms will be bound." (§2) [V]
- `prov` and `xsd` are implicitly defined (§2) [V]. The submission gives `xsd` as
  `http://www.w3.org/2000/10/XMLSchema#` (§2) [V]. That is the same mistake the PROV RECs made,
  and the PROV errata correct it to `http://www.w3.org/2001/XMLSchema#` (errata dm-1, o-1). Both
  libraries use `2001`: Python `prov` `src/prov/constants.py` line 18; ProvToolbox through
  `addKnownNamespaces()`. Treat `2000/10` as an alias of `2001`. [V for the errata and Python; U
  for the exact ProvToolbox constant]
- Schema: the top-level object has `additionalProperties: false`, so any key not listed is
  invalid, including `mentionOf`, the dictionary sections and the correctly spelled `wasEndedBy`
  (§A.9). [V]
- PROV-N forbids declaring `prov` or `xsd` ("A namespace declaration … MUST NOT declare prefixes
  prov and xsd", PROV-N §3.7.4). PROV-JSON says nothing on this, and CPF-Toolbox output declares
  both explicitly (CPF-Toolbox README "Basic example"). A reader must accept explicit `prov`/`xsd`
  entries and should check that they map to the standard IRIs. [V]

### A.3 Record sections and their keys

Section keys come from the submission §3 (examples), the schema `properties`, and the libraries'
key tables: Python `prov` `PROV_N_MAP` (`src/prov/constants.py` l.50–69), and ProvToolbox
`ProvJsonVocabulary.java` l.13–63. The attribute keys inside each record are `prov:` + the PROV-DM
argument name. The "Req. (PROV-DM)" column follows PROV-DM §5. [V]

| PROV-DM concept | Section key | Formal keys (PROV-DM argument → JSON key) | Required by PROV-DM | Required by W3C schema |
|---|---|---|---|---|
| Entity | `entity` | (id is the map key) | id | — |
| Activity | `activity` | `prov:startTime`, `prov:endTime` | id | — (startTime/endTime mis-placed, not enforced) |
| Agent | `agent` | — | id | — |
| Generation | `wasGeneratedBy` | `prov:entity`, `prov:activity`, `prov:time` | entity; plus at least one of id/activity/time/attrs | `prov:entity` |
| Usage | `used` | `prov:activity`, `prov:entity`, `prov:time` | **activity**; plus at least one of id/entity/time/attrs | `prov:entity` (**wrong**, see §A.9) |
| Communication | `wasInformedBy` | `prov:informed`, `prov:informant` | both | both |
| Start | `wasStartedBy` | `prov:activity`, `prov:trigger`, `prov:starter`, `prov:time` | activity; plus at least one of id/trigger/starter/time/attrs | `prov:activity` |
| End | `wasEndedBy` | `prov:activity`, `prov:trigger`, `prov:ender`, `prov:time` | activity; plus at least one of id/trigger/ender/time/attrs | (schema key misspelled `wasEndedby`) |
| Invalidation | `wasInvalidatedBy` | `prov:entity`, `prov:activity`, `prov:time` | entity; plus at least one of id/activity/time/attrs | `prov:entity` |
| Derivation (incl. Revision, Quotation, Primary Source via `prov:type`) | `wasDerivedFrom` | `prov:generatedEntity`, `prov:usedEntity`, `prov:activity`, `prov:generation`, `prov:usage` | generatedEntity, usedEntity | both entities |
| Attribution | `wasAttributedTo` | `prov:entity`, `prov:agent` | both | both |
| Association | `wasAssociatedWith` | `prov:activity`, `prov:agent`, `prov:plan` | activity; plus at least one of id/agent/plan/attrs | `prov:activity` |
| Delegation | `actedOnBehalfOf` | `prov:delegate`, `prov:responsible`, `prov:activity` | delegate, responsible | both |
| Influence | `wasInfluencedBy` | `prov:influencee`, `prov:influencer` | both | both |
| Specialization | `specializationOf` | `prov:specificEntity`, `prov:generalEntity` | both; **no id, no attributes** in PROV-DM | both |
| Alternate | `alternateOf` | `prov:alternate1`, `prov:alternate2` | both; **no id, no attributes** in PROV-DM | both |
| Membership | `hadMember` | `prov:collection`, `prov:entity` | both; no id/attributes in PROV-DM | both |
| Mention (PROV-LINKS) | `mentionOf` (not in submission) | `prov:specificEntity`, `prov:generalEntity`, `prov:bundle` | all three; no id, no attributes | not in schema |
| Bundle | `bundle` | nested document | id | — |
| Dictionary (PROV-Dictionary, submission App. B) | `hadDictionaryMember`, `derivedByInsertionFrom`, `derivedByRemovalFrom` | `prov:dictionary`, `prov:entity`, `prov:key`, `prov:after`, `prov:before`, `prov:key-entity-set`, `prov:key-set` | — | not in schema |

Notes:

- The PROV-DM "at least one of … MUST be present" rules are quoted in PROV-DM §5.1.3, §5.1.4,
  §5.1.6, §5.1.7, §5.1.8 and §5.3.3, and collected in PROV-N §3.7.5 Table 2. [V]
- Specialization: "A specialization is not, as defined here, also an influence, and therefore does
  not have an id and attributes." (PROV-DM §5.5.1). Alternate has the same wording (§5.5.2). PROV-JSON
  still requires a key, so these appear with `_:` keys. ProvToolbox reads an identifier or
  attributes on them as its "qualified" extension (`ProvJsonReader.java` l.148–165). The Python
  `prov` HISTORY (Unreleased) lists "an identifier or attribute block on `alternateOf`,
  `specializationOf` or `hadMember`" among inputs it will warn about under its strict PROV-N
  profile. [V]
- `hadMember`: both libraries accept `prov:entity` as an **array** of members, expanded into one
  membership each (ProvToolbox `ProvJsonReader.java` l.269–280 and writer l.185–196; Python
  `provjson.py` l.719–743). [V]
- Mention: PROV-LINKS §2 defines `prov:mentionOf(infra, supra, b)` with `specificEntity`,
  `generalEntity` and `bundle`, and constrains "IF mentionOf(e2,e1,b) THEN specializationOf(e2,e1)"
  (§5). The JSON key `mentionOf` and `prov:bundle` come only from the libraries
  (`ProvJsonVocabulary.java` l.30, l.63; Python `constants.py` l.67, l.145). [V]
- Ordering trap: in submission Example 37 (Alternate), `prov:alternate1`/`prov:alternate2` are
  swapped relative to the PROV-N argument order in Example 36. Map by key name, never by position. [V]
- Formal (relation-argument) values are **plain strings holding a qualified name**, not typed
  objects (all examples in §3.2, and schema `"type": "string"`). Times are `xsd:dateTime` strings:
  "The values of startTime and endTime MUST conform to xsd:dateTime" (§3.1.3). Python `prov` 3.x
  also accepts `{"$": …, "type": "xsd:dateTime"}` for times and a one-element array for a formal
  argument, with a warning (`provjson.py` l.774–793, l.864–901). ProvToolbox requires a JSON
  string for a reference (`ProvJsonReader.java` l.262–267). [V]
- Any non-formal key in a relation is an ordinary attribute, e.g. `ex:port` on `wasGeneratedBy`
  (§3.2, "Any values of additionalProperties are specified alongside the identifier pairs"). [V]

### A.4 Identifiers, prefixes, the default namespace, blank identifiers

- "An identifier in PROV-JSON, as in [PROV-DM], is a qualified name. A qualified name can be
  mapped into an IRI by concatenating the IRI associated with the prefix and the local part."
  (§2.1) [V]
- PROV-DM §5.7.5: "A qualified name's prefix is OPTIONAL. … In the absence of prefix, the
  qualified name refers to the default namespace." [V]
- PROV-N §3.7.1: the prefix follows SPARQL `PN_PREFIX`, and the local part is an extended
  `PN_LOCAL` that may contain `:`, so split at the **first** `:`. A local part such as `1234` is
  legal (PROV-N §3.7.3 note on `INT_LITERAL` vs `QUALIFIED_NAME`). Python `prov` documents the same
  first-colon split (`provjson.py` l.240–247). [V]
- Equivalent qualified names: "It is possible that there are two (or more) qualified names that
  resolves to the same IRI. PROV-JSON makes no provision to ensure that records referred by
  equivalent identifiers will be merged. It is the responsibility of the application consuming
  PROV-JSON to deal with such cases properly." (§2.1 Note). **The parser should key records by
  expanded IRI, not by the prefixed string.** [V]
- Blank node identifiers (§2.1) [V]:
  > "Although identifiers of relations are optional in PROV, they are still required at all time
  > in PROV-JSON … blank node identifiers MUST be created for such relations following the nodeID
  > production in Turtle (e.g. _:A1)."
  > "Blank node identifiers do not carry any semantics and SHOULD be ignored by applications
  > consuming PROV-JSON documents."

  So a relation id is *required as a JSON key* but semantically optional. Python `prov` writes
  `_:id<n>` (`provjson.py` l.54–71), and ProvToolbox writes `_:n<n>` and drops `_:` ids on read
  (`ProvJsonWriter.java` l.209, l.364–366; `ProvJsonReader.java` l.116, l.247–249). [V]
- Blank keys on **elements** (entity/activity/agent) are not covered by the submission (elements
  have mandatory ids, PROV-DM §5.7.1). ProvToolbox keeps such an id in a pseudo-namespace
  `https://openprovenance.org/blank#` (`ProvJsonVocabulary.java` l.89–91). The CPF test report
  says Python `prov` rejects `_:` as an entity id (CPF-Compatibility-Tests `TEST_REPORT.md` #32).
  [V for ProvToolbox; R for Python]
- The `blank:` **prefix** in CPF data (e.g. `"blank": "https://openprovenance.org/blank#"` in the
  EMBRC examples) is an ordinary namespace, not a blank node. [V]
- Unresolvable prefixes: the submission does not say what to do. Python `prov` 3.x drops an
  unresolvable formal reference with a warning, keeps an unresolvable QName value as an opaque
  literal, and warns about a record key that is a full IRI (`provjson.py` l.796–825, l.1044–1056;
  HISTORY "Unreleased"). ProvToolbox throws `ProvJsonException("cannot resolve …")`
  (`ProvJsonReader.java` l.253–260). [V]

### A.5 Attribute value forms

From submission §2.2 "JSON Data Typing" [V]:

| Form | JSON | Meaning |
|---|---|---|
| plain string | `"Workflow 1"` | `xsd:string` |
| number | `2`, `82.5e-2` | "xsd:decimal, xsd:string, and xsd:boolean values MAY be represented using the JSON native data types number, string, and boolean" |
| boolean | `true` | `xsd:boolean` |
| typed literal | `{"$": "1034", "type": "xsd:positiveInteger"}` | "The value of a literal is stored in the object's special property $, represented as a string; the data type in the type property, given as a qualified name." |
| language-tagged | `{"$": "Londres", "lang": "fr"}` | "if the xsd:string value has a language tag, the literal MUST be represented in a JSON object …, the language tag in the lang property and the type property omitted." |
| qualified name | `{"$": "prov:Person", "type": "xsd:QName"}` | submission Examples 7, 9, 25 etc.; PROV-N writes it `'prov:Person'` = `"prov:Person" %% prov:QUALIFIED_NAME` (PROV-N §3.7.3) |
| IRI | `{"$": "http://…", "type": "xsd:anyURI"}` | Example 29 |
| multiple values | `[ {...}, 2, {...} ]` | "Some keys in PROV-JSON may have more than one value. These MUST be represented as PROV-JSON literals as above and collated in to a JSON array" (§2.2) |

Where each form can appear:

- **Formal relation arguments and record keys**: plain qualified-name strings only (§A.3).
- **`prov:startTime`, `prov:endTime`, `prov:time`**: an `xsd:dateTime` string.
- **All other attributes**, including `prov:type`, `prov:label`, `prov:location`, `prov:role` and
  `prov:value`: any of the value forms above, or an array of them. Schema `attributeValues`
  allows string, number, boolean, typed literal, or a non-empty array of those, but never an
  array nested in an array or `null`. [V]

What the tools actually do with values:

- **Qualified-name type string.** The submission uses only `xsd:QName` and never mentions
  `prov:QUALIFIED_NAME` [V]. Python `prov` ≤ 2.x **writes** `{"$": …, "type": "prov:QUALIFIED_NAME"}`
  (`provjson.py` @2.0.2 l.292–295) and on read recognises only `prov:QUALIFIED_NAME`, so an
  `xsd:QName` value becomes a plain `Literal` (@2.0.2 `decode_json_representation`). Python
  `prov` ≥ 3.0.0 writes `xsd:QName` and reads both (`provjson.py` l.958–962, l.1044; HISTORY
  3.0.0 "#168"). ProvToolbox reads both (`ProvJsonReader.java` l.235–237) and writes
  `prov:QUALIFIED_NAME` unless the value carries another type (`ProvJsonWriter.java` l.312–314).
  CPF-Toolbox output and the CPM example data use `prov:QUALIFIED_NAME` throughout. [V]
- **Numbers.** Python `prov` 2.0.2 writes `{"$": 5, "type": "xsd:int"}` with a **numeric `$`**
  (@2.0.2 l.298–299, `LITERAL_XSDTYPE_MAP`). That is schema-invalid, because `$` must be a
  string, and 3.0.0 fixed it (HISTORY 3.0.0 "#246"). ProvToolbox reads a bare JSON number as
  `xsd:int`/`xsd:long`/`xsd:integer` when integral and as `xsd:decimal` otherwise
  (`ProvJsonReader.java` l.208–223). Accept a non-string `$` and coerce it. [V]
- **Language tag together with `type`.** The schema allows both keys at once (`typedLiteral` has
  `type` and `lang` as optional properties). ProvToolbox gives `lang` precedence and ignores
  `type` (l.230–232). [V]
- **Unknown keys in a value object**, or a `null` value: Python `prov` 3.x warns and keeps or
  drops them (`provjson.py` l.1023–1026, l.673–678). [V]
- **Bare QName-looking strings.** `"prov:type": "cpm:forwardConnector"` (a plain string) is
  strictly an `xsd:string`, **not** a qualified name. The CPF test report records Python output
  of `["cpm:forwardConnector", "prov:QUALIFIED_NAME"]` (two strings) as a "silent semantic
  error" (`TEST_REPORT.md` "FAIL Details", issue09_v2). The CPM checker must decide whether to
  accept bare strings as CPM types (open question). [V for the spec rule; R for the Python
  behaviour]

### A.6 Reserved attributes

PROV-DM §5.7.2, Table 8 [V]:

| Attribute | Allowed in | Value | Notes |
|---|---|---|---|
| `prov:label` | any construct | "The value associated with the attribute prov:label MUST be a string." | Language-tagged strings and repeats allowed (Example 48: `prov:label="Voiture 01"@fr, prov:label="Car 01"@en`) |
| `prov:location` | Entity, Activity, Agent, Usage, Generation, Invalidation, Start, End | "MUST be a PROV-DM Value" | Free form. CPM example data writes a QName-looking plain string here (`"prov:location": "gen:a237…"` in EMBRC Dataset2), which is ambiguous |
| `prov:role` | Usage, Generation, Invalidation, Association, Start, End | "MUST be a PROV-DM Value" | "allowed to occur multiple times" |
| `prov:type` | any construct | "MUST be a PROV-DM Value" | "allowed to occur multiple times". Predefined: `prov:Bundle`, `prov:Collection`, `prov:EmptyCollection`, `prov:Organization`, `prov:Person`, `prov:Plan`, `prov:PrimarySource`, `prov:Quotation`, `prov:Revision`, `prov:SoftwareAgent` (Table 9) |
| `prov:value` | Entity | "MUST be a PROV-DM Value" | "MAY occur at most once" |

PROV-DM §5.7.3: values are constants such as "a string, number, time, qualified name, IRI, and
encoded binary data". Use of RDF/XSD datatypes and of qualified names is RECOMMENDED. [V]

### A.7 Repeated identifiers and merging

- **In the submission:** each section is "a map-like structure … indexed by their identifiers"
  (§2), so a key can appear only once per section. The schema requires each record to be an
  **object**. Neither defines arrays of records. [V]
- **Python `prov` writes arrays.** When two records in one container share an identifier, it
  turns the value into a list (`provjson.py` l.424–436, and the same at @2.0.2 l.177–180). The
  reader accepts "a single JSON object (one instance of `rec_id`) or a JSON array of objects
  (multiple instances sharing the same identifier)" (l.465–495). [V]
- **ProvToolbox current master merges on write and rejects arrays on read.** The writer merges
  two statements with one id into one record: "two statements with one identifier make one
  record: PROV-JSON has no room for both". Formal properties must agree, otherwise it throws, and
  attribute values are unioned (`ProvJsonWriter.java` l.210–238). The reader calls `object(…)` on
  each record value, so an array throws `ProvJsonException` (`ProvJsonReader.java` l.77,
  l.292–295). [V]
- **Duplicate JSON keys** (`{"ex:k": "a", "ex:k": "b"}`): Python `prov` keeps the last value and
  warns (`provjson.py` l.119–141). [V]
- **Semantics: PROV-CONSTRAINTS key constraints.** Constraint 22 (key-object): "The identifier
  field id is a KEY for the entity(id,attrs) statement", and likewise for activity and agent.
  Constraint 23 (key-properties) says the same for every relation with an optional id. Enforcing
  a key constraint merges the records: "return instance {r(S(a0); S(a1),...S(an),attrs1 ∪
  attrs2)}". If the formal arguments cannot be unified, "the constraint is unsatisfiable" and the
  instance is invalid (PROV-CONSTRAINTS §6.1). Merging an activity with
  `_t1`/`_t2` placeholders shows the effect: two partial descriptions combine into one. [V]
- PROV-DM §5.7.1: "Two entities (resp. activities, agents) are equal if they have the same
  identifier." [V]
- **Consequence for the parser:** normalise to one record per (kind, expanded IRI) by unioning
  attributes, and report a conflict when formal arguments differ. Do this per bundle (§A.8),
  never across bundles.

### A.8 Bundles

- Submission §3.3 [V]:
  > "a PROV bundle's content is represented as a normal PROV-JSON document with the exception that
  > it MUST NOT contain another PROV bundle. The PROV-JSON bundle's content is then put into the
  > bundle structure of the containing PROV-JSON document and indexed by its identifier."
- **Own prefixes:** yes. The schema's `bundle` definition has its own `prefix` property, and
  PROV-N defines the scoping (§3.7.4) [V]:
  > "The scope of a prefix-namespace declaration directly occurring in a bundle is the bundle itself."
  > "The scope of a prefix-namespace declaration directly occurring in a document is the document
  > including the bundles it contains but excluding those bundles that re-declare this prefix."
  > "A set of namespace declarations namespaceDeclarations occurring in a bundle MAY re-declare a
  > prefix declared in a surrounding document."

  PROV-N Example 43 shows a bundle-level `default` overriding the document's, **including for
  the bundle's own name**. PROV-N §3.4.1: "Each identifier occurring in a bundle, including the
  bundle identifier itself, MUST be interpreted with respect to the namespace declarations of
  that bundle, or if the identifier's prefix is not declared in the bundle, with respect to the
  namespace declarations in the document." [V]
- **Library divergence on the bundle key:** Python `prov` resolves the bundle id in the bundle's
  own scope, as PROV-N requires (`provjson.py` l.523–526, l.357–359). ProvToolbox master resolves
  the bundle key against the **document** namespaces (`ProvJsonReader.java` l.69:
  `readBundle(id(e.getKey(), ns), …)` with the document `ns`). [V]
- **Nesting:** not allowed. PROV-N §3.4.1: "Bundles cannot be nested." Submission: "MUST NOT
  contain another PROV bundle". ProvToolbox throws "a bundle must not contain a bundle"
  (l.64–65). The W3C schema does **not** enforce this, because the `bundle` definition has no
  `additionalProperties: false`. [V]
- **Bundle as entity:** PROV-DM §5.4.1: "A bundle is a named set of provenance descriptions, and
  is itself an entity". §5.4.2: "A bundle description is of the form entity(id, [
  prov:type='prov:Bundle', attr1=val1, ...] )". PROV-N §3.4.2: "When described, a Bundle MUST
  be expressed as an entityExpression with attribute prov:type='prov:Bundle'." In submission
  Example 41, the `entity` records for `bob:bundle1`/`alice:bundle2` and their
  `wasGeneratedBy`/`wasAttributedTo` sit **at the top level**, next to the `bundle` map. [V]
- **Bundle independence:** PROV-CONSTRAINTS §7.2 [V]:
  > "each bundle is handled independently; there is no interaction between bundles from the
  > perspective of applying definitions, inferences, or constraints, computing normal forms, or
  > checking validity or equivalence."
  > "A PROV document is valid if each of the bundles I0, ..., In are valid and none of the bundle
  > identifiers bi are repeated."

  So the same entity id in two bundles describes two separate things from PROV's point of view.
  CPM's shared-connector-id convention is a layer on top of PROV, not a PROV merge.
- **Legacy `@id` inside a bundle:** ProvToolbox up to 2.2.4 wrote `"@id"` inside each bundle
  object. The current reader accepts it only if it equals the key (`ProvJsonVocabulary.java`
  l.86–87; `ProvJsonReader.java` l.58–63). [V]

### A.9 Known gaps: submission vs schema vs tools

Schema defects (W3C schema file as served on 2026-10-18) [V]:

1. `wasEndedby` (lowercase `b`) at the top level and in `bundle`. Combined with top-level
   `additionalProperties: false`, a document using the spec's own `wasEndedBy` key fails
   validation at the top level.
2. `usage` is `{"$ref": "#/definitions/generation"}`, so `used` requires `prov:entity`, while
   PROV-DM makes `activity` required and `entity` optional. ProvToolbox's own test schema
   (`modules-core/prov-jsonld/src/test/resources/prov-json-schema-v4.json`) fixes this with
   `"required": ["prov:activity"]`.
3. `mentionOf` and the dictionary sections from Appendix B are absent, and the top level rejects
   unknown keys.
4. In `activity`, `prov:startTime`/`prov:endTime` sit outside `properties`, so they are never
   checked.
5. Qualified names are declared `"format": "uri"`, which is wrong for `ex:foo`-style names and
   for unprefixed names.
6. `start`/`end` do not list `prov:starter`/`prov:ender` (submission §3.2.4–5 never shows them).
   The libraries use those keys (Python `constants.py` l.131–132; `ProvJsonVocabulary.java`
   l.45–46).
7. Records must be objects, so the arrays that Python `prov` writes for repeated ids are invalid.

Submission-text quirks [V]:

- `xsd` namespace `2000/10` (§A.2).
- Example 31 (Delegation) leaves out the `prov:role="loggedInUser"` attribute and two of the
  three associations shown in Example 30.
- Example 11 gives `ex:gen1` the port `"p1"` although Example 10 says `"p2"`.
- Example 37 swaps the order of `prov:alternate1`/`prov:alternate2` (§A.3).

Library behaviour, with the versions read:

- **Python `prov`**: read at master `c868665` (2026-10-18, `__version__` 3.2.2 plus "Unreleased")
  and at tag `2.0.2`. The 2024 Python Provenance Storage pins `prov==2.0.0` (`requirements.txt`
  l.16). Differences that matter here [V]:
  - 2.x writes `prov:QUALIFIED_NAME` and numeric `$`; 3.x writes `xsd:QName` and string `$`.
  - It writes arrays for repeated ids.
  - It reads `default`.
  - 3.x warns about a long list of non-conforming inputs ahead of 3.3.0 (HISTORY "Unreleased").
- **ProvToolbox**: read at master `78d679a` (2026-10-16, pom version 2.2.5). Its PROV-JSON
  reader and writer were rewritten in 2.2.5: "PROV-JSON read and written as the W3C submission
  has it (ProvJsonReader/ProvJsonWriter replace the mixins ported from JSON-LD)" (RELEASES.md
  2.2.5). **CPF-Toolbox depends on ProvToolbox 2.2.1** (CPF-Toolbox README "Maven
  dependencies"), the older mixin-based serializer. Its output, as seen in the README and the
  EMBRC examples, wraps every `prov:type` in an **array** and uses `prov:QUALIFIED_NAME`. [V]
- **Interoperability issues the CPM authors found** (Wittner & Formánek, ESWC 2024 poster,
  between "ProvToolbox version 2.0.2 and Prov Python version 2.0.0", reported as fixed) [V that
  the paper says so]:
  1. Identifiers with spaces cannot be read back from PROV-N by ProvToolbox.
  2. "ProvToolbox expects that the "prov" and "xsd" namespaces are explicitly defined in the
     PROV-JSON serialization."
  3. Microsecond timestamps lose information.
  4. "If a PROV-JSON document contains "prefix.default" node, the ProvToolbox does not consider it
     as a default namespace".
- **Variants seen in CPF practice** (CPF-Compatibility-Tests, `TEST_REPORT.md`, dated
  2026-06-20) [R]:
  - Prefix declared at the top level, inside the bundle, or both.
  - Implicit vs explicit `prov`/`xsd`.
  - `@id` inside bundles.
  - `default` vs a non-standard `defaultNamespace` key.
  - Bare vs array-wrapped values, and native vs string numbers.
  - `blank#` vs `blank/` namespace, and `_:` vs `blank:` ids.
  - Typed vs bare-string `prov:type`.
  - Timestamps with timezone, milliseconds or microseconds.
  - Records at the top level vs only inside the bundle.
  - Full-IRI vs prefixed vs unprefixed identifiers.

  CPF-Store (Java) rejects records at the top level and documents with more than one bundle
  (#43–45, #18). That matches `CpmDocument`'s "exactly one" bundle check in CPF-Toolbox (§B.8).
- **Relative namespace IRIs** occur in real CPM data: `"gen": "gen/"` in the EMBRC Dataset2
  storage file. The submission requires IRIs (schema `format: uri`). Resolve them as-is and warn. [V]

---

## B. Common Provenance Model (CPM)

### B.1 Which document is authoritative, and what is free

| Source | Date | Access | Vocabulary generation |
|---|---|---|---|
| ISO 23494-2:2026 *Biotechnology — Provenance information model for biological material and data — Part 2: Common provenance model* ([iso.org/standard/87714](https://www.iso.org/standard/87714.html)) | Published 2026-06, Edition 1, 65 pp. | **Paywalled** (CHF 204) | normative, not read |
| Wittner, *Common Provenance Framework for Multi-organizational Environments* (preprint under review) + supplementary, [Zenodo 14526108](https://zenodo.org/records/14526108) | 2024-12-19 | Free, CC-BY-4.0 | **current**: Table 1 (types), Table 2 (attributes), Figs 5–9 |
| CPF-Toolbox ([GitHub](https://github.com/Common-Provenance-Framework/CPF-Toolbox)) | commit `a21e512`, 2026-09-18 | Free, Apache-2.0 | current + extensions. README: "The reference implementation of the Common Provenance Model (CPM) from the ISO 23494 standard … it reflects the model's state as of Spring 2025 ([reference](https://zenodo.org/records/14526108))" |
| CPF-Storage ([GitHub](https://github.com/Common-Provenance-Framework/CPF-Storage)) | commit `61b521f`, 2026-10-12 | Free | current, plus `…SpecV` attributes |
| commonprovenancemodel.org | fetched 2026-10-18 | Free | **legacy**. Every page title says "(SITE UNDER CONSTRUCTION)", and the namespace page says terms are defined "within the namespace [PLACEHOLDER]" |
| Wittner et al., *Lightweight Distributed Provenance Model…*, Sci Data 9, 503 (2022), [doi:10.1038/s41597-022-01537-6](https://doi.org/10.1038/s41597-022-01537-6) | 2022-08 | Open access | **legacy** (senderConnector/receiverConnector/externalInput/receiptActivity/jump connectors) |
| EOSC-Life common provenance model, [doi:10.5281/zenodo.4705074](https://doi.org/10.5281/zenodo.4705074) | 2021 | Free | not read |
| Wittner et al., *Toward a common standard for data and specimen provenance in life sciences*, Learn Health Sys 2024, [doi:10.1002/lrh2.10365](https://doi.org/10.1002/lrh2.10365) | 2024 | Open access | not read |

All rows are [V] except the two marked "not read".

**Recommendation:** treat the CPF preprint (Tables 1–2, Figs 5–9) together with CPF-Toolbox and
CPF-Storage as the working specification. Record every rule's source, because the ISO text may
differ. The preprint: "This work builds on the W3C PROV and the Common Provenance Model (CPM),
and serves as an open conceptual foundation for the ISO 23494 provenance standard series"
(Zenodo abstract). [V]

### B.2 Namespace and prefix

- CPF-Toolbox: `CPM_PREFIX = "cpm"`, `CPM_NS = "https://www.commonprovenancemodel.org/cpm-namespace-v1-0/"`
  (`cpm-core/…/constants/CpmNamespaceConstants.java` l.4–5). CPF-Storage, the 2024 Python
  Provenance Storage (`neomodel2prov.py` l.12) and all EMBRC example files use the same IRI. [V]
- CPF preprint Table 1 caption: "The cpm prefix stands for the CPM namespace
  (https://www.commonprovenancemodel.org/, where the model will be sustained.)". That is a site
  URL, not the namespace IRI. [V]
- Older IRIs in first-party examples: `http://commonprovenancemodel.org/` and
  `http://commonprovenancemodel.org/ns/0.1` (BBMRI-ERIC ISO23494-PROV-demonstrator, Part 3 TTL
  files, 2023). [V]
- **Trap:** CPF-Toolbox recognises a CPM term only if **both** the prefix is `cpm` **and** the
  IRI matches (`CpmUtilities.belongsToCpmNs`, l.182–186). A document that binds the namespace to
  another prefix would not be recognised. Our checker should match on the expanded IRI. [V]
- `dct` = `http://purl.org/dc/terms/` is used for `dct:hasPart` (`DctNamespaceConstants.java`). [V]

### B.3 CPM types (values of `prov:type`)

Spelling is exactly as in the sources. "Paper" means CPF preprint Table 1. [V]

| Type | PROV kind | Paper (2024) | CPF-Toolbox `CpmType` | Meaning |
|---|---|---|---|---|
| `cpm:backwardConnector` | entity | yes | yes | "A connector that represents a received described object" |
| `cpm:forwardConnector` | entity | yes | yes | "A connector that represents a described object that can be or has been sent from a sender to a receiver" |
| `cpm:specForwardConnector` | entity | no (the paper describes "a PROV entity that specializes the connector") | yes | Forward connector specialisation carrying the forward link (§B.6) |
| `cpm:mainActivity` | activity | yes | yes | "represents the described activity documented in a particular provenance component" |
| `cpm:senderAgent` | agent | yes | yes | "a sender in the context of passing a described object between two provenance controllers" |
| `cpm:receiverAgent` | agent | yes | yes | "a receiver …" |
| `cpm:currentAgent` | agent | no | yes | "The provenance controller of this bundle" (CPF-Toolbox README `currentAgent`) |
| `cpm:id` | entity | no | yes (`IDENTIFIER("id")`) | identifier entities (external ids). Excluded from the traversal information by the default strategy |

CPF-Toolbox enforces the kinds through `CPM_TYPE_TO_KIND` (`CpmType.java`): connectors and `id`
are entities, `mainActivity` is an activity, and the three agent roles are agents. An element may
carry several CPM types only if all of them are agent roles (`CpmUtilities.hasValidCpmType`). [V]
The EMBRC example has one agent typed both `cpm:senderAgent` and `cpm:receiverAgent` (Dataset2). [V]

**Legacy types** that may still appear in older data:

- `cpm:currentConnector`, `cpm:receiptActivity`, `cpm:jumpForwardConnector` and
  `cpm:jumpBackwardConnector` (website "CPM Namespace v1.0").
- `senderConnector`, `receiverConnector` and `externalInput` (Sci Data 2022).
- `cpm:externalInputConnector`, `cpm:receiverConnector` and `cpm:currentConnector` (2023
  demonstrator TTL).
- `cpm:token`, `cpm:tokenGeneration` and `cpm:trustedParty` (meta-bundle NRO tokens, Python
  Provenance Storage `prov2neomodel.py` l.89, l.101, l.129).

All [V].

### B.4 CPM attributes

"Paper" means CPF preprint Table 2, plus the mandatory/optional marks from Table 1. [V]

| Attribute | Paper description | Required by (paper, Table 1) | CPF-Toolbox | CPF-Storage validator |
|---|---|---|---|---|
| `cpm:referencedBundleId` | "identifier of a PROV bundle referenced by the connector that contains this attribute" | backwardConnector: **mandatory**; forwardConnector: optional | backward + spec-forward connectors | required on backward and spec-forward connectors |
| `cpm:referencedMetaBundleId` | "identifier of a PROV bundle that contains respective provenance component's metadata in the standardized representation" | backwardConnector: **mandatory**; forwardConnector: optional; mainActivity: **mandatory** | yes | required on backward, spec-forward, mainActivity |
| `cpm:referencedBundleHashValue` | "a hash value of a PROV bundle referenced by the connector in which the attribute is present" | backwardConnector: **mandatory**; forwardConnector: optional | yes (README: "Pass a plain string") | required on backward, spec-forward |
| `cpm:hashAlg` | "algorithm used to create a hashValue" | backwardConnector: **mandatory**; forwardConnector: optional | one of `MD5`, `SHA1`, `SHA256`, `SHA512` | required; compared with the stored token's hash function |
| `cpm:provenanceServiceUri` | "URI of a service where a provenance component can be requested using its identifier" | backward/forward: optional | spec-forward only | — |
| `cpm:referencedBundleSpecV` | — (not in paper) | — | "Content version of the referenced bundle" | required on backward, spec-forward |
| `cpm:referencedMetaBundleSpecV` | — (not in paper) | — | "Content version of the referenced meta-bundle" | required on backward, spec-forward, mainActivity |
| `cpm:externalId` | "external identifier of a described object or a described activity" | — | yes | — |
| `cpm:externalIdType` | "type of the external identifier" | — | yes | — |
| `cpm:hasId` | "an indication whether the PROV entity having this attribute has an external identifier expressed as a standalone entity" | — | yes | — |
| `cpm:describedObjectType` | "type of the object represented by the PROV entity" | — | yes | — |
| `cpm:hashValue` | "a hash value of a described object … presuming that the described object is a digital object" | — | yes | — |
| `cpm:contactIdPid` | "a resolvable persistent identifier dereferencing to actual contact information … thus it has the form of a qualified name" | — | on agents; README: "The CPM specification makes `contactIdPid` **mandatory** for a current agent" (not enforced) | — |
| `cpm:comment` | — (not in paper) | — | yes | — |
| `dct:hasPart` | Table 1: optional on mainActivity | — | `mainActivity.hasPart` → sub-activities | — |

Sources: CPF-Toolbox `CpmAttribute.java` l.8–21 and README "Field reference"; CPF-Storage
`CPMAttributesValidator.java` l.28–59. [V]

Value forms in practice [V]:

- `cpm:referencedBundleId`, `cpm:referencedMetaBundleId` and `dct:hasPart` are written as
  **qualified-name typed literals**, e.g. `{"type": "prov:QUALIFIED_NAME", "$": "storage:SamplingBundle_V0"}`
  (EMBRC Dataset2). The checker must resolve them against the bundle's prefixes.
- `cpm:hashAlg` is a plain string (`"SHA256"`).
- `cpm:referencedBundleHashValue` is a plain hex string in the EMBRC data. A CPF-Toolbox test
  fixture instead has `"{hash=abc123def456}" %% xsd:byte` (`deserialization/expectedOutput.provn`),
  an artefact the README warns about ("a JSON object is not converted correctly").
- `cpm:contactIdPid` is a plain string in the CPF-Toolbox fixtures, although the paper says it is
  a qualified name.

Spelling trap: the preprint figures 6–7 write `cpm:referencedBundleHasValue` (no `h`), while
Table 2 and all code use `cpm:referencedBundleHashValue`. [V]

**Legacy attributes** [V]:

- `cpm:senderBundleId`, `cpm:receiverBundleId`, `cpm:currentBundle` and `cpm:metabundle`
  (website namespace page).
- `cpm:senderBundleId`, `cpm:receiverBundleId` and `cpm:metabundle` as used by the 2024 Python
  Provenance Storage (`controller.py` l.219–230, l.331–345). Its validator requires the main
  activity to carry `cpm:metabundle` (`validators.py` l.239–247).
- `cpm:externalID` (capital `ID`, 2023 demonstrator TTL).

### B.5 What a CPM bundle contains: traversal information vs domain-specific part

- CPF preprint §"The Common Provenance Model" (lines 336–340) [V]:
  > "each provenance component is technically realized as a PROV bundle which is identified by a
  > persistent identifier, which is unique and resolvable to the component's content. Each
  > component consists of two types of information: 1) traversal information that is used for
  > navigation and traversal of a provenance chain; and 2) domain or scenario-specific information
  > documenting described activities."
- Traversal information, from the preprint (l.342–343 and Fig. 5 caption) [V]:
  > "Each provenance component contains exactly one main activity that should generate at least
  > one forward connector."
  > "No other provenance structures (except redundant connectors defined below) are allowed in the
  > traversal information."
- Supplementary, "Minimal content of an existing provenance component" [V]:
  > "Each provenance component in a provenance chain must contain at least two types of provenance
  > structures: the main activity and the forward connector"
  > "A provenance component without a backward connector is considered as the beginning of a
  > provenance chain, and each provenance chain must contain at least one such component."

  The preprint adds (l.355–357): "A provenance component without a forward connector represents an
  end of a provenance chain (e.g., for destructive activities…)". This contradicts the "must
  contain … the forward connector" above; see Open questions.
- **Required relations in the traversal information** (preprint Fig. 5, Figs 6–8; CPF-Toolbox
  README "What This Toolbox Does" and "Basic example") [V]:

  | Relation | From → to | Source |
  |---|---|---|
  | `used` | mainActivity → backwardConnector | Fig. 5; Toolbox `mainActivity.used` |
  | `wasGeneratedBy` | forwardConnector → mainActivity | Fig. 5; Toolbox `mainActivity.generated` |
  | `wasDerivedFrom` | forwardConnector → backwardConnector ("if not present, they are assumed implicitly", supplementary simplification 5) | Fig. 5; Toolbox `forwardConnectors[].derivedFrom` |
  | `wasDerivedFrom` | backwardConnector → backwardConnector, forwardConnector → forwardConnector (redundant connectors) | Fig. 8 |
  | `wasAttributedTo` | backwardConnector → senderAgent | Fig. 5 |
  | `specializationOf` | spec-forward connector → forwardConnector | Fig. 5, Fig. 7; Toolbox `specForwardConnectors[].specializationOf` |
  | `wasAttributedTo` | spec-forward connector → receiverAgent | Fig. 5 |
  | `wasAssociatedWith` | mainActivity → currentAgent | Toolbox README: the spec "requires the main activity to be associated with it", not enforced |

  The legacy website "CPM Backbone Template v1.0" states the opposite attribution: "backwardConnector entity is attributed
  (prov:wasAttributedTo) to receiverAgent. forwardConnector entity is attributed
  (prov:wasAttributedTo) to senderAgent." [V] See Open questions.
- **How domain content is attached** [V]:
  - Preprint l.361–362: "Domain-specific information is attached to the traversal information
    using the Compound Activities Pattern".
  - Website "CPM Backbone Template v1.0": "the main and the receipt activities contain an
    attribute (dct:hasPart) listing sub-activities … Inputs and outputs of the activities and
    sub-activities are related using the PROV specialisation relation."
  - CPF-Toolbox README: "Add `specializationOf` relations to connect relevant domain entities to
    the corresponding connectors."
  - Direction in the data is `specializationOf(domainEntity, connector)`: the domain entity is
    `prov:specificEntity` and the connector is `prov:generalEntity`. EMBRC example:
    `"prov:specificEntity": "blank:BigProject_belgium_water_10m_1000um_r1", "prov:generalEntity": "blank:StoredSampleCon_r1"`.
- **How the two parts are distinguished in the data:** there is **no explicit marker**. The
  split is computed from `cpm:` types. CPF-Toolbox has three strategies (`cpm-core/…/strategy/`)
  [V]:
  - `CpmTypeHasTIStrategy` (**default**, `CpmDocument.java` l.45): a node is in the traversal
    information iff it has exactly one CPM type from the known set (excluding `cpm:id`), or
    several CPM types that are all agent roles. Other, non-CPM types are ignored.
  - `CpmTypeTIStrategy`: the node's types must all be valid CPM types (`hasValidCpmType`); any
    non-CPM `prov:type` disqualifies it.
  - `AttributeTIStrategy`: no non-CPM attributes (no `prov:label`, no `prov:location`, only
    `cpm:` attributes and `dct:hasPart`). A node without a CPM type also counts if it is the
    specialization of exactly one forward or spec-forward connector.

  `getCrossPartEdges()` then returns the relations that join the two parts
  (`CpmDocument.java` l.660–677). [V]

### B.6 Connectors and following the chain

- **Shared identifier** (preprint Fig. 6 caption) [V]:
  > "The connection is realized using the connector – a PROV entity with a shared identifier. The
  > attributes associated with the backward connector provide the link to sender's provenance
  > component in finalized provenance information and respective meta component."
- **Backward link** (required for chaining). In the receiver's bundle, the backward connector has
  the **same id** as the sender's forward connector. It carries `cpm:referencedBundleId` = the
  sender's bundle id, `cpm:referencedMetaBundleId`, `cpm:referencedBundleHashValue` and
  `cpm:hashAlg` (Fig. 6; Table 1). The supplementary describes this as "including the identifier
  of the forward connector in the newly connected component, which essentially creates a
  backward link between the two components". [V]
- **Forward link** (optional, needs a new version of the sender's bundle) [V]:
  - Preprint l.353–355: "The forward traceability is realized by adding a PROV entity that
    specializes the connector and has all the values for the forward connector attributes
    provided."
  - Fig. 7: the specializing entity (`connector_id2`) carries `cpm:referencedBundleId` =
    `receiver:bundleId`.
  - In CPF-Toolbox and CPF-Storage this entity has type `cpm:specForwardConnector` and is
    `specializationOf` the plain forward connector. The README says to "use one when the
    receiving bundle already exists and you want the chain verifiable in both directions".
  - The plain `forwardConnector` in CPF-Toolbox accepts no `referencedBundleId` ("Forward
    connectors accept **nothing else**"). The preprint's Table 1, however, lists
    `cpm:referencedBundleId` etc. as optional attributes of the forward connector itself.
- **Redundant connectors** for missing intermediate components (preprint l.368–377, Fig. 8):
  extra backward or forward connectors chained by `wasDerivedFrom`, each "present in an existing
  provenance component, where it will be used or generated by the corresponding main activity".
  These replace the 2022 "jump connectors". [V]
- **Traversal algorithm** (Supplementary Appendix IV) [V]:
  1. Input: a forward connector id and a component (bundle) id.
  2. Resolve the bundle and check the connector's type.
  3. Report an error if the type is wrong.
  4. Find "all the backward connectors from which the input forward connector is derived" via
     `wasDerivedFrom`, possibly through redundant connectors. For each, recurse with "the
     backward connector id and the referenced bundle id present in the backward connector's
     attributes".
  5. Stop when no new backward connectors are found.

  "a cycle is never present in a provenance chain that is in finalized provenance information".
  The forward direction is analogous. Before loading a linked component, the algorithm may use
  the referenced meta-bundle to find the latest version, because "a component version linked by
  a connector is not necessarily the latest one".
- **Bundle-to-service resolution**: PT-Service gets a bundle's access service URI from
  `cpm:provenanceServiceUri` on the referencing connector or from a configured prefix table
  (PT-Service README "Provenance service table"). [V]
- **Worked chain in first-party data** (CPF-Toolbox `cpm-template/src/test/resources/embrc/*/…_storage_v*.json`):
  - `storage:SamplingBundle_V0` has `cpm:forwardConnector` `blank:StoredSampleCon_r1`.
  - `storage:ProcessingBundle_V0` and `_V1` have `cpm:backwardConnector` `blank:StoredSampleCon_r1`
    with `cpm:referencedBundleId` `storage:SamplingBundle_V0`.
  - `storage:SamplingBundle_V1` adds `blank:StoredSampleCon_r1_Spec` (`cpm:specForwardConnector`)
    with `cpm:referencedBundleId` `storage:ProcessingBundle_V0`.
  - `SpeciesIdentificationBundle_V0` holds a redundant backward connector straight to
    `SamplingBundle_V0`.

  Connectors point at **specific versions** (`_V0`) even after a `_V1` exists. [V]

### B.7 Meta-bundles (meta components)

- Purpose, from the preprint (l.378–393) [V]:
  - "The CPF defines a versioning mechanism for provenance components in finalised provenance
    information."
  - "The component is represented by a general entity expressing general attributes, and a
    specialised entity expressing attributes specific to a particular version of that provenance
    component. In the case of multiple versions of the same component, they are all expressed in
    the meta component as a specialisation of the general entity, and related using the PROV
    revision relation."
  - "The actual link from meta component to respective provenance component is realised by the
    identifier [of] a PROV entity that represents the specific version of the provenance
    component."
- Fig. 9 labels the version entities `prov:type prov:bundle`, lowercase. The 2024 Python
  Provenance Storage also writes `'prov:type': 'prov:bundle'`, plus `pav:version`, and links
  versions with `wasDerivedFrom` typed `prov:Revision` (`prov2neomodel.py` l.41–75). PROV-DM's
  type is `prov:Bundle`. [V]
- **How bundles point to meta-bundles** [V]:
  - The main activity has `cpm:referencedMetaBundleId`, mandatory in Table 1.
  - Backward connectors carry the sender's `cpm:referencedMetaBundleId`.
  - Spec-forward connectors carry the receiver's.
  - Legacy data uses `cpm:metabundle` on the main activity.
- Requirements 13–15 (preprint l.308–314) [V]:
  - "Each provenance component in finalized provenance information shall have associated a meta
    component".
  - Metadata of one component "should not be fragmented across multiple meta components".
  - A meta component "shall be assigned a globally unique persistent identifier".
- Extensions: meta components may also hold NRO tokens (supplementary Appendix III). CPF-Storage
  compares `cpm:hashAlg`/`cpm:referencedBundleHashValue` on connectors against the stored token
  (`CPMChainValidator.java`). [V]
- In the EMBRC data, meta-bundle ids look like `meta:SamplingBundle_V0_meta` with
  `meta = http://prov-storage-hospital:8000/api/v1/documents/meta/`. The `_V1` main activity
  still references `meta:ProcessingBundle_V0_meta`. [V]

### B.8 Validation rules and validators

Normative-sounding rules in the free sources [V]:

- Preprint requirements (l.263–314, "Keywords MUST, SHALL, SHOULD, and MAY in this section
  should be interpreted as defined in RFC2119"). The rules relevant to a checker:
  - 4: linking to origins "shall always be linked".
  - 5: linking to derivatives "should".
  - 10: each component "shall be assigned a globally unique persistent identifier".
  - 12: "Provenance component shall be immutable. Update shall be performed outside the
    provenance component by creating a new provenance component".
  - 13–15: meta components (§B.7).
- Table 1 mandatory attributes (§B.4). "Exactly one main activity" and "No other provenance
  structures … are allowed in the traversal information" (§B.5).

Executable checks found [V]:

| Implementation | What it checks |
|---|---|
| CPF-Toolbox `CpmDocument` | The input `Document` must hold exactly one statement, a `Bundle` ("Cpm Document must consist of exactly one statement of type PROV_BUNDLE", `CpmExceptionConstants.java` l.6; `CpmDocument.java` l.61–64). Dictionary relations throw `UnsupportedOperationException`. `getMainActivity()` returns the **first** main activity and does not reject several. CPM type ↔ PROV kind consistency. |
| CPF-Storage (Java) `CPMAttributesValidator` | The bundle id's namespace must equal `<fqdn>organizations/<orgId>/documents/`. Backward and spec-forward connectors need `referencedBundleId`, `referencedMetaBundleId`, `referencedBundleSpecV`, `referencedMetaBundleSpecV`, `referencedBundleHashValue` and `hashAlg`. A forward connector needs only its type; any other attribute is allowed since PR #28 (CPF-Storage issue #25). The main activity needs `referencedMetaBundleId` and `referencedMetaBundleSpecV`. |
| CPF-Storage `CPMChainValidator` | For backward and spec-forward connectors: `referencedBundleId` and `referencedMetaBundleId` must be resolvable, and `hashAlg`/`referencedBundleHashValue` must match the referenced bundle's token. |
| Python Provenance Storage (2024, legacy vocabulary) `validators.py` | Exactly one bundle, at most one `mainActivity` ("Multiple 'mainActivity' activities specified"), `cpm:metabundle` on the main activity, connector **identifiers** resolvable as PIDs. Normalisation is a TODO (`_is_graph_normalized` returns `True`). |

**SHACL / published validator:** none found. I searched the web and GitHub for CPM- or ISO
23494-specific SHACL shapes, the CPF organisation repos and the BBMRI-ERIC demonstrator. The
`Part 2 - Common Provenance Model` folder of the demonstrator holds only a README. Project
notes say a validator (PROV-DM + CPM, SHACL) is being written by Matúš Formánek, so there was no
public artefact to cite at this date. [V for "not found"; U for absence in general]

### B.9 Official example documents

All [V]:

- **CPF-Toolbox README "Basic example"**: minimal traversal information as template JSON with
  its PROV-JSON output (one bundle, `mainActivity`, one backward and one forward connector).
- **CPF-Toolbox `cpm-template/src/test/resources/`**:
  - `embrc/dataset{1..4}/*_cpm_storage_v{0,1}.json`: a 4-bundle chain in PROV-JSON with real
    connectors, versions and meta references.
  - `embrc/*/…_cpm.jsonld`: PROV-JSONLD.
  - `mou/{acquisition,storage}/*.provn`: MMCI biobank chain in PROV-N.
  - `deserialization/expectedOutput.provn`: every CPM type and attribute in one bundle.
- **CPF-Toolbox `docs/datasets.md`** describes the datasets. It notes the MMCI files "are not
  directly compatible with Provenance Storage" and that files must not be altered, or "this will
  affect their content hash and prevent successful upload".
- **CPF-Compatibility-Tests `base_document.json`**: "CPM document adapted from CPF-Toolbox".
- **CPF preprint supplementary, Appendices I–II**: chain examples as figures only.

**Excerpt** (abridged from `embrc/dataset2/Dataset2_cpm_storage_v1.json` at CPF-Toolbox
`a21e512`). Only the traversal-information records, plus one domain `specializationOf`, are kept,
and the `prefix` map is reduced. Values are verbatim:

```json
{
  "prefix": {
    "cpm": "https://www.commonprovenancemodel.org/cpm-namespace-v1-0/",
    "storage": "http://prov-storage-hospital:8000/api/v1/organizations/I2LAH5SF/documents/",
    "meta": "http://prov-storage-hospital:8000/api/v1/documents/meta/",
    "blank": "https://openprovenance.org/blank#",
    "dct": "http://purl.org/dc/terms/",
    "prov": "http://www.w3.org/ns/prov#",
    "xsd": "http://www.w3.org/2001/XMLSchema#"
  },
  "bundle": {
    "storage:ProcessingBundle_V1": {
      "entity": {
        "blank:StoredSampleCon_r1": {
          "cpm:hashAlg": "SHA256",
          "prov:type": { "type": "prov:QUALIFIED_NAME", "$": "cpm:backwardConnector" },
          "cpm:referencedMetaBundleId": { "type": "prov:QUALIFIED_NAME", "$": "meta:SamplingBundle_V0_meta" },
          "cpm:referencedBundleHashValue": "dc76720f4a8a8dba16782a01aa083afcd1bf1f6907fc93983f471b5455283888",
          "cpm:referencedBundleId": { "type": "prov:QUALIFIED_NAME", "$": "storage:SamplingBundle_V0" }
        },
        "blank:ProcessedSampleCon": {
          "prov:type": { "type": "prov:QUALIFIED_NAME", "$": "cpm:forwardConnector" }
        },
        "blank:ProcessedSampleConSpec": {
          "cpm:hashAlg": "SHA256",
          "prov:type": { "type": "prov:QUALIFIED_NAME", "$": "cpm:specForwardConnector" },
          "cpm:referencedMetaBundleId": { "type": "prov:QUALIFIED_NAME", "$": "meta:SpeciesIdentificationBundle_V0_meta" },
          "cpm:referencedBundleHashValue": "539b9299c472184ce765f2906352a55554c14498f61721479930d5cb015be5bd",
          "cpm:referencedBundleId": { "type": "prov:QUALIFIED_NAME", "$": "storage:SpeciesIdentificationBundle_V0" }
        }
      },
      "activity": {
        "blank:Processing": {
          "dct:hasPart": { "type": "prov:QUALIFIED_NAME", "$": "blank:MaterialProcessing" },
          "prov:type": { "type": "prov:QUALIFIED_NAME", "$": "cpm:mainActivity" },
          "cpm:referencedMetaBundleId": { "type": "prov:QUALIFIED_NAME", "$": "meta:ProcessingBundle_V0_meta" }
        }
      },
      "agent": {
        "blank:NiceMarineStation": {
          "prov:type": [
            { "type": "prov:QUALIFIED_NAME", "$": "cpm:senderAgent" },
            { "type": "prov:QUALIFIED_NAME", "$": "cpm:receiverAgent" }
          ]
        }
      },
      "used":           { "_:n105": { "prov:activity": "blank:Processing", "prov:entity": "blank:StoredSampleCon_r1" } },
      "wasGeneratedBy": { "_:n106": { "prov:entity": "blank:ProcessedSampleCon", "prov:activity": "blank:Processing" } },
      "wasDerivedFrom": { "_:n108": { "prov:generatedEntity": "blank:ProcessedSampleCon", "prov:usedEntity": "blank:StoredSampleCon_r1" } },
      "specializationOf": {
        "_:n110": { "prov:specificEntity": "blank:ProcessedSampleConSpec", "prov:generalEntity": "blank:ProcessedSampleCon" },
        "_:n116": { "prov:specificEntity": "blank:BigProject_belgium_water_10m_1000um_r1", "prov:generalEntity": "blank:StoredSampleCon_r1" }
      },
      "wasAttributedTo": {
        "_:n107": { "prov:entity": "blank:StoredSampleCon_r1", "prov:agent": "blank:NiceMarineStation" },
        "_:n109": { "prov:entity": "blank:ProcessedSampleConSpec", "prov:agent": "blank:NiceMarineStation" }
      }
    }
  }
}
```

Things this real example shows, all of which a checker must handle:

- `prov:type` appears both as a single object and as an array.
- References are QName-typed literals.
- One agent plays both roles.
- The `blank:` prefix is a real namespace.
- The main activity has **no** `wasAssociatedWith` to a current agent.
- There are no `…SpecV` attributes, so this file would fail the current CPF-Storage validator.
  The example data predates that validator.

---

## Open questions / ambiguities

Contradictions between sources [V that each source says what is quoted]:

1. **Normative CPM text is paywalled.** ISO 23494-2:2026 may differ from the 2024 preprint and
   from CPF-Toolbox. Every CPM rule below is "per preprint/implementation", unverified against
   ISO.
2. **Namespace IRI:** `https://www.commonprovenancemodel.org/cpm-namespace-v1-0/` (all code),
   `https://www.commonprovenancemodel.org/` (preprint caption), `[PLACEHOLDER]` (website), or the
   older `http://commonprovenancemodel.org/…` IRIs. Should the checker accept several IRIs as CPM?
3. **Forward-link representation:** the preprint lists `cpm:referencedBundleId` etc. as optional
   on `cpm:forwardConnector` itself and describes "a PROV entity that specializes the connector"
   without a type. The implementations use a separate `cpm:specForwardConnector` and forbid those
   attributes on the plain forward connector. A checker probably needs to accept both.
4. **Agent attribution direction:** the preprint Fig. 5 has the backward connector attributed to
   the **sender** agent and the forward-connector specialisation to the **receiver** agent. The
   website template says "backwardConnector … attributed … to receiverAgent; forwardConnector …
   to senderAgent". The EMBRC data attributes both to one agent typed as both.
5. **Must a component have a forward connector?** The supplementary says yes ("must contain …
   the main activity and the forward connector"). The preprint body says "A provenance component
   without a forward connector represents an end of a provenance chain".
6. **Mandatory attributes differ per source:**
   - Preprint: backward connector needs `referencedBundleId`, `referencedMetaBundleId`,
     `referencedBundleHashValue` and `hashAlg`; the main activity needs `referencedMetaBundleId`.
   - CPF-Storage: adds both `…SpecV` attributes.
   - CPF-Toolbox: makes everything optional.
   - Example data: lacks `…SpecV`.
7. **`cpm:currentAgent` and `cpm:id`** exist only in the implementations. Is `currentAgent` part
   of the traversal information? CPF-Toolbox says yes, since it is in `CpmType.AGENTS`.
8. **Exactly one main activity:** the preprint says "exactly one". CPF-Toolbox silently takes the
   first. The Python storage rejects more than one.
9. **Bare-string `prov:type`** (`"cpm:mainActivity"` as `xsd:string`): accept as a CPM type,
   warn, or reject? Per PROV-JSON it is not a qualified name.
10. **Meta-bundle `prov:bundle` (lowercase)** in Fig. 9 and in the Python storage, vs PROV-DM
    `prov:Bundle`.
11. **Traversal vs domain split** is heuristic, with three strategies in CPF-Toolbox. Which one
    should our viewer adopt, or should it show "unclassified"?
12. **`cpm:contactIdPid` value form:** a qualified name per the paper, a plain string in the
    fixtures.

PROV-JSON-level questions:

13. **Arrays of records for repeated ids** (Python `prov`) vs "one object per key" (submission,
    schema, ProvToolbox reader). Plan: accept both, then merge per PROV-CONSTRAINTS.
14. **Bundle-id resolution scope:** PROV-N and Python `prov` resolve in the bundle's scope;
    ProvToolbox resolves in the document's. Plan: follow PROV-N.
15. **Statements at the top level next to `bundle`** are legal PROV-JSON (submission Example 41
    puts the bundle's own `entity` description there), but CPF-Store rejects them. The viewer
    must decide whether to show top-level content.
16. **Relation ids on `specializationOf`/`alternateOf`/`hadMember`** carry no meaning in PROV-DM
    but may come with attributes in the data. Keep or drop them?
17. **`xsd` namespace** `2000/10` vs `2001`: treat as aliases.

Could not verify:

- The text of ISO 23494-2:2026 (paywalled).
- The EOSC-Life deliverable (Zenodo 4705074) and the 2024 Learning Health Systems article, which
  were not read.
- The exact ProvToolbox `xsd` namespace constant. The 2.2.1 PROV-JSON behaviour was inferred from
  CPF-Toolbox output and release notes, not from 2.2.1 source.
- The CPF-Compatibility-Tests results (read the report, did not run them).
- Whether a public CPM SHACL or validator exists outside GitHub and the web search results.

---

## Sources

W3C (all [V]):

- PROV-JSON, W3C Member Submission, 24 April 2013:
  https://www.w3.org/submissions/2013/SUBM-prov-json-20130424/ (latest:
  https://www.w3.org/submissions/prov-json/)
- PROV-JSON JSON Schema (draft-04), schema `id` `http://provenance.ecs.soton.ac.uk/prov-json/schema#`:
  https://www.w3.org/submissions/prov-json/schema (fetched 2026-10-18)
- PROV-DM: The PROV Data Model, W3C Recommendation, 30 April 2013:
  https://www.w3.org/TR/2013/REC-prov-dm-20130430/
- PROV-N: The Provenance Notation, W3C Recommendation, 30 April 2013:
  https://www.w3.org/TR/2013/REC-prov-n-20130430/
- Constraints of the PROV Data Model (PROV-CONSTRAINTS), W3C Recommendation, 30 April 2013:
  https://www.w3.org/TR/2013/REC-prov-constraints-20130430/
- Linking Across Provenance Bundles (PROV-LINKS), W3C Working Group Note, 30 April 2013:
  https://www.w3.org/TR/2013/NOTE-prov-links-20130430/
- PROV errata: https://www.w3.org/2011/prov/errata.html

PROV libraries (first-party source code, all [V]):

- Python `prov` (Trung Dong Huynh), https://github.com/trungdong/prov:
  - master commit `c8686657061aeed99d4280c5bc2b3f373e4409a2` (2026-10-18), `__version__` 3.2.2:
    `src/prov/serializers/provjson.py`, `src/prov/constants.py`, `src/prov/model/records.py`,
    `HISTORY.md`.
  - Tag `2.0.2`: https://raw.githubusercontent.com/trungdong/prov/2.0.2/src/prov/serializers/provjson.py,
    `src/prov/constants.py` and `src/prov/model.py`.
  - PyPI: https://pypi.org/project/prov/ (latest 3.2.2, 2026-09-18).
- ProvToolbox (Luc Moreau), https://github.com/lucmoreau/ProvToolbox, master commit
  `78d679aeecba6f2a7fd7f37b15318d8c0849d80f` (2026-10-16), pom version 2.2.5:
  - `modules-core/prov-jsonld/src/main/java/org/openprovenance/prov/core/json/serialization/ProvJsonVocabulary.java`,
    `ProvJsonReader.java` and `ProvJsonWriter.java`.
  - `modules-core/prov-jsonld/src/test/resources/prov-json-schema-v4.json`.
  - `RELEASES.md`.

CPM papers and specification (all [V] except where marked not read):

- R. Wittner, *Common Provenance Framework for Multi-organizational Environments* (preprint under
  review) + Supplementary Document, Zenodo record 14526108, 2024-12-19, CC-BY-4.0:
  https://zenodo.org/records/14526108 (`CPF_paper.pdf`, `CPF_supplementary.pdf`)
- R. Wittner et al., *Lightweight Distributed Provenance Model for Complex Real–world
  Environments*, Scientific Data 9, 503 (2022): https://doi.org/10.1038/s41597-022-01537-6
- R. Wittner, M. Formánek, *Compatibility Challenges of the Current State-of-the-Art Provenance
  Tools*, ESWC 2024:
  https://2024.eswc-conferences.org/wp-content/uploads/2024/05/77770329.pdf
- ISO 23494-2:2026 *Biotechnology — Provenance information model for biological material and
  data — Part 2: Common provenance model*, Edition 1, 2026-06, 65 pp. (catalogue page only,
  paywalled): https://www.iso.org/standard/87714.html
- Common Provenance Model website (marked "SITE UNDER CONSTRUCTION"), fetched 2026-10-18:
  - https://www.commonprovenancemodel.org/
  - https://www.commonprovenancemodel.org/cpm-namespace-v1-0/
  - https://www.commonprovenancemodel.org/cpm-backbone-template-v1/
  - https://www.commonprovenancemodel.org/provenance-backbone/
  - https://www.commonprovenancemodel.org/publications/
- Common Provenance Model RO-Crate profile 0.2 (BY-COVID): https://w3id.org/cpm/ro-crate/0.2
  (checked for links only)
- EOSC-Life common provenance model, Zenodo, 2021: https://doi.org/10.5281/zenodo.4705074 (not read)
- R. Wittner et al., *Toward a common standard for data and specimen provenance in life
  sciences*, Learning Health Systems 2024: https://doi.org/10.1002/lrh2.10365 (not read)

CPM implementations and data (first-party code, all [V]):

- CPF-Toolbox, https://github.com/Common-Provenance-Framework/CPF-Toolbox, commit
  `a21e51299f6b7fc5355076f837eae83dce01b742` (2026-09-18), artefact version 2.5.0:
  - `README.md` and `docs/datasets.md`.
  - `cpm-core/src/main/java/cz/muni/fi/cpm/constants/{CpmType,CpmAttribute,CpmNamespaceConstants,DctNamespaceConstants,CpmExceptionConstants}.java`.
  - `cpm-core/…/model/{CpmDocument,CpmUtilities}.java` and `cpm-core/…/strategy/*.java`.
  - `cpm-template/src/test/resources/{embrc,mou,deserialization}/…`.
- CPF-Storage, https://github.com/Common-Provenance-Framework/CPF-Storage, commit
  `61b521f7fea478dfbaa24fe167fc7347468dbc82` (2026-10-12):
  - `README.md`.
  - `src/main/java/org/commonprovenance/framework/store/common/validation/{CPMAttributesValidator,CPMChainValidator}.java`.
  - Issue #25 and PR #28 (seen via search results).
- PT-Service, https://github.com/Common-Provenance-Framework/PT-Service, commit `7b74889`
  (2026-10-09): `README.md`
- CPF-Compatibility-Tests, https://github.com/Common-Provenance-Framework/CPF-Compatibility-Tests,
  commit `9501410` (2026-06-24): `README.md`, `TEST_REPORT.md` (report dated 2026-06-20) [R]
- provtoolbox-prov-json-compat,
  https://github.com/Common-Provenance-Framework/provtoolbox-prov-json-compat: `README.md`
- Python Provenance Storage (legacy vocabulary),
  https://github.com/GajdosDavid/distributed-provenance-system, commit `3752b84` (2024-06-11):
  - `distributed_prov_system/provenance/{validators,controller,prov2neomodel,neomodel2prov}.py`.
  - `requirements.txt`.
- BBMRI-ERIC ISO23494-PROV-demonstrator, https://github.com/BBMRI-ERIC/ISO23494-PROV-demonstrator,
  commit `64f8890` (2023-07-26): `README.md`, `Part 2 - Common Provenance Model/README.md` and
  `Part 3 …/examples-2023/*.ttl`.

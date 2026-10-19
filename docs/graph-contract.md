# Graph contract

The JSON the backend returns for a PROV document and the frontend draws. It is the single source
of truth between the two sides: change this document first, then both sides.

The contract describes meaning only (kinds, relations, attributes, parse notes). It says nothing
about styling.

## Endpoint

`POST /api/graph` with the raw PROV-JSON document as the request body (`application/json`).
The API is stateless: nothing is stored.

| Response | When |
| --- | --- |
| `200` with the graph | The body is a PROV-JSON document. |
| `400` with a ProblemDetails body | The body is not JSON, or not shaped like a PROV-JSON document. |

A body is not shaped like a PROV-JSON document when:

- its root is not a JSON object;
- its root has keys but none of them is a PROV-JSON section (`prefix`, `bundle`, `entity`,
  `activity`, `agent` or one of the relation sections below);
- `prefix`, `bundle`, a bundle's content, or a known section is not a JSON object;
- a prefix maps to something other than a string.

Everything else is read leniently. An empty object `{}` is an empty document.

## Shape

```json
{
  "prefixes": { "ex": "http://example.org/" },
  "bundles": [
    {
      "key": "b1",
      "id": "ex:bundle1",
      "iri": "http://example.org/bundle1",
      "prefixes": { "ex2": "http://example.org/2/" }
    }
  ],
  "elements": [
    {
      "key": "e1",
      "id": "ex:sample",
      "iri": "http://example.org/sample",
      "kind": "entity",
      "bundle": "b1",
      "attributes": [
        {
          "name": "prov:label",
          "iri": "http://www.w3.org/ns/prov#label",
          "values": [{ "value": "Water sample" }]
        }
      ]
    }
  ],
  "relations": [
    {
      "key": "r1",
      "type": "wasGeneratedBy",
      "id": null,
      "iri": null,
      "bundle": "b1",
      "source": "e1",
      "target": "e2",
      "attributes": []
    }
  ],
  "parseNotes": [],
  "validationFindings": []
}
```

### Keys

Every bundle, element and relation has a `key`, unique in the graph and the same every time the
same document is read. Bundles are keyed `b1`, `b2`, …, elements `e1`, `e2`, … and relations `r1`,
`r2`, …, in reading order: the top-level statements first, then each bundle in turn, and within
each the entities, activities and agents, then the relations section by section. Everything in the
contract refers to a bundle, element or relation by its key, never by its identifier: the same
identifier can appear in several bundles (a connector does exactly that), and a relation can have
no identifier at all.

### Identifiers and IRIs

`id` is the qualified name as written in the document, e.g. `blank:Sampling`. `iri` is its expanded
IRI: the namespace of the prefix followed by the local part, split at the first `:`. A name without
a `:` uses the `default` prefix.

Prefixes are looked up in this order: the bundle's own `prefix` (for a bundle's identifier and
everything inside it), then the document's `prefix`, then the implicit `prov`
(`http://www.w3.org/ns/prov#`) and `xsd` (`http://www.w3.org/2001/XMLSchema#`). If the prefix is
declared nowhere, `iri` is the identifier as written.

### `prefixes`

The document's own prefix declarations, prefix → namespace IRI, as written.

### `bundles`

One entry per bundle in the document. Bundles never nest.

| Field | Type | Meaning |
| --- | --- | --- |
| `key` | string | Key of the bundle. |
| `id` | string | Identifier as written. |
| `iri` | string | Expanded IRI, resolved with the bundle's own prefixes first. |
| `prefixes` | object | The bundle's own prefix declarations, as written. Empty if it has none. |

### `elements`

One entry per entity, activity and agent, from every bundle and from the top level of the document.

| Field | Type | Meaning |
| --- | --- | --- |
| `key` | string | Key of the element. |
| `id` | string | Identifier as written. |
| `iri` | string | Expanded IRI. |
| `kind` | `"entity"` \| `"activity"` \| `"agent"` | PROV element kind. |
| `bundle` | string \| null | Key of the bundle holding the element, or `null` for a top-level element. |
| `attributes` | array | The element's attributes, see [Attributes](#attributes). |

### `relations`

One entry per relation record, from every bundle and from the top level of the document.

| Field | Type | Meaning |
| --- | --- | --- |
| `key` | string | Key of the relation. |
| `type` | string | The PROV-JSON section name, see the table below. |
| `id` | string \| null | Identifier as written, or `null` when the relation has none. Blank identifiers (`_:…`) carry no meaning in PROV-JSON and are `null`. |
| `iri` | string \| null | Expanded IRI of `id`. |
| `bundle` | string \| null | Key of the bundle holding the relation, or `null` at the top level. |
| `source` | string \| null | Key of the element named by the relation's first argument. |
| `target` | string \| null | Key of the element named by the relation's second argument. |
| `attributes` | array | Every key of the record other than `source` and `target`, see [Attributes](#attributes). An argument that is not a string, such as an array of members in `hadMember`, stays among the attributes. |

`source` and `target` are PROV-DM's two main arguments, so an edge from `source` to `target` points
the way PROV defines it: towards the past. They name an element in the same bundle (or at the top
level, for a top-level relation). Each is `null` when the argument is missing, is not a string, or
names no element there.

| `type` | `source` | `target` |
| --- | --- | --- |
| `wasGeneratedBy` | `prov:entity` | `prov:activity` |
| `used` | `prov:activity` | `prov:entity` |
| `wasInformedBy` | `prov:informed` | `prov:informant` |
| `wasStartedBy` | `prov:activity` | `prov:trigger` |
| `wasEndedBy` | `prov:activity` | `prov:trigger` |
| `wasInvalidatedBy` | `prov:entity` | `prov:activity` |
| `wasDerivedFrom` | `prov:generatedEntity` | `prov:usedEntity` |
| `wasAttributedTo` | `prov:entity` | `prov:agent` |
| `wasAssociatedWith` | `prov:activity` | `prov:agent` |
| `actedOnBehalfOf` | `prov:delegate` | `prov:responsible` |
| `wasInfluencedBy` | `prov:influencee` | `prov:influencer` |
| `specializationOf` | `prov:specificEntity` | `prov:generalEntity` |
| `alternateOf` | `prov:alternate1` | `prov:alternate2` |
| `hadMember` | `prov:collection` | `prov:entity` |
| `mentionOf` | `prov:specificEntity` | `prov:generalEntity` |

Other arguments, such as a derivation's `prov:activity` or an association's `prov:plan`, are listed
among the attributes for now.

### Attributes

Attributes keep the document's order. An attribute whose value is an array has one entry in
`values` per array item; an attribute with a bare value has a single entry.

| Field | Type | Meaning |
| --- | --- | --- |
| `name` | string | Attribute name as written, e.g. `prov:type`. |
| `iri` | string | Expanded IRI of the name. |
| `values` | array of `{ "value": string }` | The values, in order. |

`value` is the value's text: a string as is, a number or boolean as its JSON text, and a typed or
language-tagged literal (`{"$": …, "type": …}`, `{"$": …, "lang": …}`) as the text of its `$`. A
value of any other shape is left out.

### `parseNotes`

Remarks from reading the document leniently. They say nothing about whether the document is valid.
Always empty for now.

| Field | Type | Meaning |
| --- | --- | --- |
| `message` | string | What the reader did, in one sentence. |
| `subject` | string \| null | Key of the bundle, element or relation the note concerns, or `null` for the whole document. |

### `validationFindings`

Violations of PROV-DM or CPM rules reported by a validator. The validators are not built yet, so
this list is always empty. A finding has the same fields as a parse note: `message` and `subject`.

## Fixtures

The graphs of a few test documents are committed under `testdata/contract/`, at the same path as
the document under `testdata/`. The API tests check that the backend still produces them, and the
frontend tests use them as input, so the two sides can't drift apart unnoticed. After an intended
contract change, regenerate them and review the diff:

```fish
UPDATE_CONTRACT_FIXTURES=1 dotnet test tests/CpmProvViz.Api.Tests
```

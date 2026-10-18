# EMBRC test documents

Six real CPM documents from the EMBRC example: a marine sampling and analysis workflow recorded
as a provenance chain of four provenance components. Each file is a PROV-JSON document holding
one bundle.

## Origin

The supervisor provided these files. They match the EMBRC example in CPF-Toolbox's own test data
(`cpm-template/src/test/resources/embrc/`, https://github.com/Common-Provenance-Framework/CPF-Toolbox),
except for the identifiers. A provenance storage generated these, so bundles and meta components
have UUIDs rather than names like `SamplingBundle_V0`.

All files use the current CPM namespace `https://www.commonprovenancemodel.org/cpm-namespace-v1-0/`
and the prefix `storage` = `http://localhost:8080/api/v1/organizations/6fb292aa-ee38-48ae-998f-079ad9d01e7c/documents/`.

## Files

| File | Bundle | Main activity |
| --- | --- | --- |
| `dataset1/SamplingBundle_V0.json` | `storage:dc8efed0-0035-4029-9065-8c46667151db` | `blank:Sampling` |
| `dataset1/SamplingBundle_V1.json` | `storage:b4becbce-5ee4-4408-87e3-5add55b72c5f` | `blank:Sampling` |
| `dataset2/ProcessingBundle_V0.json` | `storage:4a63c118-47f2-4358-a0b9-4562314663b4` | `blank:Processing` |
| `dataset2/ProcessingBundle_V1.json` | `storage:16d36e10-62e0-49f7-af62-b4ec589cfa28` | `blank:Processing` |
| `dataset3/SpeciesIdentificationBundle_V0.json` | `storage:951cc66b-8987-42ad-b104-333d9eb6a63a` | `blank:SpeciesIdentification` |
| `dataset4/DnaSequencingBundle_V0.json` | `storage:014adc06-9b44-4875-b9a7-616d3fcbfb6d` | `blank:DnaSequencing` |

## How they chain

```
Sampling ──► Processing ──► SpeciesIdentification
    │
    └──────► DnaSequencing
```

A backward connector names the provenance component it continues from in
`cpm:referencedBundleId`. Every backward connector points at a `_V0` provenance component, even
where a `_V1` exists.

- **Sampling** is the start of the chain. `_V0` has two forward connectors:
  `blank:StoredSampleCon_r1` and `blank:StoredSampleCon_r2_3um`.
- **Processing** has the backward connector `blank:StoredSampleCon_r1`, which references
  Sampling `_V0`. Its forward connector is `blank:ProcessedSampleCon`.
- **SpeciesIdentification** has the backward connector `blank:ProcessedSampleCon`, which
  references Processing `_V0`. Its forward connector is `blank:IdentifiedSpeciesCon`.
- **DnaSequencing** has the backward connector `blank:StoredSampleCon_r2_3um`, which references
  Sampling `_V0`. Its forward connector is `blank:FilteredSequencesCon`.

### V0 and V1

`_V1` is a new version of a component, made after the components that follow it existed. Each
`_V1` adds specialised forward connectors (`cpm:specForwardConnector`) that reference those
following components, together with the receiver agents:

- **Sampling `_V1`** adds `blank:StoredSampleCon_r1_Spec` (references Processing `_V0`),
  `blank:StoredSampleCon_r2_3um_Spec` (references DnaSequencing `_V0`) and
  `blank:ProcessedSampleConSpec` (references SpeciesIdentification `_V0`). Its receiver agents
  are `blank:NiceMarineStation` and `blank:SequencingIsUsHQ`.
- **Processing `_V1`** adds `blank:ProcessedSampleConSpec` (references SpeciesIdentification
  `_V0`). Its agent `blank:NiceMarineStation` is typed both `cpm:senderAgent` and
  `cpm:receiverAgent`.

SpeciesIdentification and DnaSequencing have only `_V0`.

### Redundant connector

SpeciesIdentification also holds a redundant backward connector, `blank:StoredSampleCon_r1`,
which references Sampling `_V0` directly. It skips Processing. Its backward connector
`blank:ProcessedSampleCon` `wasDerivedFrom` it. Sampling `_V1` has the matching redundant forward
connector: `blank:ProcessedSampleCon` `wasDerivedFrom` `blank:StoredSampleCon_r1`, and it is
specialised by `blank:ProcessedSampleConSpec`.

See `docs/reference/prov-json-and-cpm.md` (section B.6) for the CPM rules on connectors and
redundant connectors.

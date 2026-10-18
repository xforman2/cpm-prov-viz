# CPM Provenance Viewer

Viewing provenance recorded with the Common Provenance Model (CPM): W3C PROV documents whose
provenance components are chained across organisations through shared connectors.

## PROV

**Document**:
A PROV document: the unit the viewer loads, holding statements and bundles.

**Bundle**:
A named set of PROV statements inside a document. Every provenance component is a bundle; a bundle
is a provenance component only when it follows CPM.

## CPM

**Provenance component**:
The CPM unit of provenance: an immutable, persistently identified bundle documenting one described
activity of one organisation.
_Avoid_: FPC, bundle (when the CPM unit is meant)

**Provenance chain**:
Provenance components linked to each other through connectors.

**Traversal information**:
The part of a provenance component used to navigate the provenance chain: the main activity, the
connectors, the sender and receiver agents, and the relations between them.
_Avoid_: traversal part, backbone

**Domain-specific information**:
Everything in a provenance component that is not traversal information: the described activity
documented in domain terms.
_Avoid_: domain part

**Main activity**:
The single activity that stands for the described activity of a provenance component.

**Connector**:
An entity whose identifier is shared by two provenance components, representing the object passed
from one to the other.

**Forward connector**:
A connector representing an object that can be or has been sent from this provenance component to
a receiver.

**Backward connector**:
A connector representing an object received from another provenance component, which it references.

**Sender agent**:
The organisation that sent the object a connector represents.

**Receiver agent**:
The organisation that received the object a connector represents.

## Reading and checking

**Parse note**:
A remark from reading a document leniently: something skipped, merged or not declared. It says
nothing about whether the document is valid.
_Avoid_: warning, error, diagnostic

**Validation finding**:
A violation of PROV-DM or CPM rules reported by a validator.
_Avoid_: parse note, error

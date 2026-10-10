# cpm-prov-viz

Interactive web viewer for **Common Provenance Model (CPM)** provenance: W3C PROV documents
split into per-organisation bundles that are chained together through shared connector IDs.
Part of a master's thesis. The public repo holds the tool, its docs and its test data only.
Thesis text and research notes live elsewhere.

No existing viewer handles CPM: the tested ones drop bundles, crash on typed values, ignore
`specializationOf` and never show links between bundles. Correct CPM semantics come first and
looks come second.

## Status

Early stage, nothing is built yet. The architecture below is **planned**. When something
becomes real, update this file with the actual commands and paths.

## Planned architecture

```
PROV-JSON ──► parser (C#, ASP.NET Core) ──► graph JSON ──► frontend (JS/TS)
                normalise, CPM checks        contract      ELK layout → Cytoscape.js render
```

- **Parser / backend (C#, .NET 10):** reads PROV-JSON (PROV-N later, maybe). It normalises the
  input and runs CPM checks: connector pairs, `cpm:referencedBundleId`, traversal vs domain
  part. It returns the graph JSON. It may also fetch bundles from remote provenance services,
  because server-side fetching avoids CORS.
- **Graph JSON contract:** the single source of truth between backend and frontend. It will be
  specified in `docs/graph-contract.md`. Change the contract doc first, then both sides.
- **Frontend:** Cytoscape.js for rendering and interaction. Bundles are compound nodes and can be
  collapsed. elkjs (Layered, `INCLUDE_CHILDREN`) does the layout in a Web Worker. Details are
  shown in HTML side panels, not inside nodes.
- **Scaling strategy:** bundles are collapsed by default and expanded on click or zoom. Layout is
  computed per bundle and cached, which is safe because CPM bundles are immutable. Agent edges
  don't constrain the layering. React Flow was rejected because it became unusable at about
  1200 nodes.

## Domain rules (the parser must get these right)

- Element IDs are **scoped per bundle**. The same connector ID in two bundles means two nodes
  plus an explicit inter-bundle CPM link. Never merge them silently.
- PROV edges point **into the past** (effect → cause), e.g. `wasGeneratedBy: entity → activity`.
- `specializationOf` connects the domain part to the connectors. Dropping it splits the graph
  into islands.
- python-prov output contains **typed values** (`{"$": "prov:Person", "type": "xsd:QName"}`),
  **arrays of descriptions** for the same ID, and relation IDs like `_:id1` repeated across
  bundles. All of these must be handled.
- There are two PROV-JSON flavours: python `prov` and ProvToolbox. Support both, and test
  against both.

## Test data

- `test-data/` (planned) holds one **minimal file per edge case**, each with an expected parser
  output (golden file). Add a new case whenever a bug is found.
- Larger samples: a CPM chain of 3 bundles, plus synthetic graphs of 174 and 1200 nodes.

## Environment

- Arch Linux, **fish** shell (not bash). The `hostname` command is not installed; use `uname -n`.
- Node 26, .NET 10, Python 3.14.
- GitHub: `xforman2/cpm-prov-viz` (public), default branch `development`.

## Working rules

- If I only ask a question, **answer it**. Don't build, install or run anything unless I ask.
- Dev servers bind to **127.0.0.1** only. Use fixed ports, and stop servers when done.
- Write factual claims about external tools or specs **with a date**
  (e.g. "checked 2026-10-10").
- Never copy code from GPL projects (e.g. yProv Explorer). Borrowing ideas is fine.
- Docs and code comments are in English.

## Git workflow and issues

- **All work is tracked in GitHub issues** (`gh issue list`, `gh issue view <n>`). Every change
  belongs to an issue. If there is none, ask before creating one.
- **Branches:**
  - `main` holds released versions only.
  - `development` is the integration branch and the default branch.
  - Both are protected on GitHub: no force push, no deletion, changes only through pull requests.
- **Work on a short-lived branch** created from `development` and named
  `<type>/<issue>-<short-desc>`, e.g. `feat/12-bundle-parser`, using the commit types below.
- **Commits follow [Conventional Commits](https://www.conventionalcommits.org/):**
  `<type>(<optional scope>): <subject>`, e.g. `feat(parser): parse typed attribute values`.
  - Types: `feat`, `fix`, `docs`, `refactor`, `test`, `perf`, `build`, `ci`, `chore`.
  - The subject is imperative and lowercase, with no trailing period.
  - **Subject line only: no body, no description.** Explanations belong in the PR.
  - Mark breaking changes with `!`, e.g. `feat(contract)!: rename node kind field`.
- **Pull requests** go into `development` and are **squash-merged**. The PR title becomes the
  commit on `development`, so it must be a Conventional Commit too. The PR description explains
  the change and contains `Closes #12`, so the issue closes on merge.
- **Release:** open a PR from `development` into `main` and merge it with a **merge commit**
  (never squash; squashing makes `main` and `development` diverge). Then tag `main` as `vX.Y.Z`.
- **Claude:**
  - Never push to `main` or `development` directly, never force-push, and never delete those
    branches.
  - Commit, push, open PRs and merge PRs only when asked.
- **Enforcement:**
  - The GitHub ruleset "protect main and development".
  - Deny and ask rules in `.claude/settings.json`.

## Verification

Check UI changes by running the app, loading a file from `test-data/` and taking a screenshot
(Playwright), not just by building. Check parser changes against the golden files.
<!-- TODO: add exact build / run / test commands once they exist -->

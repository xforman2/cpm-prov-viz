# cpm-prov-viz

Interactive web viewer for **Common Provenance Model (CPM)** provenance: W3C PROV documents
split into per-organisation bundles that are chained together through shared connector IDs.
Part of a master's thesis. The public repo holds the tool, its docs and its test data only.
Thesis text and research notes live elsewhere.

No existing viewer handles CPM: the tested ones drop bundles, crash on typed values, ignore
`specializationOf` and never show links between bundles. Correct CPM semantics come first and
looks come second.

## Status

The graph contract and a first PROV-JSON reader behind `POST /api/graph` exist. The backend
interprets CPM: each element's CPM roles, traversal information vs domain-specific information,
and which bundles are provenance components. The frontend opens a file, draws its graph with the
traversal information styled apart, can hide the domain-specific information, and shows the
selected element or relation in a side panel. The menu and the side panel collapse to thin strips.
The Java provenance service runs under the AppHost and serves only health; nothing calls it yet.

## Structure

```
src/
  CpmProvViz.Domain/           PROV/CPM model, no dependencies
  CpmProvViz.Application/      use cases (load, normalise, CPM checks, map to graph contract) → Domain
  CpmProvViz.Infrastructure/   PROV-JSON reading, file and remote loading → Application
  CpmProvViz.Api/              ASP.NET Core minimal API → Application, Infrastructure
  CpmProvViz.ServiceDefaults/  shared Aspire setup (health checks, telemetry)
  CpmProvViz.AppHost/          Aspire AppHost: runs the API and the frontend together
tests/                         xUnit projects; CpmProvViz.Api.Tests tests at the HTTP seam
testdata/                      PROV-JSON test documents (EMBRC example: `testdata/embrc/README.md`,
                               hand-made ones: `testdata/handmade/`) and contract fixtures
                               (`testdata/contract/`)
provenance-service/            Java 25 + Spring Boot 4 (Maven) service that takes over PROV and CPM
                               handling
frontend/                      React + TypeScript (Vite), Cytoscape.js + elkjs; no automated tests,
                               checked by eye in the running app
CpmProvViz.slnx
```

Keep the Clean Architecture dependency direction: Domain depends on nothing, and nothing in
Domain or Application references ASP.NET Core, files or HTTP.

Each use case in Application is its own class named `<Action>UseCase`, with one `ExecuteAsync`
method and only the dependencies it needs (e.g. `ReadGraphUseCase`). No `…Service` classes and
no mediator. Every use case and every service has an interface (`IReadGraphUseCase`,
`IProvDocumentReader`), is registered by it and is injected only through it. A use case takes its
own input record (`ReadGraphRequest`) and returns its own output DTO (`GraphDto`); input and
output never share a type. Every type, DTOs included, lives in its own file, even when one holds
another.

The one-type-per-file rule holds in the frontend too: every TypeScript type and interface, component
props included, lives in its own file named after it, e.g. `frontend/src/api/contract/GraphElement.ts`.
Each component also gets its own file named after it. Functions are grouped by topic instead: one
module per topic, e.g. `frontend/src/api/graph.ts` holds `readGraph` and its hook `useReadGraph`.

Frontend styles: `frontend/src/index.css` holds only the design tokens (CSS custom properties) and
the reset. Each component keeps its styles in a CSS module next to it (`GraphView.module.css`). The
Cytoscape stylesheet lives in `frontend/src/graph/style.ts` with its own colours, font and sizes as
constants: CSS can't reach the canvas, and nothing in the HTML uses them.

Application is split by feature. `<Feature>/Contract/` holds the DTOs the feature's use cases
return, `<Feature>/<Action>/` holds one use case with its interface and request record, and
`Ports/` holds the interfaces Infrastructure implements and the types in their signatures, e.g.
`Graph/Contract/GraphDto.cs`, `Graph/ReadGraph/ReadGraphUseCase.cs`,
`Ports/IProvDocumentReader.cs` and the `Ports/UnreadableDocumentException.cs` it throws.

Never use local functions (a function declared inside a method). Use private methods, and when
they share per-call state, a small class created per call that holds it in fields (e.g.
`GraphBuilder`). Never keep per-call state in fields of a use case or service: they are singletons.

Don't use the spread element `..` in C# collection expressions (`[.. items.Select(…)]`): write
`.ToList()`, `.ToArray()` or `.ToHashSet()` at the end of the query, and `Concat` to join
sequences. Plain collection expressions such as `[]` and `["a", "b"]` are fine.

Each layer registers its own services in a `DependencyInjection` class with one extension method
(`AddApi`, `AddApplication`, `AddInfrastructure`), and `Program.cs` only calls those. Domain
registers nothing and stays free of dependencies.

A service that only works on domain types (no ports, DTOs or I/O) is a domain service and lives
in Domain with its interface, e.g. `Domain/Cpm/ICpmInterpreter.cs`. `AddApplication` registers it.

## Commands

```fish
dotnet run --project src/CpmProvViz.AppHost --launch-profile http   # API + frontend + dashboard
dotnet build CpmProvViz.slnx
dotnet test CpmProvViz.slnx
npm --prefix frontend run build               # type-check + production build
npm --prefix frontend run lint
provenance-service/mvnw -f provenance-service/pom.xml spring-boot:run
```

The frontend calls the API through the Vite proxy (`/api/*`). The AppHost passes the API address
in as `API_HTTPS` / `API_HTTP`.

In Development, Swagger UI runs at http://localhost:5024/swagger, built from the OpenAPI document
at `/openapi/v1.json`. Neither is available outside Development.

## Architecture

```
PROV-JSON ──► parser (C#, ASP.NET Core) ──► graph JSON ──► frontend (React + TypeScript)
                normalise, CPM checks        contract      ELK layout → Cytoscape.js render
```

- **Parser / backend (C#, .NET 10):** reads PROV-JSON (PROV-N later, maybe). It normalises the
  input and runs CPM checks: connector pairs, `cpm:referencedBundleId`, traversal information
  vs domain-specific information. It returns the graph JSON. It may also fetch bundles from
  remote provenance services, because server-side fetching avoids CORS.
- **Provenance service (Java, Spring Boot 4):** all PROV and CPM handling moves here, onto
  CPF-Toolbox, and the C# code that reads PROV and interprets CPM is temporary. Where CPF-Toolbox
  semantics differ from ours, CPF-Toolbox wins. The Java code follows the same rules as the C#
  code: one type per file, an interface for every use case and service, `<Action>UseCase`, no local
  functions and the Clean Architecture direction, in one Maven module with the packages `domain`,
  `application`, `infrastructure` and `api`.
- **Graph JSON contract:** the single source of truth between backend and frontend. It is
  specified by `GraphDto` and the `…Dto` records it holds (`src/CpmProvViz.Application/Graph/Contract/`)
  and their XML comments, which the OpenAPI document publishes. Change the records first, then
  the frontend.
- **Frontend:** React for the menus and panels, TanStack Query for the API calls, Cytoscape.js for
  rendering and interaction.
  Bundles are compound nodes and can be collapsed. elkjs (Layered, `INCLUDE_CHILDREN`) does the
  layout in a Web Worker. Details are shown in HTML side panels, not inside nodes.
- **Scaling strategy:** bundles are collapsed by default and expanded on click or zoom. Layout is
  computed per bundle and cached, which is safe because CPM bundles are immutable. Agent edges
  don't constrain the layering. React Flow was rejected because it became unusable at about
  1200 nodes.

## Git workflow and issues

- **All work is tracked in GitHub issues** (`gh issue list`, `gh issue view <n>`). Every change
  belongs to an issue. If there is none, ask before creating one.
- **Issue titles are lowercase only**, e.g. `project scaffolding`, `swagger`.
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

## Agent skills

### Issue tracker

GitHub Issues on `xforman2/cpm-prov-viz`, via the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

The default five: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

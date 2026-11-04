# cpm-prov-viz

Interactive web viewer for **Common Provenance Model (CPM)** provenance: W3C PROV documents
split into per-organisation bundles that are chained together through shared connector IDs.
Part of a master's thesis. The public repo holds the tool, its docs and its test data only.
Thesis text and research notes live elsewhere.

No existing viewer handles CPM: the tested ones drop bundles, crash on typed values, ignore
`specializationOf` and never show links between bundles. Correct CPM semantics come first and
looks come second.

## Status

The frontend reads documents through the Java provenance service, with no C# in between. Its
`POST /api/graph` reads a PROV-JSON document with ProvToolbox, builds CPF-Toolbox's CPM view of each
bundle and returns the graph contract: each element's CPM roles and traversal information vs
domain-specific information, as the toolbox decides. The frontend opens a file, draws its graph with
the traversal information styled apart, can hide the domain-specific information, and shows the
selected element or relation in a side panel. The menu and the side panel collapse to thin strips.
The C# API still runs under the AppHost with its own `POST /api/graph`, but nothing uses it now; its
code and tests stay until it is removed.

## Structure

```
src/                           C# code, not used now (see Status)
  CpmProvViz.Domain/           PROV/CPM model, no dependencies
  CpmProvViz.Application/      use cases (load, normalise, CPM checks, map to graph contract) → Domain
  CpmProvViz.Infrastructure/   PROV-JSON reading, file and remote loading → Application
  CpmProvViz.Api/              ASP.NET Core minimal API → Application, Infrastructure
  CpmProvViz.ServiceDefaults/  shared Aspire setup (health checks, telemetry)
  CpmProvViz.AppHost/          Aspire AppHost: runs the provenance service, the C# API and the
                               frontend together
tests/                         xUnit projects; CpmProvViz.Api.Tests tests at the HTTP seam
testdata/                      PROV-JSON test documents (EMBRC example: `testdata/embrc/README.md`,
                               hand-made ones: `testdata/handmade/`) and contract fixtures
                               (`testdata/contract/`)
provenance-service/            Java 25 + Spring Boot 4 (Maven) service that does the PROV and CPM
                               handling, on CPF-Toolbox 2.5.0 from JitPack (see Java below)
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
dotnet run --project src/CpmProvViz.AppHost --launch-profile http   # services + frontend + dashboard
dotnet build CpmProvViz.slnx
dotnet test CpmProvViz.slnx
npm --prefix frontend run build               # type-check + production build
npm --prefix frontend run lint
provenance-service/mvnw -f provenance-service/pom.xml spring-boot:run
provenance-service/mvnw -f provenance-service/pom.xml verify          # compile, format check
provenance-service/mvnw -f provenance-service/pom.xml spotless:apply  # format the Java code
```

The frontend calls the provenance service through the Vite proxy (`/api/*`). The AppHost passes
its address in as `PROVENANCE_SERVICE_HTTP`.

In Development, the C# API's Swagger UI runs at http://localhost:5024/swagger, built from the OpenAPI document
at `/openapi/v1.json`. Neither is available outside Development.

## Architecture

```
PROV-JSON ──► provenance service (Java) ──► graph JSON ──► frontend (React + TypeScript)
               ProvToolbox, CPF-Toolbox      contract      ELK layout → Cytoscape.js render
```

- **Provenance service (Java, Spring Boot 4):** does all PROV and CPM handling, on CPF-Toolbox.
  It reads PROV-JSON with ProvToolbox, builds one CPF-Toolbox `CpmDocument` per bundle and maps the
  toolbox's nodes and edges to the graph JSON. Where CPF-Toolbox semantics differ from ours,
  CPF-Toolbox wins. It may later fetch bundles from remote provenance services, because
  server-side fetching avoids CORS.
- **C# backend (.NET 10):** the first reader and CPM interpreter, behind its own `POST /api/graph`.
  Nothing uses it now; it stays registered and running, with its code and tests, until it is
  removed.
- **Graph JSON contract:** the single source of truth between the provenance service and the
  frontend. It is specified by `GraphDto` and the records and enums it holds
  (`provenance-service/src/main/java/cz/muni/fi/cpmprovviz/provenance/application/graph/contract/`)
  and their Javadoc. Change the records first, then the frontend.
- **Frontend:** React for the menus and panels, TanStack Query for the API calls, Cytoscape.js for
  rendering and interaction.
  Bundles are compound nodes and can be collapsed. elkjs (Layered, `INCLUDE_CHILDREN`) does the
  layout in a Web Worker. Details are shown in HTML side panels, not inside nodes.
- **Scaling strategy:** bundles are collapsed by default and expanded on click or zoom. Layout is
  computed per bundle and cached, which is safe because CPM bundles are immutable. Agent edges
  don't constrain the layering. React Flow was rejected because it became unusable at about
  1200 nodes.

## Java

The provenance service is one Maven module. The C# rules above apply to it too: one type per file,
an interface for every use case and service, injected only through it, no local functions and the
Clean Architecture direction. Packages under `cz.muni.fi.cpmprovviz.provenance`:

```
api/                           controllers (`GraphController`, `InfoController` for `GET /api/info`),
                               `ProblemHandler`, which turns a failed read into a 400 naming the step
application/
  ApplicationConfiguration     registers the use cases by interface
  graph/contract/              `GraphDto` and every record and enum it holds, one per file
  graph/readgraph/             `ReadGraphUseCase`, `ReadGraphUseCaseImpl`, `ReadGraphRequest`, and the
                               per-call `GraphBuilder` and `ScopeMapper` that map CPF-Toolbox's nodes
                               and edges to the contract
  ports/                       interfaces infrastructure implements (`ProvDocumentReader`) and the
                               types in their signatures (`UnreadableDocumentException`)
infrastructure/
  InfrastructureConfiguration  registers the port implementations by port
  ProvJsonDocumentReader       ProvToolbox's PROV-JSON reader
```

`domain/` comes once something needs it. Names follow Java, not C#: no `I` prefix. A use case is an
interface named `<Action>UseCase` with one `execute` method, implemented by `<Action>UseCaseImpl`;
its input is `<Action>Request`. A port is an interface named after what it does
(`ProvDocumentReader`), implemented by a class named after how (`ProvJsonDocumentReader`). Each
layer registers its beans by interface in its own `@Configuration` class.

The code compiles with `-Xlint:all -Werror` and is formatted with palantir-java-format through
Spotless: `mvnw spotless:apply` formats it, and `mvnw verify` fails when it is not formatted.

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

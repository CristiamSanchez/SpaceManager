# Phase 1 — Project Foundation

You are working on an existing software project.

Before making any changes, inspect the current repository and understand its existing structure.

## Mandatory instructions

Read these files first:

1. `README.md`
2. `AGENTS.md`
3. `docs/domain-model.md`

Treat these files as the current source of truth for the project.

Do not recreate, replace, or rewrite information that already exists.

Do not assume that the repository is empty.

---

# Objective

Implement the initial project foundation for the Reservation System using:

* .NET 10
* ASP.NET Core
* Clean Architecture
* Minimal API
* Entity Framework Core
* PostgreSQL

The goal of this phase is ONLY to establish the project structure and database foundation.

Do not implement authentication, JWT, reservations, business use cases, or API endpoints yet.

---

# Step 1 — Inspect the repository

Before modifying anything:

* Inspect the current directory structure.
* Identify the existing `.sln` file.
* Identify existing projects.
* Identify existing source files.
* Identify existing test projects.
* Identify existing NuGet packages.
* Identify the current .NET SDK/framework configuration.
* Check whether the project already contains Docker or PostgreSQL configuration.
* Check whether Entity Framework Core is already configured.

Do not modify anything during this inspection step.

After inspecting the repository, determine what already exists and what is missing.

---

# Step 2 — Preserve existing work

If the repository already contains part of the required structure:

* Reuse it.
* Do not recreate it.
* Do not delete working code.
* Do not rename projects unless it is strictly necessary.
* Do not introduce a second solution or duplicate projects.

If an existing implementation conflicts with the architecture defined in `README.md` and `AGENTS.md`, identify the conflict and make the smallest necessary correction.

---

# Step 3 — Establish Clean Architecture

The solution must contain these main projects:

```text
Reservation.Api
Reservation.Application
Reservation.Domain
Reservation.Infrastructure
```

Tests should be organized separately.

The dependency direction must follow:

```text
Domain
   ↑
Application
   ↑
Infrastructure

Api → Application
Api → Infrastructure
```

Important:

* `Domain` must not reference `Application`.
* `Domain` must not reference `Infrastructure`.
* `Domain` must not reference `Api`.
* `Application` must not reference `Infrastructure`.
* `Application` must not reference `Api`.
* `Infrastructure` may reference `Application` and `Domain`.
* `Api` may reference `Application` and `Infrastructure`.

Verify the project references after creating them.

---

# Step 4 — Establish Entity Framework Core

Configure Entity Framework Core in the appropriate Infrastructure project.

Use PostgreSQL as the database provider.

Do not implement the complete domain model yet.

For this phase, create only the minimum infrastructure required to establish the database foundation.

Do not create repositories, services, handlers, endpoints, authentication, or business logic unless they are strictly required for the initial EF Core configuration.

---

# Step 5 — Database configuration

Prepare the application to connect to PostgreSQL through configuration.

Use a connection string configuration approach appropriate for ASP.NET Core.

Do not hardcode credentials inside source code.

Use configuration placeholders where necessary.

Do not expose passwords or secrets in source-controlled files.

If Docker/PostgreSQL configuration already exists, inspect and reuse it instead of creating a duplicate configuration.

If Docker/PostgreSQL configuration does not exist, create only the minimal configuration required for local development.

---

# Step 6 — Initial DbContext

Create the Infrastructure `DbContext`.

The DbContext should be located in the Infrastructure layer.

At this stage, do not implement every entity from `docs/domain-model.md`.

Only establish the DbContext and the required configuration structure.

If entity configurations are created, keep them organized so that the complete domain model can be added incrementally in later phases.

---

# Step 7 — Database design preparation

Use `docs/domain-model.md` as the source of truth for the future database model.

The main entities are:

```text
User
Service
Professional
ProfessionalService
Availability
Reservation
ReservationStatus
```

Do not invent additional business entities.

Do not implement relationships that have not been defined in the domain model.

Do not change the domain model during this phase.

If a technical decision is required but has not yet been defined, choose the simplest reasonable implementation and document the decision instead of silently changing the domain model.

---

# Step 8 — Dependency Injection

Configure the Infrastructure services so that the API can register Infrastructure dependencies.

Keep dependency registration organized and isolated from business logic.

Do not implement application use cases yet.

---

# Step 9 — Verification

After implementation:

1. Restore dependencies.
2. Build the entire solution.
3. Verify project references.
4. Verify that Entity Framework Core packages are compatible with .NET 10.
5. Verify that PostgreSQL configuration is syntactically correct.
6. Run existing tests if any exist.

If something fails, fix only problems introduced by this phase.

Do not perform unrelated refactoring.

---

# Step 10 — Update project state

After successfully completing the phase, update the project documentation.

If `docs/project-state.md` exists:

* Mark Phase 1 as completed.
* Record the important implementation decisions.
* Record the commands used for verification.
* Record the result of the build/tests.

If `docs/project-state.md` does not exist, create it according to the documentation conventions defined by the project.

Also update the phase status in `README.md` if required by the existing project conventions.

---

# Scope restrictions

Do NOT implement:

* JWT
* Authentication
* Authorization
* Roles
* User registration
* Login
* Reservation endpoints
* Service CRUD
* Professional CRUD
* Availability logic
* Reservation logic
* Business validation
* Advanced exception handling
* External services
* Payments
* Notifications

Those features belong to later phases.

---

# Efficiency requirements

This project is being developed using a free AI model.

Minimize unnecessary token usage.

Therefore:

* Do not reread unrelated files.
* Do not analyze the entire repository repeatedly.
* Do not recreate existing code.
* Do not generate large explanations.
* Do not perform speculative refactoring.
* Do not install packages that are not required.
* Do not implement future phases.
* Reuse existing conventions whenever possible.

Before creating a file, verify whether an equivalent file already exists.

Before adding a package, verify whether it is already installed.

Before changing a project reference, inspect the existing references.

---

# Final response

When the phase is complete, provide a concise summary containing:

## Changes

List the important files/projects created or modified.

## Architecture

Confirm the final project dependency direction.

## Database

Explain briefly what was configured for PostgreSQL and EF Core.

## Verification

Report:

* `dotnet restore`
* `dotnet build`
* tests, if applicable

Include the actual result.

## Pending

List only issues that remain unresolved.

Then STOP.

Do not continue to Phase 2.


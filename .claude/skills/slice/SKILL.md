\---

name: slice

description: Use when asked to add a new command or query slice to a module, wire up a new endpoint, or implement a new use case end to end through Application, Presentation and registration.

\---



\# Add a vertical slice



Add one command or query, its handler, its validator, its endpoint and its registration.

One use case per slice. Follow CLAUDE.md and AGENTS.md exactly; they are the contract.



\## Steps



1\. Read CLAUDE.md, then read the nearest existing slice in

&#x20;  `src/Modules/<Module>/Helpdesk.<Module>.Application/<Aggregate>/` and copy its shape.

&#x20;  The existing code is the specification; do not invent a second way of doing this.

2\. Create the Application file `<UseCase>.cs` beside its siblings. It holds, in order:

&#x20;  the command or query record implementing `ICommand<TResponse>` or `IQuery<TResponse>`,

&#x20;  the handler (internal, sealed, thin: load or create through the domain, call the domain

&#x20;  behavior, save through `IUnitOfWork`), and any response records the query returns.

3\. Add a validator implementing `IValidator<T>` in the same file when the input has a

&#x20;  shape to check. Validators check input shape only. Business rules belong in the domain

&#x20;  entity and are returned as a `Result` failure, never thrown.

4\. Add the minimal-API endpoint in

&#x20;  `src/Modules/<Module>/Helpdesk.<Module>.Presentation/<Aggregate>Endpoints.cs`.

&#x20;  Map the `Result` the handler returns: success to 201 Created for a create, 204 No Content

&#x20;  for a state change, 200 OK for a query; failure to 404 when the error code ends in

&#x20;  `NotFound`, and 400 Bad Request with the error code and message otherwise.

5\. Register the handler in the module's existing registration extension

&#x20;  (`TicketsApplication.cs` for the Tickets module). Do not add new infrastructure, a new

&#x20;  DI container, or a new registration file.

6\. Add a test. Domain rules go in `tests/Helpdesk.<Module>.Domain.Tests`. Status-code

&#x20;  mapping goes in `tests/Helpdesk.ApiTests`, which drives the real host against the SQL

&#x20;  Server container.

7\. Verify: run `docker compose up -d`, then `dotnet build Helpdesk.slnx` and

&#x20;  `dotnet test Helpdesk.slnx`. Show the output. Warnings are errors, so a build that

&#x20;  reports any warning is a failure.



\## Constraints



\- Never modify SharedKernel, the Domain layer, or an existing test to make a slice pass.

\- Never add a NuGet package. Never change the database schema without a migration.

\- If a step conflicts with an existing convention, stop and ask rather than improvising.




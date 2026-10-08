Create the solution skeleton described in CLAUDE.md:

\- Helpdesk.slnx at the root. Projects: src/Helpdesk.SharedKernel, src/Helpdesk.Host,

&#x20; src/Modules/Tickets/Helpdesk.Tickets.{Domain,Application,Infrastructure,Presentation},

&#x20; tests/Helpdesk.Tickets.Domain.Tests, tests/Helpdesk.ArchitectureTests.

\- SharedKernel, Domain, Application: classlib. Infrastructure: classlib with

&#x20; Microsoft.EntityFrameworkCore.SqlServer. Presentation: classlib with a

&#x20; Microsoft.AspNetCore.App framework reference. Host: web project.

\- Test projects use xUnit; ArchitectureTests also gets NetArchTest.Rules and references

&#x20; SharedKernel plus all four Tickets projects.

\- Wire ONLY these project references: Domain -> SharedKernel; Application -> Domain;

&#x20; Infrastructure -> Application; Presentation -> Application; Host -> Presentation,

&#x20; Infrastructure; Domain.Tests -> Domain; ArchitectureTests as above.

\- Each classlib gets one placeholder file ModuleInfo.cs in its root namespace:

&#x20; public static class ModuleInfo { public const string Name = "Tickets"; } (Name is

&#x20; "SharedKernel" in Helpdesk.SharedKernel). Each test project gets one PlaceholderTests.cs

&#x20; with a single xUnit fact asserting ModuleInfo.Name: Domain.Tests against

&#x20; Helpdesk.Tickets.Domain, ArchitectureTests against Helpdesk.SharedKernel. Delete each

&#x20; template's Class1.cs / UnitTest1.cs.

\- Host: minimal API with a /health endpoint returning "OK".

\- Everything must build with zero warnings (Directory.Build.props already sets

&#x20; TreatWarningsAsErrors).




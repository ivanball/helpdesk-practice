using System.Reflection;
using Helpdesk.Tickets.Application.Abstractions;
using NetArchTest.Rules;

namespace Helpdesk.ArchitectureTests;

// One test per rule in docs/architecture-rules.md.
public class ArchitectureRulesTests
{
    private const string SharedKernel = "Helpdesk.SharedKernel";
    private const string TicketsNamespace = "Helpdesk.Tickets";
    private const string TicketsDomain = "Helpdesk.Tickets.Domain";
    private const string TicketsInfrastructure = "Helpdesk.Tickets.Infrastructure";
    private const string TicketsPresentation = "Helpdesk.Tickets.Presentation";
    private const string Host = "Helpdesk.Host";

    private static readonly Assembly DomainAssembly = typeof(Tickets.Domain.ModuleInfo).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Tickets.Application.ModuleInfo).Assembly;

    // Rule 1
    [Fact]
    public void TicketsDomain_MayDependOnlyOn_SharedKernelAndSystem()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .OnlyHaveDependenciesOn(TicketsDomain, SharedKernel, "System")
            .GetResult();

        AssertSuccessful(result, "Tickets.Domain may depend only on Helpdesk.SharedKernel and System.");
    }

    // Rule 2
    [Fact]
    public void TicketsDomain_MustNotReference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertSuccessful(result, "Tickets.Domain must not reference Microsoft.EntityFrameworkCore anywhere.");
    }

    // Rule 3
    [Fact]
    public void TicketsApplication_MustNotDependOn_InfrastructureOrPresentation()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(TicketsInfrastructure, TicketsPresentation)
            .GetResult();

        AssertSuccessful(result, "Tickets.Application must not depend on Tickets.Infrastructure or Tickets.Presentation.");
    }

    // Rule 4
    [Fact]
    public void CommandAndQueryHandlers_MustBeInternal()
    {
        var handlers = Types.InAssemblies(HelpdeskAssemblies())
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>));

        // Guard against a selector that matches nothing and so passes by construction.
        Assert.NotEmpty(handlers.GetTypes());

        var result = handlers.Should().NotBePublic().GetResult();

        AssertSuccessful(result, "Command and query handlers must be internal, not public.");
    }

    // Rule 5
    [Fact]
    public void TypesOutsideTickets_MustNotDependOn_TicketsInfrastructure()
    {
        // Host is the composition root and is exempt, so its assembly is never scanned.
        var outsideTickets = HelpdeskAssemblies()
            .Where(a => !IsTicketsAssembly(a) && a.GetName().Name != Host)
            .ToArray();

        var types = Types.InAssemblies(outsideTickets)
            .That()
            .DoNotResideInNamespaceStartingWith(TicketsNamespace);

        Assert.NotEmpty(types.GetTypes());

        var result = types.ShouldNot().HaveDependencyOn(TicketsInfrastructure).GetResult();

        AssertSuccessful(result, "No type outside Helpdesk.Tickets.* may depend on Helpdesk.Tickets.Infrastructure (Host excepted).");
    }

    private static void AssertSuccessful(TestResult result, string rule)
    {
        var offenders = result.FailingTypeNames ?? [];
        Assert.True(
            result.IsSuccessful,
            $"{rule}{Environment.NewLine}Offending types:{Environment.NewLine}  "
                + string.Join(Environment.NewLine + "  ", offenders));
    }

    private static bool IsTicketsAssembly(Assembly assembly) =>
        assembly.GetName().Name!.StartsWith(TicketsNamespace + ".", StringComparison.Ordinal);

    // Every production Helpdesk assembly copied to the test output (excluding this test assembly),
    // so a new project referenced here is picked up by the rules without editing them.
    private static Assembly[] HelpdeskAssemblies()
    {
        var self = typeof(ArchitectureRulesTests).Assembly.GetName().Name;
        return Directory.GetFiles(AppContext.BaseDirectory, "Helpdesk.*.dll")
            .Select(path => AssemblyName.GetAssemblyName(path))
            .Where(name => name.Name != self)
            .Select(Assembly.Load)
            .ToArray();
    }
}

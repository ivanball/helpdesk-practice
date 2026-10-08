using System.Net;
using System.Net.Http.Json;

namespace Helpdesk.ApiTests;

public class CloseTicketTests(HelpdeskApiFactory factory) : IClassFixture<HelpdeskApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Close_open_ticket_returns_204_and_ticket_reports_Closed()
    {
        var id = await CreateTicket("Printer on fire");

        var close = await _client.PostAsync($"/tickets/{id}/close", null);

        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);

        var get = await _client.GetAsync($"/tickets/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var ticket = await get.Content.ReadFromJsonAsync<TicketBody>();
        Assert.Equal("Closed", ticket!.Status);
    }

    [Fact]
    public async Task Close_already_closed_ticket_returns_400_AlreadyClosed()
    {
        var id = await CreateTicket("Printer on fire");
        var first = await _client.PostAsync($"/tickets/{id}/close", null);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.PostAsync($"/tickets/{id}/close", null);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        var error = await second.Content.ReadFromJsonAsync<ErrorBody>();
        Assert.Equal("Tickets.AlreadyClosed", error!.Code);
    }

    private async Task<Guid> CreateTicket(string title)
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedBody>();
        return created!.Id;
    }

    private sealed record CreatedBody(Guid Id);
    private sealed record TicketBody(string Status);
    private sealed record ErrorBody(string Code, string Message);
}

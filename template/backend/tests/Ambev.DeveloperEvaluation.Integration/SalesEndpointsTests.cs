using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.TestSupport;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class SalesEndpointsTests
{
    [Fact(DisplayName = "Given valid sale payload When calling sales CRUD endpoints Then flow succeeds")]
    public async Task SalesCrudFlow_Works()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var createPayload = new
        {
            saleNumber = $"SALE-{Guid.NewGuid():N}"[..18],
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Integration Customer",
            branchId = Guid.NewGuid(),
            branchName = "Integration Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Integration Product",
                    quantity = 4,
                    unitPrice = 100m
                }
            }
        };

        var createResponse = await client.PostAsJsonAsync("/api/sales", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        createJson.GetProperty("success").GetBoolean().Should().BeTrue();
        var saleId = createJson.GetProperty("data").GetProperty("id").GetGuid();
        var productId = createPayload.items[0].productId;

        var getResponse = await client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        getJson.GetProperty("data").GetProperty("id").GetGuid().Should().Be(saleId);
        getJson.GetProperty("data").GetProperty("totalAmount").GetDecimal().Should().Be(360m);

        var updatePayload = new
        {
            saleNumber = createPayload.saleNumber,
            saleDate = createPayload.saleDate,
            customerId = createPayload.customerId,
            customerName = createPayload.customerName,
            branchId = createPayload.branchId,
            branchName = createPayload.branchName,
            isCancelled = false,
            cancelledItemProductIds = Array.Empty<Guid>(),
            items = new[]
            {
                new
                {
                    productId = productId,
                    productName = "Integration Product",
                    quantity = 10,
                    unitPrice = 100m
                }
            }
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/sales/{saleId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateJson = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        updateJson.GetProperty("data").GetProperty("totalAmount").GetDecimal().Should().Be(800m);

        var deleteResponse = await client.DeleteAsync($"/api/sales/{saleId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAfterDeleteResponse = await client.GetAsync($"/api/sales/{saleId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given item quantity above 20 When creating sale Then returns bad request")]
    public async Task CreateSale_QuantityAboveTwenty_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            saleNumber = $"SALE-{Guid.NewGuid():N}"[..18],
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Integration Customer",
            branchId = Guid.NewGuid(),
            branchName = "Integration Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Integration Product",
                    quantity = 21,
                    unitPrice = 100m
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/sales", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

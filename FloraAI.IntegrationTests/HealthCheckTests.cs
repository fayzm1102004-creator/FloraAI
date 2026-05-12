using FloraAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FloraAI.IntegrationTests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public HealthCheckTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task Test_Gemini_Narrative_Response()
    {
        using var scope = _factory.Services.CreateScope();
        var geminiService = scope.ServiceProvider.GetRequiredService<IGeminiService>();
        
        // Test 1: Healthy Plant
        var result1 = await geminiService.GenerateArabicTreatmentTextAsync("طماطم", "healthy");
        _output.WriteLine("=== Test 1: Healthy Plant ===");
        _output.WriteLine(result1);

        // Test 2: Sick Plant
        var result2 = await geminiService.GenerateArabicTreatmentTextAsync("مونستيرا", "بقع بنية على الأوراق", "فطريات وعفن");
        _output.WriteLine("\n=== Test 2: Sick Plant ===");
        _output.WriteLine(result2);

        // Test 3: Invalid Object (e.g. Cat)
        var result3 = await geminiService.GenerateArabicTreatmentTextAsync("قطة", "صورة قطة نائمة");
        _output.WriteLine("\n=== Test 3: Invalid Object ===");
        _output.WriteLine(result3);
    }
}

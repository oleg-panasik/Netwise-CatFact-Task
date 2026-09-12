using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetwiseCatFact;

public class CatFactDto
{
    [JsonPropertyName("fact")]
    public string Fact { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public int Length { get; set; }
}

public interface ICatFactService
{
    Task<CatFactDto?> GetRandomFactAsync();
}

public class CatFactService : ICatFactService
{
    private readonly HttpClient _httpClient;

    public CatFactService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CatFactDto?> GetRandomFactAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CatFactDto>("https://catfact.ninja/fact");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Błąd podczas pobierania danych: {ex.Message}");
            return null;
        }
    }
}

public interface IFileLoggerService
{
    Task AppendFactToFileAsync(string fact, string filePath);
}

public class FileLoggerService : IFileLoggerService
{
    public async Task AppendFactToFileAsync(string fact, string filePath)
    {
        try
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {fact}";
            await File.AppendAllTextAsync(filePath, logEntry + Environment.NewLine);
            Console.WriteLine($"[SUCCESS] Zapisano do pliku: {fact}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Błąd zapisu do pliku: {ex.Message}");
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddHttpClient<ICatFactService, CatFactService>();
        services.AddSingleton<IFileLoggerService, FileLoggerService>();

        var serviceProvider = services.BuildServiceProvider();

        var catFactService = serviceProvider.GetRequiredService<ICatFactService>();
        var fileLoggerService = serviceProvider.GetRequiredService<IFileLoggerService>();

        string filePath = "cat_facts.txt";

        Console.WriteLine("=== Netwise CatFact App ===");
        Console.WriteLine("Pobieranie danych z API...");

        var factData = await catFactService.GetRandomFactAsync();

        if (factData != null && !string.IsNullOrWhiteSpace(factData.Fact))
        {
            await fileLoggerService.AppendFactToFileAsync(factData.Fact, filePath);
        }
        else
        {
            Console.WriteLine("[WARNING] Nie udało się pobrać faktu o kotach.");
        }
    Console.WriteLine("\n--- RUNNING UNIT TESTS ---");
            await CatFactTests.Test_GetFact_ReturnsData();
            await CatFactTests.Test_FileLogger_WritesToFile();
    }
}
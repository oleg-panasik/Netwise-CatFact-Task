using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        int maxRetries = 3;
        int delayMilliseconds = 1000;

        // Если у HttpClient задан BaseAddress (как в тесте), стучимся по относительному пути, иначе по полному URL
        string requestUrl = _httpClient.BaseAddress == null 
            ? "https://catfact.ninja/fact" 
            : "";

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<CatFactDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                Console.WriteLine($"[WARNING] Attempt {attempt} failed with status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Attempt {attempt} failed: {ex.Message}");
            }

            if (attempt < maxRetries)
            {
                Console.WriteLine($"[INFO] Retrying in {delayMilliseconds / 1000}s...");
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2;
            }
        }

        Console.WriteLine("[ERROR] All retry attempts failed.");
        return null;
    }
}

public class FileLoggerService
{
    public async Task AppendFactToFileAsync(string fact, string filePath)
    {
        string formattedEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {fact}{Environment.NewLine}";
        await File.AppendAllTextAsync(filePath, formattedEntry);
        Console.WriteLine($"[SUCCESS] Zapisano do pliku: {fact}");
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        string filePath = "cat_facts.txt";

        Console.WriteLine("=== Netwise CatFact App ===");
        Console.WriteLine("Pobieranie danych z API...");

        using var httpClient = new HttpClient();
        var catFactService = new CatFactService(httpClient);
        var fileLoggerService = new FileLoggerService();

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
        await CatFactTests.Test_GetFact_RetryMechanism();
    }
}
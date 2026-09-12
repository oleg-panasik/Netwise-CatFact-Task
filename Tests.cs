using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetwiseCatFact
{
    public class CatFactTests
    {
        public static async Task Test_GetFact_ReturnsData()
        {
            using var httpClient = new HttpClient();
            var service = new CatFactService(httpClient);

            var result = await service.GetRandomFactAsync();

            if (result != null && !string.IsNullOrEmpty(result.Fact))
            {
                Console.WriteLine("[TEST SUCCESS] CatFact API service is working correctly.");
            }
            else
            {
                Console.WriteLine("[TEST FAILED] Failed to retrieve fact from API.");
            }
        }

        public static async Task Test_FileLogger_WritesToFile()
        {
            var logger = new FileLoggerService();
            string testFilePath = "test_log.txt";
            string testFact = "Cats have 32 muscles in each ear.";

            await logger.AppendFactToFileAsync(testFact, testFilePath);

            if (File.Exists(testFilePath) && File.ReadAllText(testFilePath).Contains(testFact))
            {
                Console.WriteLine("[TEST SUCCESS] FileLoggerService successfully wrote entry to file.");
                File.Delete(testFilePath);
            }
            else
            {
                Console.WriteLine("[TEST FAILED] FileLoggerService failed to write to file.");
            }
        }
    }
}
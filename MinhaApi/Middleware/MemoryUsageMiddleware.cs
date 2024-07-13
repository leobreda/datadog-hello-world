using Datadog.Trace;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace MinhaApi.Middleware
{
    public class MemoryUsageMiddleware
    {

        private static readonly HttpClient client = new HttpClient();


        private readonly RequestDelegate _next;

        public MemoryUsageMiddleware(RequestDelegate next)
        {
            _next = next;

            client.DefaultRequestHeaders.Add("DD-API-KEY", Environment.GetEnvironmentVariable("DD-API-KEY"));
            client.DefaultRequestHeaders.Add("DD-SITE", "datadoghq.com");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {

                var payload = new
                {
                    series = new[]
                    {
                        new
                        {
                            metric = "api.memoria",
                            points = new[] { new[] { DateTimeOffset.UtcNow.ToUnixTimeSeconds(), GetMemoryUsage() } },
                            type = "gauge"
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");


                //todo remover result

                var result = client.PostAsync("https://api.datadoghq.com/api/v1/series", content).Result;
            }
        }

        private long GetMemoryUsage()
        {
            // Usar o processo atual para obter o consumo de memória
            var process = Process.GetCurrentProcess();
            return process.WorkingSet64;
        }
    }
}

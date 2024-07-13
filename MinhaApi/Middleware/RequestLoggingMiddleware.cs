using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MinhaApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("status_code", context.Response.StatusCode);
                dic.Add("path", context.Request.Path);
                dic.Add("method", context.Request.Method);
                dic.Add("latency", stopwatch.ElapsedMilliseconds);
                dic.Add("ip", context.Connection.RemoteIpAddress?.ToString());
                dic.Add("client_id", context.Request.Headers["client_id"].ToString());

                //context.Request.Headers


                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("DD-API-KEY", Environment.GetEnvironmentVariable("DD-API-KEY"));
                client.DefaultRequestHeaders.Add("DD-SITE", "datadoghq.com");
                var payload = new
                {
                    series = new[]
                    {
                        new
                        {
                            metric = "api.latencia",
                            points = new[] { new[] { DateTimeOffset.UtcNow.ToUnixTimeSeconds(), dic["latency"] } },
                            type = "gauge",
                            tags = new[]
                            {
                                $"path:{dic["path"]}",
                                $"status_code:{dic["status_code"]}",
                                $"ip:{dic["ip"]}",
                                $"method:{dic["method"]}",
                                $"client_id:{dic["client_id"]}"
                                }
            }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                client.PostAsync("https://api.datadoghq.com/api/v1/series", content);

            }
        }
    }



}

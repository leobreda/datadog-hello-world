
using Microsoft.EntityFrameworkCore;
using MinhaApi;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using MinhaApi.Middleware;

namespace MinhaApi
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var settings = TracerSettings.FromDefaultSources();
            settings.ServiceName = "MinhaApi";
            Tracer.Configure(settings);

            builder.Services.AddDbContext<ProdutoContext>(options =>
                options.UseInMemoryDatabase("ProdutosDb"));
            builder.Services.AddControllers();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                //logging.AddDatadog();
            });

           

            var app = builder.Build();
            app.UseMiddleware<RequestLoggingMiddleware>();
            //app.UseMiddleware<MemoryUsageMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
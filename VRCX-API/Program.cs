using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using VRCX_API.Configs;
using VRCX_API.Helpers;
using VRCX_API.Services;

namespace VRCX_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(LoggerHelper.GetSerilogConfiguration().CreateLogger());
            });

            CommonConfig.Config.Load();

            builder.Services.AddSingleton<CloudflareService>();
            builder.Services.AddSingleton<GithubCacheService>();
            builder.Services.AddSingleton<CachePeriodicService>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<CloudflareService>());
            builder.Services.AddHostedService(provider => provider.GetRequiredService<CachePeriodicService>());

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
                    options.JsonSerializerOptions.Converters.Add(new IgnoreEmptyStringNullableEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });
#if DEBUG
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.DocumentFilter<PathLowercaseDocumentFilter>();
            });
#endif

            var app = builder.Build();

#if DEBUG
            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
#endif

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

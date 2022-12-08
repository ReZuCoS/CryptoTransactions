using CryptoTransactions.API.Model;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CryptoTransactions.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = GenerateApplication(args);

            SetConnectionString(app.Configuration.GetConnectionString("CryptoTransactions"));

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.EnableTryItOutByDefault());
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static WebApplication GenerateApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureSwaggerGen(c => {
                var filePath = Path.Combine(AppContext.BaseDirectory, "CryptoTransactions.API.xml");
                c.IncludeXmlComments(filePath);
            });

            return builder.Build();
        }

        private static void SetConnectionString(string? connectionString) =>
            CryptoTransactionsContext.ConnectionString = connectionString;
    }
}

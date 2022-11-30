using CryptoTransactions.API.Model;

namespace CryptoTransactions.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = GenerateApplication(args);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            SetConnectionString(app.Configuration.GetConnectionString("CryptoTransactions"));

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static WebApplication GenerateApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
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

using Microsoft.EntityFrameworkCore;
using EasyModbus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ITSystem.Context;
using ITSystem.Models;


namespace IntegrationSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Integration System – IT ↔ OT via Modbus";

            // Ladda konfiguration (appsettings.json med samma ConnectionString som ITSystem)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            var provider = services.BuildServiceProvider();
            using var db = provider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Koppla upp mot OT-systemet via Modbus
            var client = new ModbusClient("127.0.0.1", 502); // OT lyssnar på port 502
            try
            {
                client.Connect();
                Console.WriteLine("Ansluten till OT via Modbus.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FEL] Kunde inte ansluta till OT-systemet: {ex.Message}");
                return;
            }

            Console.WriteLine("Integration System körs. Tryck Ctrl+C för att avsluta.\n");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    // Hämta första order i status New
                    var order = db.Orders.FirstOrDefault(o => o.Status == OrderStatus.New);
                    if (order != null)
                    {
                        Console.WriteLine($"Skickar order #{order.Id} ({order.Item} x{order.Quantity}) till OT...");

                        // Skriv coil 0 = true → OT tar emot
                        client.WriteSingleCoil(0, true);

                        // Uppdatera orderstatus
                        order.Status = OrderStatus.Sent;
                        db.SaveChanges();
                    }
                    Thread.Sleep(2000); // loopa var 2 sek
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FEL] Integrationsfel: {ex.Message}");
                    Thread.Sleep(2000);
                }
            }
            client.Disconnect();
        }
    }
}

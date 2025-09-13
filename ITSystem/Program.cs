using ITSystem.Context;
using ITSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ITSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            var provider = services.BuildServiceProvider();

            using var db = provider.GetRequiredService<ApplicationDbContext>();
            //db.Database.EnsureCreated();
            //db.Users.RemoveRange(db.Users);
            //db.SaveChanges();

            SeedAdmin(db);

            Console.WriteLine("Välkommen till IT-Systemet. Logga in");
            Console.Write("Användarnamn: ");
            var user = Console.ReadLine() ?? string.Empty;
            Console.Write("Lösenord: ");
            //var pass = Console.ReadLine() ?? string.Empty;  // <-- Temporärt utan ReadPassword
            //Console.WriteLine($"DEBUG: user='{user}', pass='{pass}'");
            var pass = ReadPassword();

            if (!Authenticate(db, user, pass))
            {
                Console.WriteLine("[SÄKERHET] Inloggning misslyckades.");
                return;
            }

            Console.WriteLine("Inloggning lyckades.\n");

            while (true)
            {
                Console.WriteLine("1) Lista ordrar");
                Console.WriteLine("2) Skapa ny order");
                Console.WriteLine("3) Avsluta");
                Console.Write("Val: ");
                var choice = Console.ReadLine();
                try
                {
                    if (choice == "1") ListOrders(db);
                    else if (choice == "2") CreateOrder(db);
                    else if (choice == "3") break;
                    else Console.WriteLine("Ogiltigt val.\n");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"[FEL] Databasfel: {ex.Message}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FEL] Okänt fel: {ex.Message}\n");
                }
            }

            static void SeedAdmin(ApplicationDbContext db)
            {
                if (!db.Users.Any())
                {
                    var hash = PasswordHash.Hash("Admin!23");
                    db.Users.Add(new User { Username = "admin", PasswordHash = hash });
                    db.SaveChanges();
                    Console.WriteLine("[Init]Skapade admin/admin (lösenord: Admin!23");
                    
                }
            }

            static bool Authenticate(ApplicationDbContext db, string username, string password)
            {
                var user = db.Users.SingleOrDefault(x => x.Username == username);
                return user != null && PasswordHash.Verify(password, user.PasswordHash);
            }

            static void ListOrders(ApplicationDbContext db)
            {
                var orders = db.Orders.OrderByDescending(o => o.CreatedUtc).ToList();
                if (orders.Count == 0)
                {
                    Console.WriteLine("Inga ordrar funna.\n");
                    return;
                }
                foreach (var order in orders)
                {
                    Console.WriteLine($"{order.Id}| {order.CustomerName} | {order.Item} x {order.Quantity} | {order.Status})");
                    Console.WriteLine();
                }
            }

            static void CreateOrder(ApplicationDbContext db)
            {
                Console.Write("Kundnamn: ");
                var customer = Console.ReadLine();
                Console.Write("Artikel: ");
                var item = Console.ReadLine();
                Console.Write("Antal: ");
                var qtyStr = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(item) || !int.TryParse(qtyStr, out var qty) || qty < 1)
                {
                    Console.WriteLine("[FEL] Ogiltiga fält.\n");
                    return;
                }
                var order = new Order { CustomerName = customer, Item = item, Quantity = qty };
                db.Orders.Add(order);
                db.SaveChanges();
                Console.WriteLine($"Skapade order #{order.Id} (status: {order.Status}).\n");
            }

            static string ReadPassword()
            {
                Console.Write("");
                var password = "";
                ConsoleKeyInfo key;

                while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password[0..^1];
                        Console.Write("\b \b");
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                }
                Console.WriteLine();
                return password;


                //var password = string.Empty;
                //ConsoleKey key;
                //while ((key = Console.ReadKey(intercept: true).Key) != ConsoleKey.Enter)
                //{
                //    if (key == ConsoleKey.Backspace && password.Length > 0)
                //    {
                //        password = password[0..^1];
                //        Console.Write("\b \b");
                //        continue;
                //    }
                //    else if (!char.IsControl((char)key))
                //    {
                //        password += key.ToString();
                //        Console.Write("*");
                //    }
                //}
                //Console.WriteLine();
                //return password;
            }
        }

    }
}
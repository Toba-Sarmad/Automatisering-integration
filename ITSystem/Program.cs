using ITSystem.Context;
using ITSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ITSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SeedAdmin(db);

            Console.WriteLine("Välkommen till IT-Systemet. Logga in");
            Console.Write("Användarnamn: ");
            var user = Console.ReadLine(); string.Empty;
            Console.Write("Lösenord: ");
            var pass = ReadPassword();

            if(!Authenticate(db, user, pass))
            {
                Console.WriteLine("[SÄKERHET] Inloggning misslyckades.");
                return;
            }

            Console.WriteLine("Inloggning lyckades.\n");

            while(true)
            {
                Console.WriteLine("1) Lista ordrar");
                Console.WriteLine("2) Skapa ny order");
                Console.WriteLine("3) Avsluta");
                Console.Write("Val: "); 
                var choice = Console.ReadLine();
                try
                {
                    if (choice == "1") ListOrders();
                    else if (choice == "2") CreateOrder();
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
                
                static void SeedAdmin(ApplicationDbContext db)
                {
                    if (!db.Users.Any())
                    {
                        var hash = PasswordHash.Hash("admin$23");
                        db.Users.Add(new User { Username = "admin", PasswordHash = hash } );
                        db.SaveChanges();
                        Console.WriteLine(" Skapade admin/admin (lösenord: Admin$23");

                    }
                }
                static void ListOrders(ApplicationDbContext db)
                {

                }

                static void CreateOrder(ApplicationDbContext db)
                {
                }
            }
        }
    }
}

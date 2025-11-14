using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace HashGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            

            Console.WriteLine("BCrypt Hash Generator" +
                "\n-----------------------");

            // Hash for password123
            string password123 = "password123";
            // call includes a unique random salt
            string hash123 = BCrypt.Net.BCrypt.HashPassword(password123);
            Console.WriteLine($"Password: {password123}");
            Console.WriteLine($"Hash to use in SSMS for 'password123':\n{hash123}\n");

            // Hash for jadey456
            string password456 = "jadey456";
            string hash456 = BCrypt.Net.BCrypt.HashPassword(password456);
            Console.WriteLine($"Password: {password456}");
            Console.WriteLine($"Hash to use in SSMS for 'jadey456':\n{hash456}\n");

            Console.WriteLine("generated hash string for SSMS");
            Console.ReadKey();
        }
    }
}

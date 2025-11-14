using ClassLibrary.Data;
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ClassLibrary.Services
{
    public class SqlLoginService
    {
        private readonly ApplicationDbContext _context;

        //---------------------------------------------------------------------------------------------------------------------
        public SqlLoginService(ApplicationDbContext context)
        {
            _context = context;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<SqlCustomer?> AuthenticateUser(string email, string password)
        {
            var customer = await _context.SqlCustomers.FirstOrDefaultAsync(c => c.CustomerEmail == email);

            if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            {
                return customer; // if authentication is successful
            }

            return null; // if authentication failed
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task RegisterUser(string name, string email, string password, string role)
        {
            if (await _context.SqlCustomers.AnyAsync(c => c.CustomerEmail == email))
            {
                throw new InvalidOperationException("Email already registered.");
            }

            var newCustomer = new SqlCustomer
            {
                CustomerName = name,
                CustomerEmail = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            _context.SqlCustomers.Add(newCustomer);
            await _context.SaveChangesAsync();
        }

        //---------------------------------------------------------------------------------------------------------------------
    }
    
        //---------------------------------------------------------------------------------------------------------------------
}

//--------------------------------------------------------NED OF FILE-------------------------------------------------------------
using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClassLibrary.Data
{
    //-------------------------------------------------------------------------------------------------------------------------
    public class ApplicationDbContext : DbContext
    {
        //---------------------------------------------------------------------------------------------------------------------
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //---------------------------------------------------------------------------------------------------------------------
        public DbSet<SqlCustomer> SqlCustomers { get; set; } = default!;
        //---------------------------------------------------------------------------------------------------------------------

    }
}
//-------------------------------------------------------END OF FILE-----------------------------------------------------------
using EmployeeAdminPortal.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) 
        {
            

        }

         public DbSet<User> Users { get; set; }

    }


}

using Microsoft.EntityFrameworkCore;
using SafeTunnel.Models;

namespace SafeTunnel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Simulacion> Simulaciones { get; set; }
    }
}
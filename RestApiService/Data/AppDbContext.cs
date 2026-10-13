using Microsoft.EntityFrameworkCore;
using RestApiService.Models;

namespace RestApiService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products { get; set; }
    }
}
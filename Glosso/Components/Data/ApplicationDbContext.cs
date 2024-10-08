using Glosso.Services;
using Glosso.Models;
using Microsoft.EntityFrameworkCore;

namespace Glosso.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<User> Users { get; set; }
}
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace POS_Cafeteria.Models
{
    public class PosContext : DbContext
    {
        public PosContext(DbContextOptions<PosContext> options) : base(options) { }

        // Aquí está el truco: Mapeamos los nombres que buscan tus controladores 
        // a las tablas reales de SQL Server.
        public DbSet<Product> Products { get; set; }
        public DbSet<Product> PRODUCT { get; set; } // Por si acaso
        
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Sale> SALE { get; set; }

        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<SaleDetail> SALE_DETAIL { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<User> USERS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("PRODUCT");
    modelBuilder.Entity<Sale>().ToTable("SALE");
    
    // ESTA ES LA LÍNEA CLAVE:
    modelBuilder.Entity<SaleDetail>()
        .ToTable("SALE_DETAIL", t => t.HasTrigger("tr_ActualizarStock")); // Dile que hay un trigger
        
    modelBuilder.Entity<User>().ToTable("USERS");
        }
    }

    public class Product {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class Sale {
        public int SaleId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public int UserId { get; set; }
        public List<SaleDetail> Details { get; set; } = new();
    }

    public class SaleDetail {
        public int SaleDetailId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        [JsonIgnore] public Sale? Sale { get; set; }
        public Product? Product { get; set; }
    }

    public class User {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
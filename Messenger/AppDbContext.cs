using Microsoft.EntityFrameworkCore;

namespace Messenger.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> users { get; set; }
        public DbSet<Message> messages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к PostgreSQL
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=admin");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasKey(m => m.id);  // Устанавливаем первичный ключ для Message

            modelBuilder.Entity<Message>()
                .Property(m => m.senderid)
                .HasColumnName("senderid"); // Убедитесь, что эти имена совпадают с базой данных

            modelBuilder.Entity<Message>()
                .Property(m => m.receiverid)
                .HasColumnName("receiverid");
            modelBuilder.Entity<Message>()
                .Property(m => m.content)
                .HasColumnType("TEXT");
        }

    }
}

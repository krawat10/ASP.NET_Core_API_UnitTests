using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BookOrder>()
                .HasKey(order => new {order.OrderId, order.BookId});

            modelBuilder
                .Entity<BookOrder>()
                .HasOne(bookOrder => bookOrder.Order)
                .WithMany(order => order.BookOrders);

            modelBuilder
                .Entity<BookOrder>()
                .HasOne(bookOrder => bookOrder.Book)
                .WithMany(book => book.BookOrders);
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<BookOrder> BookOrders { get; set; }
    }
}
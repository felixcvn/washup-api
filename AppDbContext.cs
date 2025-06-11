using Microsoft.EntityFrameworkCore;
using WashUpAPIFix.Models;

namespace WashUpAPIFix
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<LaundryOrder> LaundryOrders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<LaundryService> LaundryServices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping tabel sesuai casing PostgreSQL
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<LaundryOrder>().ToTable("laundryorders");
            modelBuilder.Entity<LaundryService>().ToTable("laundryservices");
            modelBuilder.Entity<OrderDetail>().ToTable("orderdetails");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<Rating>().ToTable("ratings");

            // USER mapping
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.userid);
                entity.Property(e => e.userid).HasColumnName("userid");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.passwordHash).HasColumnName("passwordhash");
                entity.Property(e => e.role).HasColumnName("role");
            });

            modelBuilder.Entity<LaundryService>(entity =>
            {
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnName("price");
            });

            modelBuilder.Entity<LaundryOrder>(entity =>
            {
                entity.HasKey(e => e.LaundryOrderId);
                entity.Property(e => e.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.CourierId).HasColumnName("courierid");
                entity.Property(e => e.PickupAddress).HasColumnName("pickupaddress");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            });


            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentId).HasColumnName("paymentid");
                entity.Property(e => e.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(e => e.PaymentProofUrl).HasColumnName("paymentproofurl");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.PaidAt).HasColumnName("paidat");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.RatingId);
                entity.Property(e => e.RatingId).HasColumnName("ratingid");
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.OrderId).HasColumnName("orderid");
                entity.Property(e => e.Score).HasColumnName("score");
                entity.Property(e => e.Comment).HasColumnName("comment");
                entity.Property(e => e.RatedAt).HasColumnName("ratedat");
            });

            // Relasi User -> LaundryOrder (Customer)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi User -> LaundryOrder (Courier)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Deliveries)
                .WithOne(o => o.Courier)
                .HasForeignKey(o => o.CourierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi LaundryOrder -> Payment (1-1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.LaundryOrder)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.LaundryOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi LaundryOrder -> Rating (1-1)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.LaundryOrder)
                .WithOne(o => o.Rating)
                .HasForeignKey<Rating>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi LaundryOrder -> OrderDetails (1-n)
            modelBuilder.Entity<LaundryOrder>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.LaundryOrder)
                .HasForeignKey(od => od.LaundryOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi LaundryService -> OrderDetails (1-n)
            modelBuilder.Entity<LaundryService>()
                .HasMany(ls => ls.OrderDetails)
                .WithOne(od => od.LaundryService)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                // Composite key

                entity.Property(od => od.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(od => od.Quantity).HasColumnName("quantity");
                entity.Property(od => od.Subtotal).HasColumnName("subtotal");
            });

        }
    }
}

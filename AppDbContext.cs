using Microsoft.EntityFrameworkCore;
using WashUpAPIFix.Models;
using System.Linq;

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
            // Mapping tabel ke nama plural lowercase (sudah sesuai konvensi PostgreSQL)
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<LaundryOrder>().ToTable("laundryorders");
            modelBuilder.Entity<LaundryService>().ToTable("laundryservices");
            modelBuilder.Entity<OrderDetail>().ToTable("orderdetails");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<Rating>().ToTable("ratings");

            // ===================== USERS =====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.userid);
                entity.Property(e => e.userid).HasColumnName("userid");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.passwordHash).HasColumnName("passwordhash");
                entity.Property(e => e.role).HasColumnName("role");

                // Relasi ke LaundryOrder sebagai Customer dan Courier
                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Deliveries)
                    .WithOne(o => o.Courier)
                    .HasForeignKey(o => o.CourierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== LAUNDRY SERVICES =====================
            modelBuilder.Entity<LaundryService>(entity =>
            {
                entity.HasKey(e => e.LaundryServiceId);
                entity.Property(e => e.LaundryServiceId).HasColumnName("laundryserviceid");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnName("price");

                // Relasi ke OrderDetails
                entity.HasMany(e => e.OrderDetails)
                    .WithOne(od => od.LaundryService)
                    .HasForeignKey(od => od.LaundryServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== LAUNDRY ORDER =====================
            modelBuilder.Entity<LaundryOrder>(entity =>
            {
                entity.HasKey(e => e.LaundryOrderId);
                entity.Property(e => e.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.CourierId).HasColumnName("courierid");
                entity.Property(e => e.PickupAddress).HasColumnName("pickupaddress");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");

                // Relasi ke OrderDetails
                entity.HasMany(e => e.OrderDetails)
                    .WithOne(od => od.LaundryOrder)
                    .HasForeignKey(od => od.LaundryOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relasi ke Payment (1-1)
                entity.HasOne(e => e.Payment)
                    .WithOne(p => p.LaundryOrder)
                    .HasForeignKey<Payment>(p => p.LaundryOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relasi ke Rating (1-1)
                entity.HasOne(e => e.Rating)
                    .WithOne(r => r.LaundryOrder)
                    .HasForeignKey<Rating>(r => r.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== ORDER DETAIL =====================
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => new { od.LaundryOrderId, od.LaundryServiceId });
                entity.Property(od => od.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(od => od.LaundryServiceId).HasColumnName("laundryserviceid");
                entity.Property(od => od.Quantity).HasColumnName("quantity");
                entity.Property(od => od.Subtotal).HasColumnName("subtotal");
            });

            // ===================== PAYMENT =====================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);
                entity.Property(p => p.PaymentId).HasColumnName("paymentid");
                entity.Property(p => p.LaundryOrderId).HasColumnName("laundryorderid");
                entity.Property(p => p.Method).HasColumnName("method");
                entity.Property(p => p.Amount).HasColumnName("amount");
                entity.Property(p => p.PaidAt).HasColumnName("paidat");
                entity.Property(p => p.PaymentProofUrl).HasColumnName("paymentproofurl");
                entity.Property(p => p.Status).HasColumnName("status");
            });

            // ===================== RATING =====================
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.RatingId);
                entity.Property(r => r.RatingId).HasColumnName("ratingid");
                entity.Property(r => r.UserId).HasColumnName("userid");
                entity.Property(r => r.OrderId).HasColumnName("orderid");
                entity.Property(r => r.Score).HasColumnName("score");
                entity.Property(r => r.Comment).HasColumnName("comment");
                entity.Property(r => r.RatedAt).HasColumnName("ratedat");

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Optional: Pastikan semua kolom lowercase (redundan karena kita mapping manual)
            modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .ToList()
                .ForEach(p => p.SetColumnName(p.Name.ToLower()));
        }
    }
}

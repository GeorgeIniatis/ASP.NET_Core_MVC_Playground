using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ASP.NET_Core_MVC_Playground.Data
{
    public partial class DataDbContext : DbContext
    {
        public DataDbContext()
        {
        }

        public DataDbContext(DbContextOptions<DataDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Tiny> Tinies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Owner Model
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                    .HasColumnType("varchar(25)")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnType("varchar(25)")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(25)   
                    .HasComputedColumnSql("[FirstName]+ ' ' + [LastName]");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("int");

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);
            });

            // Item Model
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(25)")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .IsRequired();

                entity.Property(e => e.ImageBytes)
                    .HasColumnType("varbinary(MAX)");

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(250)")
                    .HasMaxLength(250)
                    .IsRequired();
            });

            // Buyer Model
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                    .HasColumnType("varchar(25)")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnType("varchar(25)")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasComputedColumnSql("[FirstName]+ ' ' + [LastName]");
                /*
                 * Scalar Function
                 * --Declare the return variable here
	               DECLARE @TotalOwned real

	               -- Add the T-SQL statements to compute the return value here
	               SELECT @TotalOwned = SUM(Price)
						                FROM dbo.Borrowers as B, dbo.Items as I
	                                    WHERE (B.Id = @BorrowerID) AND (B.Id = I.BorrowerID)

	               -- Return the result of the function
	               RETURN @TotalOwned
                 * 
                 * 
                 */
                entity.Property(e => e.TotalOwed)
                    .HasComputedColumnSql("dbo.CalculateTotalOwned([Id])");
            });

            // Tiny Model
            modelBuilder.Entity<Tiny>(entity =>
            {
                entity.HasKey(e => e.Page);
                
                entity.Property(e => e.TextArea)
                    .HasColumnType("varchar(2000)")
                    .HasMaxLength(2000)
                    .IsRequired();
            });

            // Relationships
            modelBuilder.Entity<Owner>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .HasOne<Owner>(i => i.Owner)
                .WithMany(io => io.ItemsOwned)
                .HasForeignKey(i => i.OwnerID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<Item>()
                .HasOne<Buyer>(i => i.Buyer)
                .WithMany(ib => ib.ItemsBorrowed)
                .HasForeignKey(i => i.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);     
        }
    }
}

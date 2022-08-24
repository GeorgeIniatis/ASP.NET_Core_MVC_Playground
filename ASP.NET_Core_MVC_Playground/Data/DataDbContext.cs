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
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Tiny> Tinies { get; set; }
        public DbSet<ShoppingBasket> ShoppingBaskets { get; set; }
        public DbSet<ShoppingBasketItem> ShoppingBasketItems { get; set; }
        public DbSet<BoughtItem> BoughtItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seller Model
            modelBuilder.Entity<Seller>(entity =>
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

                entity.Property(e => e.StripeImageUrl)
                    .HasColumnType("varchar(200)")
                    .HasMaxLength(200)
                    .IsRequired();

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
                 *  -- Declare the return variable here
	                DECLARE @TotalSpent real

	                -- Add the T-SQL statements to compute the return value here
	                SELECT @TotalSpent = SUM(Price)
						                 FROM dbo.Buyers as B, dbo.BoughtItems as BI, dbo.Items as I
	                                     WHERE (B.Id = @BuyerId) AND (B.Id = BI.BuyerId) AND (I.Id = BI.ItemId)

	                -- Return the result of the function
	                RETURN @TotalSpent
                 * 
                 * 
                 */
                entity.Property(e => e.TotalSpent)
                    .HasComputedColumnSql("dbo.CalculateTotalOwned([Id])");
            });

            // Shopping Basket Model
            modelBuilder.Entity<ShoppingBasket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();            
            });

            // Shopping Basket Item Model
            modelBuilder.Entity<ShoppingBasketItem>(entity =>
            {
                entity.HasKey(e => new { e.ShoppingBasketId, e.ItemId });
            });

            // Bought Item Model
            modelBuilder.Entity<BoughtItem>(entity =>
            {
                entity.HasKey(e => new { e.BuyerId, e.ItemId });
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
            modelBuilder.Entity<Seller>()
                .HasIndex(o => o.Email)
                .IsUnique();

            /* ONE Item has ONE Seller. That Seller can own MANY Items. 
             * The Foreign Key to the Item table is the SellerId
             * If a Seller is deleted any Items owned will also be deleted
             */
            modelBuilder.Entity<Item>()
                .HasOne<Seller>(i => i.Seller)
                .WithMany(io => io.ItemsOwned)
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            /* ONE Bought Item can have ONE Item. That Item can be in MANY Bought Items. 
             * The Foreign Key to the BoughtItem table is the ItemId
             */
            modelBuilder.Entity<BoughtItem>()
                .HasOne<Item>(i => i.Item)
                .WithMany(x => x.BoughtItems)
                .HasForeignKey(y => y.ItemId)
                .IsRequired();

            /* ONE Bought Item can have ONE Buyer. That Buyer can be in MANY Bought Items. 
             * The Foreign Key to the BoughtItem table is the BuyerId
             */
            modelBuilder.Entity<BoughtItem>()
                .HasOne<Buyer>(i => i.Buyer)
                .WithMany(x => x.BoughItems)
                .HasForeignKey(y => y.BuyerId)
                .IsRequired();

            /* ONE Shopping Basket has ONE Buyer
             * The Foreign Key to the Shopping Basket table is the BuyerId
             * If a Buyer is deleted the Shopping Basket will also be deleted
             */
            modelBuilder.Entity<ShoppingBasket>()
                .HasOne<Buyer>(i => i.Buyer)
                .WithOne(x => x.ShoppingBasket)
                .HasForeignKey<ShoppingBasket>(y => y.BuyerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            /* ONE Shopping Basket Item has one Item. That Item can be in MANY Shopping Basket entries
             * The Foreigh Key to the Shopping Basket Item table is the ItemId
             * If an Item is deleted the Shopping Basket Items entries associated with it will also be deleted
             */
            modelBuilder.Entity<ShoppingBasketItem>()
                .HasOne<Item>(i => i.Item)
                .WithMany(x => x.ShoppingBasketItems)
                .HasForeignKey(y => y.ItemId)
                .IsRequired();

            /* ONE Shopping Basket Item has one Shopping Basket. That Shopping Basket can be in MANY Shopping Basket entries
             * The Foreign Key to the Shopping Basket Item is the ShoppingBasketId
             * A Shopping Basket cannot be deleted unless all the associated Shopping Basket Entries have also been deleted
             */
            modelBuilder.Entity<ShoppingBasketItem>()
                .HasOne<ShoppingBasket>(i => i.ShoppingBasket)
                .WithMany(x => x.ShoppingBasketItems)
                .HasForeignKey(y => y.ShoppingBasketId)
                .IsRequired();

        }
    }
}

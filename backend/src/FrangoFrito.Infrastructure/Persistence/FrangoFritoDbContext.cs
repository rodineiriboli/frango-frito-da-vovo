using FrangoFrito.Domain.Common;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrangoFrito.Infrastructure.Persistence;

public sealed class FrangoFritoDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public FrangoFritoDbContext(DbContextOptions<FrangoFritoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);
        ConfigureCategories(builder.Entity<Category>());
        ConfigureProducts(builder.Entity<Product>());
        ConfigureCustomers(builder.Entity<Customer>());
        ConfigureOrders(builder.Entity<Order>());
        ConfigureOrderItems(builder.Entity<OrderItem>());
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(user => user.FullName).HasMaxLength(160).IsRequired();
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
    }

    private static void ConfigureCategories(EntityTypeBuilder<Category> entity)
    {
        ConfigureEntity(entity);
        entity.ToTable("categories");
        entity.Property(category => category.Name).HasMaxLength(120).IsRequired();
        entity.Property(category => category.Description).HasMaxLength(500);
        entity.HasIndex(category => category.Name).IsUnique();
    }

    private static void ConfigureProducts(EntityTypeBuilder<Product> entity)
    {
        ConfigureEntity(entity);
        entity.ToTable("products");
        entity.Property(product => product.Name).HasMaxLength(160).IsRequired();
        entity.Property(product => product.Description).HasMaxLength(1000);
        entity.Property(product => product.Price).HasPrecision(10, 2);
        entity.Property(product => product.ImageUrl).HasMaxLength(500);
        entity.HasIndex(product => product.Name);
        entity.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCustomers(EntityTypeBuilder<Customer> entity)
    {
        ConfigureEntity(entity);
        entity.ToTable("customers");
        entity.Property(customer => customer.Name).HasMaxLength(160).IsRequired();
        entity.Property(customer => customer.Phone).HasMaxLength(40).IsRequired();
        entity.HasIndex(customer => customer.Phone);

        entity.OwnsOne(customer => customer.Address, address =>
        {
            address.Property(item => item.Street).HasColumnName("street").HasMaxLength(180).IsRequired();
            address.Property(item => item.Number).HasColumnName("number").HasMaxLength(30).IsRequired();
            address.Property(item => item.Neighborhood).HasColumnName("neighborhood").HasMaxLength(120).IsRequired();
            address.Property(item => item.City).HasColumnName("city").HasMaxLength(120).IsRequired();
            address.Property(item => item.State).HasColumnName("state").HasMaxLength(2).IsRequired();
            address.Property(item => item.ZipCode).HasColumnName("zip_code").HasMaxLength(20).IsRequired();
            address.Property(item => item.Complement).HasColumnName("complement").HasMaxLength(160);
        });
    }

    private static void ConfigureOrders(EntityTypeBuilder<Order> entity)
    {
        ConfigureEntity(entity);
        entity.ToTable("orders");
        entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        entity.Ignore(order => order.Total);
        entity.Ignore(order => order.TotalItems);
        entity.HasIndex(order => order.Status);
        entity.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        entity.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureOrderItems(EntityTypeBuilder<OrderItem> entity)
    {
        ConfigureEntity(entity);
        entity.ToTable("order_items");
        entity.Property(item => item.ProductName).HasMaxLength(160).IsRequired();
        entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
        entity.Ignore(item => item.Total);
        entity.HasIndex(item => item.ProductId);
    }

    private static void ConfigureEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : Entity
    {
        entity.HasKey(item => item.Id);
        entity.Property(item => item.CreatedAt).IsRequired();
        entity.Property(item => item.UpdatedAt);
        entity.Property(item => item.Version).IsConcurrencyToken();
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkUpdated(now);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using wangluoanqan.Models;
using wangluoanqan.Models; // 注意你的项目名

namespace wangluoanqan.Data; // 注意你的项目名

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    // 明确指定使用你自己定义的 Transaction 类
    public DbSet<Models.Transaction> Transactions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(u => u.TotpSecret).HasMaxLength(50);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        // 这里也明确指定类型
        modelBuilder.Entity<Models.Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Signature).HasMaxLength(256);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.Timestamp);
            entity.Property(a => a.Hash).HasMaxLength(64);
        });
    }
}
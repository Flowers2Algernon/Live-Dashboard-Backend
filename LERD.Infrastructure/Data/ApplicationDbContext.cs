using Microsoft.EntityFrameworkCore;
using LERD.Domain.Entities;

namespace LERD.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Organisation entity configuration
            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.ToTable("organisations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.ContactPerson).HasColumnName("contact_person");
                entity.Property(e => e.ContactPhone).HasColumnName("contact_phone");
                entity.Property(e => e.Settings).HasColumnName("settings").HasColumnType("jsonb");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // Subscription entity configuration - 匹配实际数据库结构
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscriptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrganisationId).HasColumnName("organisation_id").IsRequired();
                entity.Property(e => e.PlanType).HasColumnName("plan_type");
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();
                entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.MaxSurveys).HasColumnName("max_surveys").HasDefaultValue(100);
                entity.Property(e => e.MaxUsers).HasColumnName("max_users").HasDefaultValue(100);
                entity.Property(e => e.Features).HasColumnName("features").HasColumnType("jsonb").HasDefaultValue("{}");
                entity.Property(e => e.BillingCycle).HasColumnName("billing_cycle").HasDefaultValue("monthly");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                
                // Foreign key relationship - 使用实际表的外键名称
                entity.HasOne(e => e.Organisation)
                    .WithMany()
                    .HasForeignKey(e => e.OrganisationId)
                    .HasConstraintName("subscriptions_organisation_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);
                
                // 不需要添加check constraints，因为数据库已经有了
                // 不需要添加索引，因为实际数据库可能已经有了
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
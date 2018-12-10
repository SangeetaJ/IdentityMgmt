using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Linq;

namespace BulkSMS.MarCom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Status> Status { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductPlan> ProductPlan { get; set; }
        public DbSet<UserWithProductsPlan> UserWithProductsPlan { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                .HasMany(x => x.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired(); // Neccessary to enable roles in Identity (only required in aspnet identity core 2.1 and above

            builder.Entity<ApplicationUser>().HasOne<Status>(s => s.Status).WithMany(u => u.AppUser).HasForeignKey(x => x.StatusId); // Application and Status relationship

            builder.Entity<Product>().HasOne<ApplicationUser>(p => p.CreatedUser).WithMany().HasForeignKey(p => p.CreatedBy).IsRequired(); // Application and Product relationship

            builder.Entity<ProductPlan>().HasOne<Product>(pp => pp.ProductId).WithMany(p => p.ProductPlans).HasForeignKey(p => p.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);// Product and Product plan relationship

            builder.Entity<ProductPlan>().HasOne<ApplicationUser>(pp => pp.CreatedUser).WithMany().HasForeignKey(p => p.CreatedBy).IsRequired();// Product and Product plan relationship

            builder.Entity<UserWithProductsPlan>().HasOne<ApplicationUser>(upp => upp.GiverUser).WithMany().HasForeignKey(upp => upp.GiverId).IsRequired().OnDelete(DeleteBehavior.Restrict); ; // UPP with Application user relationship

            builder.Entity<UserWithProductsPlan>().HasOne<ApplicationUser>(upp => upp.TakerUser).WithMany(u => u.OptedProductPlans).HasForeignKey(upp => upp.TakerId).OnDelete(DeleteBehavior.Restrict);// UPP with Application user relationship

            builder.Entity<UserWithProductsPlan>().HasOne<ApplicationUser>(upp => upp.CreatedUser).WithMany().HasForeignKey(upp => upp.CreatedBy).IsRequired(); // UPP with Application user relationship

            builder.Entity<UserWithProductsPlan>().HasOne<Product>(pp => pp.Product).WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);// Product and User with Product plan relationship

            builder.Entity<UserWithProductsPlan>().HasOne<ProductPlan>(pp => pp.ProductPlan).WithMany().HasForeignKey(p => p.ProductPlanId).OnDelete(DeleteBehavior.Restrict);// Product and User with Product plan relationship
        }
    }

    public class ApplicationUser : IdentityUser
    {
        [Column("FirstName")]
        public string FirstName { get; set; }

        [Column("LastName")]
        public string LastName { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Column("CreatedBy")]
        public string CreatedBy { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; } = new List<IdentityUserRole<string>>();

        public int? StatusId { get; set; }

        public virtual Status Status { get; set; } // foriegn key from Status table

        //public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<UserWithProductsPlan> OptedProductPlans { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }

        public string StatusType { get; set; }

        public ICollection<ApplicationUser> AppUser { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public virtual ApplicationUser CreatedUser { get; set; }

        public DateTime CreatedOn { get; set; }
        public ICollection<ProductPlan> ProductPlans { get; set; }
    }

    public class ProductPlan
    {
        public Product ProductId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public virtual ApplicationUser CreatedUser { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UserWithProductsPlan
    {
        public int Id { get; set; }
        public string GiverId { get; set; }
        public string TakerId { get; set; }
        public int ProductId { get; set; }
        public int ProductPlanId { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductPlan ProductPlan { get; set; }
        public int Balance { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ApplicationUser CreatedUser { get; set; }
        public ApplicationUser GiverUser { get; set; }
        public ApplicationUser TakerUser { get; set; }

    }
}

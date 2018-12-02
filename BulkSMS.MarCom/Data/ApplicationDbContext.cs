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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasMany(x => x.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });
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

        //[Column("StatusId")]
        //public virtual Status StatusId { get; set; }
    }
        //public class ApplicationRole : IdentityRole
        //{
        //    public ApplicationRole(string roleName)
        //       : base(roleName) { }
        //}


        public class Status
        {
            [Column("Id")]
            public int Id { get; set; }

            [Column("StatusType")]
            public string StatusType { get; set; }
        }
    }

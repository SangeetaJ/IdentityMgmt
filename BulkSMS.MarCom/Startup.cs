using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkSMS.MarCom.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;


namespace BulkSMS.MarCom
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();

            loggerFactory.AddNLog(); // add NLog to asp.net core
          

            SeedInitialUsersAsync(services).Wait(); // this creates sample use and roles
        }

        private async Task SeedInitialUsersAsync(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var users = new List<ApplicationUser>();

            //prepare initial user data
            var MasterAdminUser = new ApplicationUser
            {
                UserName = "jitendra777@gmail.com",
                Email = "jitendra777@gmail.com",
                FirstName = "Jitendra",
                LastName = "Kumar",
                Location = "Mumbai",
                CreatedDate = DateTime.Now,
                CreatedBy = null
            };

            var SuperAdminUser = new ApplicationUser
            {
                UserName = "exploreworld.welcome@gmail.com",
                Email = "exploreworld.welcome@gmail.com",
                FirstName = "Sangeeta",
                LastName = "Kori",
                Location = "Mumbai",
                CreatedDate = DateTime.Now,
                CreatedBy = "jitendra777@gmail.com"
            };

            var AdminUser = new ApplicationUser
            {
                UserName = "kavita@gmail.com",
                Email = "kavita@gmail.com",
                FirstName = "Kavita",
                LastName = "Kori",
                Location = "Mumbai",
                CreatedDate = DateTime.Now,
                CreatedBy = "jitendra777@gmail.com"
            };

            var ClientUser = new ApplicationUser
            {
                UserName = "babulal@gmail.com",
                Email = "babulal@gmail.com",
                FirstName = "Babulal",
                LastName = "Kori",
                Location = "Mumbai",
                CreatedDate = DateTime.Now,
                CreatedBy = "jitendra777@gmail.com"
            };

            users.Add(MasterAdminUser);
            users.Add(SuperAdminUser);
            users.Add(AdminUser);
            users.Add(ClientUser);

            //seed role into db
            string[] roleNames = { "MasterAdmin", "SuperAdmin", "Admin", "Client" };

            await SeedInitialRolesAsync(serviceProvider, roleNames);

            //seed users in db and append role chronologically
            foreach (var user in users)
            {
                int i = 0;
                var result = await UserManager.CreateAsync(user, "First@123");

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user, roleNames[i]);
                    i++;
                }
            }

            await SeedInitialUserStatusAsync(serviceProvider); // creates sample status
            await SeedInitialProductsWithPlanAsync(serviceProvider); // creates sample projects and plans
            await SeedInitialUserPlanAsync(serviceProvider); // maps user with prodcut plan and credits balance to the account
        }

        private async Task SeedInitialRolesAsync(IServiceProvider serviceProvider, string[] roleNames)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in roleNames)
            {
                var roleExist = RoleManager.RoleExistsAsync(roleName);
                if (!roleExist.IsCompletedSuccessfully)
                {
                    //create the roles and seed them to the database: Question 1  
                    await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task SeedInitialUserPlanAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName.Equals("jitendra777@gmail.com"));

            var productPromotion = await dbContext.Product.FirstOrDefaultAsync(x => x.Name.Equals("PromotionalSMS"));
            var productTransaction = await dbContext.Product.FirstOrDefaultAsync(x => x.Name.Equals("TransactionalSMS"));

            var planAnnually = await dbContext.ProductPlan.FirstOrDefaultAsync(x => x.Name.Equals("AnnualPackage"));
            var planBiAnnually = await dbContext.ProductPlan.FirstOrDefaultAsync(x => x.Name.Equals("BIAnnualPackage"));

            dbContext.Add<UserWithProductsPlan>(new UserWithProductsPlan()
            {
                GiverId = user.Id.ToString(),
                TakerId = user.Id.ToString(),
                ProductId = productPromotion.Id,
                ProductPlanId = planAnnually.Id,
                Balance = 1000000,
                Description = "row seeded through code",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

            dbContext.Add<UserWithProductsPlan>(new UserWithProductsPlan()
            {
                GiverId = user.Id.ToString(),
                TakerId = user.Id.ToString(),
                ProductId = productTransaction.Id,
                ProductPlanId = planBiAnnually.Id,
                Balance = 1000000,
                Description = "row seeded through code",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

           await dbContext.SaveChangesAsync();
        }


        private async Task SeedInitialUserStatusAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Add<Status>(new Status()
            {
                Name = "Active",
                Alias = "Active"
            });

            dbContext.Add<Status>(new Status()
            {
                Name = "Inactive",
                Alias = "Inactive"
            });

            dbContext.Add<Status>(new Status()
            {
                Name = "Suspended",
                Alias = "Suspended"
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task SeedInitialProductsWithPlanAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName.Equals("jitendra777@gmail.com"));

            var proTransactionSMS = dbContext.Add<Product>(new Product()
            {
                Name = "TransactionalSMS",
                Alias = "Transactional SMS",
                Description = "Send Transactions SMS to your subscribed clients.",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

            var proPromotionalSMS = dbContext.Add<Product>(new Product()
            {
                Name = "PromotionalSMS",
                Alias = "Promotional SMS",
                Description = "Send campaign promotion SMS to any number.",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

            var planAnnually = dbContext.Add<ProductPlan>(new ProductPlan()
            {
                Name = "AnnualPackage",
                Alias = "Annual Package",
                Description = "Plan for Annual subscribers",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

            var planBiAnnually = dbContext.Add<ProductPlan>(new ProductPlan()
            {
                Name = "BIAnnualPackage",
                Alias = "BI-Annual Package",
                Description = "Plan for 6 months subscribers",
                CreatedBy = user.Id.ToString(),
                CreatedOn = DateTime.Now
            });

           await dbContext.SaveChangesAsync();
        }
    }
}

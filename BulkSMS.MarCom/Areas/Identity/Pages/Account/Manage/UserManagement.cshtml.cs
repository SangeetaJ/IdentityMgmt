using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BulkSMS.MarCom.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BulkSMS.MarCom.Areas.Identity.Pages.Account
{
    public class UserMangementModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public UserMangementModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        //IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            //_emailSender = emailSender;
        }

        public string ReturnUrl { get; set; }

        [BindProperty]
        public List<ApplicationUserDto> Users { get; set; }


        public class ApplicationUserDto : ApplicationUser
        {
            public string Role { get; set; }

            public int TotalUsersCreated { get; set; }
            public int TotalAdminCreated { get; set; }
            public int TotalSuperAdminCreated { get; set; }
            public int TotalClientCreated { get; set; }


        }

        //public void OnGet(string returnUrl = null)
        //{
        //    ReturnUrl = returnUrl;
        //}

        public async Task<IActionResult> OnPostAsync(string userName, string password, string returnUrl = null)
        {
            returnUrl = "/Identity/Account/Manage";

            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            AuthenticationProperties props = null;
            await _signInManager.SignInAsync(user, props);
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Users = new List<ApplicationUserDto>();

            AddCreatedUsers(user.Email, Users);

            return Page();
        }

        public void AddCreatedUsers(string email, List<ApplicationUserDto> DtoUserList)
        {
            var query = fetchUserOnCreatedBy(email); // make sure you do not enter createdby same as username otherwise this function will never endg

            foreach (ApplicationUserDto u in query)
            {
                var creatorQuery = fetchUserOnCreatedBy(u.Email);

                CreatedUserCount(u, creatorQuery); // this will feed count of users created by this user

                DtoUserList.Add(u);

                AddCreatedUsers(u.Email, DtoUserList); // call same function again to search further users created by this user
            }

        }

        IQueryable<ApplicationUserDto> fetchUserOnCreatedBy(string email)
        {
            var query = from ro in _context.Roles
                        join uro in _context.UserRoles on ro.Id equals uro.RoleId
                        join u in _context.Users on uro.UserId equals u.Id
                        where u.CreatedBy.Equals(email)
                        select new ApplicationUserDto
                        {
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Email = u.Email,
                            CreatedBy = u.CreatedBy,
                            Location = u.Location,
                            CreatedDate = u.CreatedDate,
                            Role = ro.Name
                        };

            return query;
        }

        public void CreatedUserCount(ApplicationUserDto creator, IQueryable<ApplicationUserDto> userDtos)
        {
            var query = from u in userDtos
                        where u.CreatedBy.Equals(creator.Email)
                        select u;

            foreach (var i in query)
            {
                if (i.Role.Equals("SuperAdmin"))
                    creator.TotalSuperAdminCreated += 1;

                if (i.Role.Equals("Admin"))
                    creator.TotalAdminCreated += 1;

                if (i.Role.Equals("Client"))
                    creator.TotalClientCreated += 1;
            }

            creator.TotalUsersCreated = creator.TotalClientCreated + creator.TotalAdminCreated + creator.TotalSuperAdminCreated;
        }
    }
}

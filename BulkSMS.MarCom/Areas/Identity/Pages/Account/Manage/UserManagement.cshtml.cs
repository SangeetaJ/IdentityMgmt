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

            var userEntities = _context.Users.Where(x => x.CreatedBy.Equals(user.Email)).ToList();

            Users = new List<ApplicationUserDto>();

            AddCreatedUsers(user.Email, Users);

            //foreach (ApplicationUser u in userEntities)
            //{
            //    var UserDto = new ApplicationUserDto();

            //    UserDto.FirstName = u.FirstName;
            //    UserDto.LastName = u.LastName;
            //    UserDto.Email = u.Email;
            //    UserDto.CreatedBy = u.CreatedBy;
            //    UserDto.Location = u.Location;
            //    UserDto.CreatedDate = u.CreatedDate;
            //    UserDto.Role = _userManager.GetRolesAsync(u).ToString();
            //    Users.Add(UserDto);

            //    CreatedUserCount(UserDto.Email, UserDto); // this will feed count of users created by this user
            //}

            return Page();
        }

        public void AddCreatedUsers(string email, List<ApplicationUserDto> DtoUserList)
        {
            var userEntities = _context.Users.Where(x => x.CreatedBy.Equals(email)).ToList();

            foreach (ApplicationUser u in userEntities)
            {
                var UserDto = new ApplicationUserDto();

                UserDto.FirstName = u.FirstName;
                UserDto.LastName = u.LastName;
                UserDto.Email = u.Email;
                UserDto.CreatedBy = u.CreatedBy;
                UserDto.Location = u.Location;
                UserDto.CreatedDate = u.CreatedDate;
                UserDto.Role = _userManager.GetRolesAsync(u).ToString();
                // UserDto.Role = (await _userManager.GetRolesAsync(u)).FirstOrDefault();
                DtoUserList.Add(UserDto);

                CreatedUserCount(UserDto.Email, UserDto); // this will feed count of users created by this user

                AddCreatedUsers(UserDto.Email, DtoUserList); // call same function again to search further users created by this user
            }

        }

        public void CreatedUserCount(string email, ApplicationUserDto userDto)
        {
            //int total = 0;

            var query = from ro in _context.Roles
                        join uro in _context.UserRoles on ro.Id equals uro.RoleId
                        join u in _context.Users on uro.UserId equals u.Id
                        where u.CreatedBy.Equals(email)
                        group ro.Name by ro.Name into f
                        select new { RoleName = f.Key, Count = f.Count() };

            foreach (var i in query)
            {
                if (i.RoleName.Equals("SuperAdmin"))
                    userDto.TotalSuperAdminCreated = i.Count;

                if (i.RoleName.Equals("Admin"))
                    userDto.TotalAdminCreated = i.Count;

                if (i.RoleName.Equals("Client"))
                    userDto.TotalClientCreated = i.Count;
            }

            userDto.TotalUsersCreated = userDto.TotalClientCreated + userDto.TotalAdminCreated + userDto.TotalSuperAdminCreated;
            //userDto.TotalSuperAdminCreated = query.Count(x => x.RoleName.Equals("SuperAdmin"));
            //userDto.TotalAdminCreated = query.Count(x => x.RoleName.Equals("Admin"));
            //userDto.TotalClientCreated = query.Count(x => x.RoleName.Equals("Client"));
            //total = _context.Users.Where(x => x.CreatedBy.Equals(user.Email)).Count();
            //total = query.Count();
            //return total;
        }
    }
}

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
using System.Web;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace BulkSMS.MarCom.Areas.Identity.Pages.Account
{
    public class CreateCampignModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        public CampaignModel Campaign;
        public CreateCampignModel(
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
            Campaign = new CampaignModel();
            Campaign.Response = new SMSResponse();
            //Campaign.Response.errors = new Errors[]{
            //        new Errors(){message="", code="" }
            //};
            //Campaign.Response.warnings = new Warning[] {
            //    new Warning(){message="",numbers=""}
            //};
            //_emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string result { get; set; }

        public string ReturnUrl { get; set; }

        

        public class InputModel
        {
            [Display(Name = "Number")]
            public string Numbers { get; set; }

            [Display(Name = "Message")]
            public string Message { get; set; }
        }

        public class CampaignModel
        {
            public SMSResponse Response { get; set; }
        }

        public class SMSResponse
        {
            public Warning[] warnings { get; set; }
            public Errors[] errors { get; set; }
            public string status { get; set; }
        }

        public class Warning
        {
            public string message { get; set; }
            public string numbers { get; set; }
        }

        public class Errors
        {
            public string message { get; set; }
            public string code { get; set; }
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            string message = HttpUtility.UrlEncode(Input.Message);
            string apiKey = "nQXTSlAldOc-0mw4STVmjXaacDnoJJCBQgmbVG6GCz";

            using (var wb = new WebClient())
            {
                byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                {
                {"apikey" , apiKey},
                {"numbers" , Input.Numbers},
                {"message" , message},
                {"sender" , "TXTLCL"}
                });

                var json = System.Text.Encoding.UTF8.GetString(response);
                Campaign.Response= JsonConvert.DeserializeObject<SMSResponse>(json);

                return Page();
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
    }
}

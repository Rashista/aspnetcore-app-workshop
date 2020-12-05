using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FrontEnd.Middleware;
using FrontEnd.Pages.Models;
using FrontEnd.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontEnd
{
    [SkipWelcome]
    public class WelcomeModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public WelcomeModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public Attendee Attendee { get; set; }

        public IActionResult OnGet()
        {
            // Redirect to home page if user is anonymous or already registered as attendee
            var isAttendee = User.IsAttendee();

            if (!User.Identity.IsAuthenticated || isAttendee)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var success = await _apiClient.AddAttendeeAsync(this.Attendee);

            if (!success)
            {
                ModelState.AddModelError("", "There is an issue creating the attendee for this user.");
                return Page();
            }

            // Re-issue the auth cookie with the new IsAttendee claim
            User.MakeAttendee();
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, User);

            return RedirectToPage("/Index");
        }
    }
}
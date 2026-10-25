using AuthenticationModule.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationModule.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;
        public LoginModel(IAuthService auth) => _auth = auth;

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated) return RedirectToPage("/Index");

            SuccessMessage = TempData["SuccessMessage"]?.ToString()
                          ?? TempData["LogoutMessage"]?.ToString();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            { ErrorMessage = "Usuario y contraseña son obligatorios"; return Page(); }

            var result = await _auth.LoginAsync(Username, Password);
            if (result.Success) return RedirectToPage("/Index");

            ErrorMessage = result.Message;
            return Page();
        }
    }
}
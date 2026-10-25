using AuthenticationModule.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationModule.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        public RegisterModel(IAuthService authService) => _authService = authService;

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();

        public IActionResult OnGet() =>
            User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Page();
            }

            var result = await _authService.RegisterAsync(Username, Email, Password, ConfirmPassword);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Cuenta creada exitosamente. Ya puedes iniciar sesión.";
                return RedirectToPage("/Account/Login");
            }

            ErrorMessage = result.Message;
            Errors = result.Errors;
            return Page();
        }
    }
}
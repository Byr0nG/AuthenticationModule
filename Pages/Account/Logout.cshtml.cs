using AuthenticationModule.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationModule.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        public LogoutModel(IAuthService authService) => _authService = authService;

        // GET: vulnerable a CSRF, solo para casos excepcionales
        public async Task<IActionResult> OnGetAsync()
        {
            await _authService.LogoutAsync();
            TempData["LogoutMessage"] = "Has cerrado sesión correctamente";
            return RedirectToPage("/Account/Login");
        }

        // POST: recomendado (valida token anti-CSRF)
        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.LogoutAsync();
            TempData["LogoutMessage"] = "Has cerrado sesión correctamente";
            return RedirectToPage("/Account/Login");
        }
    }
}
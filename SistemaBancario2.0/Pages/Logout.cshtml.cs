using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaBancario2._0.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Limpiar toda la sesión
            HttpContext.Session.Clear();
            
            // Redirigir al inicio
            return RedirectToPage("/Index");
        }
    }
}

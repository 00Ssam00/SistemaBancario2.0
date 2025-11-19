using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaBancario2._0.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Limpiar TempData y toda la sesión para asegurarnos de cerrar la sesión correctamente
            TempData.Clear();
            HttpContext.Session.Clear();

            // Redirigir al login
            return RedirectToPage("/Loguin");
        }
    }
}


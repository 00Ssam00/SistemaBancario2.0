using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaBancario2._0.Pages
{
    public class LoguinModel : PageModel
    {
        private readonly Banco _banco;

        public LoguinModel(Banco banco)
        {
            _banco = banco;
        }

        [BindProperty]
        [Required(ErrorMessage = "El n�mero de cuenta es obligatorio")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El n�mero de cuenta debe tener 5 d�gitos")]
        public string NumeroCuenta { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La clave es obligatoria")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "La clave debe tener 4 d�gitos")]
        public string Clave { get; set; }

        [TempData]
        public string MensajeError { get; set; }

        [TempData]
        public string MensajeExito { get; set; }

        public int IntentosRestantes { get; set; } = 3;

        public void OnGet()
        {
            // Verificar si hay un mensaje de registro exitoso
            if (!string.IsNullOrEmpty(MensajeExito))
            {
                // Ya se muestra autom�ticamente con TempData
            }

            // Obtener intentos restantes de la sesi�n
            var intentos = HttpContext.Session.GetInt32("IntentosLogin");
            if (intentos.HasValue)
            {
                IntentosRestantes = intentos.Value;
            }
        }

        public IActionResult OnPost()
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Obtener intentos de la sesi�n
            var intentosKey = $"Intentos_{NumeroCuenta}";
            var intentos = HttpContext.Session.GetInt32(intentosKey) ?? 0;

            // Buscar usuario
            Usuario usuario = _banco.BuscarUsuarioPorCuenta(NumeroCuenta);

            if (usuario == null)
            {
                MensajeError = "N�mero de cuenta no encontrado.";
                return Page();
            }

            // Verificar si la cuenta est� bloqueada
            if (!usuario.Estado)
            {
                MensajeError = "Tu cuenta est� bloqueada por exceso de intentos fallidos. Contacta con soporte.";
                return Page();
            }

            // Validar clave
            if (usuario.Clave != Clave)
            {
                intentos++;
                HttpContext.Session.SetInt32(intentosKey, intentos);
                IntentosRestantes = 3 - intentos;

                if (intentos >= 3)
                {
                    // Bloquear usuario
                    usuario.Estado = false;
                    MensajeError = "Cuenta bloqueada por exceso de intentos fallidos. Contacta con soporte.";
                    HttpContext.Session.Remove(intentosKey);
                    return Page();
                }

                MensajeError = $"Clave incorrecta. Te quedan {IntentosRestantes} intento(s).";
                return Page();
            }

            // Login exitoso
            HttpContext.Session.Remove(intentosKey);
            HttpContext.Session.SetString("NumeroCuenta", usuario.CuentaBancaria.NumeroCuenta);
            HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);

            MensajeExito = $"�Bienvenido {usuario.Nombre}!";
            return RedirectToPage("/Transacciones");
        }
    }
}

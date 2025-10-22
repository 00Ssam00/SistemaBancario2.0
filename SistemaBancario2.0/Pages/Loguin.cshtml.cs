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
        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El número de cuenta debe tener 5 dígitos")]
        public string NumeroCuenta { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La clave es obligatoria")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "La clave debe tener 4 dígitos")]
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
                // Ya se muestra automáticamente con TempData
            }

            // Obtener intentos restantes de la sesión
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

            // Obtener intentos de la sesión
            var intentosKey = $"Intentos_{NumeroCuenta}";
            var intentos = HttpContext.Session.GetInt32(intentosKey) ?? 0;

            // Buscar usuario
            Usuario usuario = _banco.BuscarUsuarioPorCuenta(NumeroCuenta);

            if (usuario == null)
            {
                MensajeError = "Número de cuenta no encontrado.";
                return Page();
            }

            // Verificar si la cuenta está bloqueada
            if (!usuario.Estado)
            {
                MensajeError = "Tu cuenta está bloqueada por exceso de intentos fallidos. Contacta con soporte.";
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

            MensajeExito = $"¡Bienvenido {usuario.Nombre}!";
            return RedirectToPage("/Transacciones");
        }
    }
}

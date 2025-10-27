using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaBancario2._0.Pages
{
    public class LoguinModel : PageModel
    {
        private readonly Banco _banco;

        public LoguinModel()
        {
            _banco = Banco.Instancia;

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

            // Reiniciar los intentos visibles cada vez que se abre la página
            IntentosRestantes = 3;
        }


        public IActionResult OnPost()
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                return Page();
            }

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
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.Estado = false;
                    MensajeError = "Cuenta bloqueada por exceso de intentos fallidos. Contacta con soporte.";
                }
                else
                {
                    int intentosRestantes = 3 - usuario.IntentosFallidos;
                    MensajeError = $"Clave incorrecta. Te quedan {intentosRestantes} intento(s).";
                }

                return Page();
            }

            // Login exitoso
            usuario.IntentosFallidos = 0; // Reiniciar intentos
            MensajeExito = $"¡Bienvenido {usuario.Nombre}!";

            TempData["NumeroCuenta"] = usuario.CuentaBancaria.NumeroCuenta;
            TempData["NombreUsuario"] = usuario.Nombre;

            return RedirectToPage("/Transacciones");
        }

    }
}

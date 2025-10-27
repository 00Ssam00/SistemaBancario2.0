using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaBancario2._0.Pages
{
    public class PerfilModel(Banco banco) : PageModel
    {
        public Usuario UsuarioActual { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        public string Nombre { get; set; }

        [TempData]
        public string MensajeError { get; set; }

        [TempData]
        public string MensajeExito { get; set; }

        public IActionResult OnGet()
        {
            // Verificar si el usuario está logueado
            var numeroCuenta = HttpContext.Session.GetString("NumeroCuenta");
            if (string.IsNullOrEmpty(numeroCuenta))
            {
                return RedirectToPage("/Login");
            }

            // Buscar usuario
            UsuarioActual = banco.BuscarUsuarioPorCuenta(numeroCuenta);
            if (UsuarioActual == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            // Cargar datos del usuario
            Nombre = UsuarioActual.Nombre;

            return Page();
        }

        // Handler: Actualizar Perfil
        public IActionResult OnPostActualizarPerfil()
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Validar nombre
                if (string.IsNullOrWhiteSpace(Nombre) || Nombre.Length < 3)
                {
                    MensajeError = "El nombre debe tener al menos 3 caracteres.";
                    return RedirectToPage();
                }

                // Actualizar nombre
                UsuarioActual.Nombre = Nombre.Trim();

                // Actualizar sesión
                HttpContext.Session.SetString("NombreUsuario", UsuarioActual.Nombre);

                MensajeExito = "Perfil actualizado correctamente.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al actualizar el perfil: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Handler: Cambiar Clave
        public IActionResult OnPostCambiarClave(string claveActual, string nuevaClave, string confirmarClave)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            // Validaciones
            if (string.IsNullOrEmpty(claveActual) || string.IsNullOrEmpty(nuevaClave) || string.IsNullOrEmpty(confirmarClave))
            {
                MensajeError = "Todos los campos son obligatorios.";
                return RedirectToPage();
            }

            // Validar formato de la nueva clave
            if (nuevaClave.Length != 4 || !nuevaClave.All(char.IsDigit))
            {
                MensajeError = "La nueva clave debe tener exactamente 4 dígitos numéricos.";
                return RedirectToPage();
            }

            // Verificar clave actual
            if (claveActual != UsuarioActual.Clave)
            {
                MensajeError = "La clave actual es incorrecta.";
                return RedirectToPage();
            }

            // Verificar que la nueva clave sea diferente
            if (nuevaClave == UsuarioActual.Clave)
            {
                MensajeError = "La nueva clave debe ser diferente a la actual.";
                return RedirectToPage();
            }

            // Verificar confirmación
            if (nuevaClave != confirmarClave)
            {
                MensajeError = "Las claves no coinciden.";
                return RedirectToPage();
            }

            try
            {
                // Cambiar clave
                UsuarioActual.Clave = nuevaClave;
                MensajeExito = "Clave cambiada exitosamente.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cambiar la clave: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Método auxiliar para cargar el usuario actual
        private void CargarUsuarioActual()
        {
            var numeroCuenta = HttpContext.Session.GetString("NumeroCuenta");
            if (!string.IsNullOrEmpty(numeroCuenta))
            {
                UsuarioActual = banco.BuscarUsuarioPorCuenta(numeroCuenta);
            }
        }

        // Método para obtener el nombre del tipo de cuenta
        public string ObtenerTipoCuenta()
        {
            if (UsuarioActual?.CuentaBancaria == null)
                return "Desconocida";

            return UsuarioActual.CuentaBancaria switch
            {
                CuentaAhorro => "Cuenta de Ahorro",
                CuentaCorriente => "Cuenta Corriente",
                TarjetaCredito => "Tarjeta de Crédito",
                _ => "Cuenta Bancaria"
            };
        }
    }
}
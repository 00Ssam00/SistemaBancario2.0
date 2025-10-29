using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaBancario2._0.Pages
{
    public class PerfilModel : PageModel
    {
        private readonly Banco _banco;

        public PerfilModel()
        {
            _banco = Banco.Instancia; // Usa el banco compartido
        }

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
            // Obtener número de cuenta desde TempData (sin consumirlo)
            var numeroCuenta = TempData.Peek("NumeroCuenta") as string;

            if (string.IsNullOrEmpty(numeroCuenta))
                return RedirectToPage("/Loguin");

            // Buscar usuario
            UsuarioActual = _banco.BuscarUsuarioPorCuenta(numeroCuenta);
            if (UsuarioActual == null)
                return RedirectToPage("/Loguin");

            Nombre = UsuarioActual.Nombre;

            // Mantener TempData disponible
            TempData.Keep("NumeroCuenta");
            TempData.Keep("NombreUsuario");

            return Page();
        }

        // Handler: Actualizar Perfil
        public IActionResult OnPostActualizarPerfil()
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Loguin");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (string.IsNullOrWhiteSpace(Nombre) || Nombre.Length < 3)
                {
                    MensajeError = "El nombre debe tener al menos 3 caracteres.";
                    return RedirectToPage();
                }

                UsuarioActual.Nombre = Nombre.Trim();
                MensajeExito = "Perfil actualizado correctamente.";

                // Actualizar TempData con nuevo nombre
                TempData["NombreUsuario"] = UsuarioActual.Nombre;
                TempData.Keep("NumeroCuenta");
                TempData.Keep("NombreUsuario");
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
            if (UsuarioActual == null) return RedirectToPage("/Loguin");

            if (string.IsNullOrEmpty(claveActual) || string.IsNullOrEmpty(nuevaClave) || string.IsNullOrEmpty(confirmarClave))
            {
                MensajeError = "Todos los campos son obligatorios.";
                return RedirectToPage();
            }

            if (nuevaClave.Length != 4 || !nuevaClave.All(char.IsDigit))
            {
                MensajeError = "La nueva clave debe tener exactamente 4 dígitos numéricos.";
                return RedirectToPage();
            }

            if (claveActual != UsuarioActual.Clave)
            {
                MensajeError = "La clave actual es incorrecta.";
                return RedirectToPage();
            }

            if (nuevaClave == UsuarioActual.Clave)
            {
                MensajeError = "La nueva clave debe ser diferente a la actual.";
                return RedirectToPage();
            }

            if (nuevaClave != confirmarClave)
            {
                MensajeError = "Las claves no coinciden.";
                return RedirectToPage();
            }

            try
            {
                UsuarioActual.Clave = nuevaClave;
                MensajeExito = "Clave cambiada exitosamente.";
                TempData.Keep("NumeroCuenta");
                TempData.Keep("NombreUsuario");
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
            var numeroCuenta = TempData.Peek("NumeroCuenta") as string;

            if (!string.IsNullOrEmpty(numeroCuenta))
            {
                UsuarioActual = _banco.BuscarUsuarioPorCuenta(numeroCuenta);
                TempData.Keep("NumeroCuenta");
                TempData.Keep("NombreUsuario");
            }
        }

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

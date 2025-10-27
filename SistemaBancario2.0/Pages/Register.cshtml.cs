using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaBancario2._0.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly Banco _banco;

        public RegisterModel()
        {
            _banco = Banco.Instancia;

        }

        [BindProperty, Required]
        public string Nombre { get; set; }

        [BindProperty, Required, RegularExpression(@"^\d{5}$", ErrorMessage = "Debe tener 5 dígitos")]
        public string NumeroCuenta { get; set; }

        [BindProperty, Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Debe tener 4 dígitos")]
        public string Clave { get; set; }

        [BindProperty, Required]
        public string TipoCuenta { get; set; }

        [TempData]
        public string MensajeExito { get; set; }

        [TempData]
        public string MensajeError { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                MensajeError = "Por favor completa todos los campos correctamente.";
                return Page();
            }

            // Validar si la cuenta ya existe
            var existente = _banco.BuscarUsuarioPorCuenta(NumeroCuenta);
            if (existente != null)
            {
                MensajeError = "El número de cuenta ya está registrado.";
                return Page();
            }

            // Crear cuenta según el tipo elegido
            Cuenta nuevaCuenta;
            if (TipoCuenta == "Ahorros")
                nuevaCuenta = new CuentaAhorro(NumeroCuenta, 0);
            else
                nuevaCuenta = new CuentaCorriente(NumeroCuenta, 0);

            // Crear el usuario
            Usuario nuevoUsuario = new Usuario(Nombre, Clave, true, nuevaCuenta);

            // Registrar en el banco
            _banco.AgregarUsuario(nuevoUsuario);

            MensajeExito = "Usuario registrado exitosamente. Ahora puedes iniciar sesión.";
            return RedirectToPage("/Loguin");
        }
    }
}


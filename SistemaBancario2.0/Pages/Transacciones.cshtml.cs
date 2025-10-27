using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaBancario2._0.Models;

namespace SistemaBancario2._0.Pages
{
    public class TransaccionesModel : PageModel
    {
        private readonly Banco _banco;

        public TransaccionesModel(Banco banco)
        {
            _banco = banco;
        }

        public Usuario UsuarioActual { get; set; }

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
            UsuarioActual = _banco.BuscarUsuarioPorCuenta(numeroCuenta);
            if (UsuarioActual == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            return Page();
        }

        // Handler: Consignar
        public IActionResult OnPostConsignar(decimal monto)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (monto <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return RedirectToPage();
            }

            try
            {
                UsuarioActual.CuentaBancaria.ConsignarDirecto("Consignación en cajero", monto);
                MensajeExito = $"Consignación exitosa de ${monto:N0}. Nuevo saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al consignar: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Handler: Retirar
        public IActionResult OnPostRetirar(decimal monto)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (monto <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return RedirectToPage();
            }

            try
            {
                // Usar el método específico según el tipo de cuenta
                if (UsuarioActual.CuentaBancaria is CuentaAhorro cuentaAhorro)
                {
                    cuentaAhorro.RetirarDirecto("Retiro en cajero", monto);
                    MensajeExito = $"Retiro exitoso de ${monto:N0}. Se aplicaron intereses del 1.5%. Nuevo saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
                }
                else if (UsuarioActual.CuentaBancaria is CuentaCorriente cuentaCorriente)
                {
                    cuentaCorriente.RetirarDirecto("Retiro en cajero", monto);
                    if (UsuarioActual.CuentaBancaria.Saldo < 0)
                    {
                        MensajeExito = $"Retiro exitoso de ${monto:N0}. Se utilizó sobregiro. Saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
                    }
                    else
                    {
                        MensajeExito = $"Retiro exitoso de ${monto:N0}. Nuevo saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
                    }
                }
                else
                {
                    UsuarioActual.CuentaBancaria.RetirarDirecto("Retiro en cajero", monto);
                    MensajeExito = $"Retiro exitoso de ${monto:N0}. Nuevo saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al retirar: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Handler: Transferir
        public IActionResult OnPostTransferir(string cuentaDestino, decimal monto)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (string.IsNullOrEmpty(cuentaDestino) || cuentaDestino.Length != 5)
            {
                MensajeError = "El número de cuenta debe tener 5 dígitos.";
                return RedirectToPage();
            }

            if (monto <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return RedirectToPage();
            }

            if (cuentaDestino == UsuarioActual.CuentaBancaria.NumeroCuenta)
            {
                MensajeError = "No puedes transferir a tu misma cuenta.";
                return RedirectToPage();
            }

            try
            {
                Usuario destinatario = _banco.BuscarUsuarioPorCuenta(cuentaDestino);
                if (destinatario == null)
                {
                    MensajeError = "La cuenta destino no existe.";
                    return RedirectToPage();
                }

                if (UsuarioActual.CuentaBancaria.Saldo < monto)
                {
                    MensajeError = "Saldo insuficiente para realizar la transferencia.";
                    return RedirectToPage();
                }

                // Ejecutar transferencia
                var transferencia = new Transferencia(monto);
                bool exito = transferencia.Ejecutar(_banco, UsuarioActual, destinatario);

                if (exito)
                {
                    MensajeExito = $"Transferencia exitosa de ${monto:N0} a la cuenta {cuentaDestino}. Nuevo saldo: ${UsuarioActual.CuentaBancaria.Saldo:N0}";
                }
                else
                {
                    MensajeError = "No se pudo completar la transferencia.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al transferir: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Handler: Compra con Crédito
        public IActionResult OnPostCompraCredito(string descripcion, decimal monto, int cuotas)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (!(UsuarioActual.CuentaBancaria is TarjetaCredito tarjeta))
            {
                MensajeError = "Esta operación solo está disponible para tarjetas de crédito.";
                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(descripcion))
            {
                MensajeError = "Debes ingresar una descripción para la compra.";
                return RedirectToPage();
            }

            if (monto <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return RedirectToPage();
            }

            if (cuotas <= 0)
            {
                MensajeError = "El número de cuotas debe ser mayor a cero.";
                return RedirectToPage();
            }

            try
            {
                bool exito = tarjeta.RealizarCompra(monto, cuotas, descripcion);
                if (exito)
                {
                    MensajeExito = $"Compra realizada: {descripcion} por ${monto:N0} en {cuotas} cuota(s). Crédito disponible: ${tarjeta.CreditoDisponible:N0}";
                }
                else
                {
                    MensajeError = "No se pudo realizar la compra.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al realizar la compra: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Handler: Pagar Tarjeta de Crédito
        public IActionResult OnPostPagarCredito(decimal monto)
        {
            CargarUsuarioActual();
            if (UsuarioActual == null) return RedirectToPage("/Login");

            if (!(UsuarioActual.CuentaBancaria is TarjetaCredito tarjeta))
            {
                MensajeError = "Esta operación solo está disponible para tarjetas de crédito.";
                return RedirectToPage();
            }

            if (monto <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return RedirectToPage();
            }

            if (monto > tarjeta.DeudaTotal)
            {
                MensajeError = $"El monto excede la deuda total (${tarjeta.DeudaTotal:N0}).";
                return RedirectToPage();
            }

            try
            {
                tarjeta.RealizarPago(monto);
                MensajeExito = $"Pago realizado de ${monto:N0}. Deuda restante: ${tarjeta.DeudaTotal:N0}. Crédito disponible: ${tarjeta.CreditoDisponible:N0}";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al realizar el pago: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Método auxiliar para cargar el usuario actual
        private void CargarUsuarioActual()
        {
            var numeroCuenta = HttpContext.Session.GetString("NumeroCuenta");
            if (!string.IsNullOrEmpty(numeroCuenta))
            {
                UsuarioActual = _banco.BuscarUsuarioPorCuenta(numeroCuenta);
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
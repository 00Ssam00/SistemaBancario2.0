using System;

namespace SistemaBancario2._0.Models
{
    public class CuentaCorriente : Cuenta
    {
        private const decimal PorcentajeSobregiro = 0.20m; // 20%

        public CuentaCorriente(string numeroCuenta, decimal saldo = 0)
            : base(numeroCuenta, saldo)
        {
        }

        // Calcula el monto máximo disponible (saldo + sobregiro)
        public decimal ObtenerMontoDisponible()
        {
            decimal sobregiroPermitido = Saldo * PorcentajeSobregiro;
            return Saldo + sobregiroPermitido;
        }

        // Sobrescribimos el método de retiro para permitir sobregiro
        public new void RetirarDirecto(string destinoDetalle, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a retirar debe ser mayor a cero.");
                return;
            }

            decimal montoDisponible = ObtenerMontoDisponible();

            if (monto > montoDisponible)
            {
                Console.WriteLine($"Fondos insuficientes. Monto disponible (incluyendo sobregiro): ${montoDisponible:N0}");
                return;
            }

            // Realizar el retiro
            decimal saldoAnterior = Saldo;
            Saldo -= monto;

            // Verificar si se utilizó sobregiro
            if (Saldo < 0)
            {
                decimal montoSobregiro = Math.Abs(Saldo);
                Console.WriteLine($"Se utilizó sobregiro por: ${montoSobregiro:N0}");
                RegistrarMovimiento(new Movimiento("Retiro con sobregiro", -monto,
                    $"{destinoDetalle} - Sobregiro: ${montoSobregiro:N0}"));
            }
            else
            {
                RegistrarMovimiento(new Movimiento("Retiro realizado", -monto, destinoDetalle));
            }

            Console.WriteLine($"Se han retirado ${monto:N0}. Nuevo saldo: ${Saldo:N0}");
        }

        // Sobrescribir también el método Retirar con la firma antigua
        public new void Retirar(string numeroCuentaDestino, decimal monto)
        {
            RetirarDirecto($"Cuenta {numeroCuentaDestino}", monto);
        }

        // Método para consultar el sobregiro disponible
        public decimal ObtenerSobregiroDisponible()
        {
            if (Saldo >= 0)
            {
                return Saldo * PorcentajeSobregiro;
            }
            else
            {
                // Si ya está en sobregiro, calcular cuánto sobregiro queda
                decimal sobregiroTotal = Math.Abs(Saldo) / PorcentajeSobregiro * (1 + PorcentajeSobregiro);
                decimal sobregiroUsado = Math.Abs(Saldo);
                return Math.Max(0, sobregiroTotal - sobregiroUsado);
            }
        }
    }
}

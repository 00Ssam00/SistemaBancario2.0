using System;

namespace SistemaBancario2._0.Models
{
    public class CuentaAhorro : Cuenta
    {
        private const decimal TasaInteresMensual = 0.015m; // 1.5%

        public CuentaAhorro(string numeroCuenta, decimal saldo = 0)
            : base(numeroCuenta, saldo)
        {
        }

        // Sobrescribimos el método de retiro para aplicar intereses
        public new void RetirarDirecto(string destinoDetalle, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a retirar debe ser mayor a cero.");
                return;
            }

            // Calcular intereses ganados sobre el saldo actual antes del retiro
            decimal interesesGanados = Saldo * TasaInteresMensual;

            // Aplicar intereses al saldo
            Saldo += interesesGanados;
            Console.WriteLine($"Intereses aplicados (1.5%): ${interesesGanados:N0}");

            // Registrar movimiento de intereses
            if (interesesGanados > 0)
            {
                RegistrarMovimiento(new Movimiento("Intereses ganados", interesesGanados, "Interés mensual 1.5%"));
            }

            // Validar si hay fondos suficientes después de aplicar intereses
            if (monto > Saldo)
            {
                Console.WriteLine($"Fondos insuficientes. Saldo disponible: ${Saldo:N0}");
                return;
            }

            // Realizar el retiro
            Saldo -= monto;
            RegistrarMovimiento(new Movimiento("Retiro realizado", -monto, destinoDetalle));
            Console.WriteLine($"Se han retirado ${monto:N0}. Nuevo saldo: ${Saldo:N0}");
        }

        // Sobrescribir también el método Retirar con la firma antigua
        public new void Retirar(string numeroCuentaDestino, decimal monto)
        {
            RetirarDirecto($"Cuenta {numeroCuentaDestino}", monto);
        }
    }
}

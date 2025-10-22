using System;
using System.Collections.Generic;

namespace SistemaBancario2._0.Models
{
    public class Cuenta
    {
        public string NumeroCuenta { get; set; } // Número único de cuenta
        public decimal Saldo { get; set; } // Saldo actual de la cuenta
        public List<Movimiento> HistorialMovimientos { get; set; } // Historial de movimientos de la cuenta

        // Constructor de la clase Cuenta
        public Cuenta(string numeroCuenta, decimal saldo = 0)
        {
            NumeroCuenta = numeroCuenta;
            Saldo = saldo;
            HistorialMovimientos = new List<Movimiento>();
        }

        // Método para generar número único de cuenta
        public static string GenerarNumeroCuentaUnico(HashSet<string> numerosExistentes)
        {
            Random random = new Random();
            string numeroCuenta;

            do
            {
                numeroCuenta = random.Next(10000, 99999).ToString();
            } while (numerosExistentes.Contains(numeroCuenta));
            return numeroCuenta;
        }

        // Registrar movimiento (agrega un objeto Movimiento) - nueva firma
        public void RegistrarMovimiento(Movimiento movimiento)
        {
            if (movimiento == null) return;
            HistorialMovimientos.Add(movimiento);
        }

        // Overload para compatibilidad con llamadas antiguas: RegistrarMovimiento(detalle, tipo, monto)
        public void RegistrarMovimiento(string detalle, string tipoMovimiento, decimal monto)
        {
            var movimiento = new Movimiento(tipoMovimiento, monto, detalle);
            HistorialMovimientos.Add(movimiento);
        }

        // Métodos "rápidos" de ayuda que crean movimientos; en operaciones complejas preferir usar Transaccion
        public void ConsignarDirecto(string origenDetalle, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a consignar debe ser mayor a cero.");
                return;
            }

            Saldo += monto;
            RegistrarMovimiento(new Movimiento("Consignación recibida", monto, origenDetalle));
            Console.WriteLine($"Se han consignado {monto:C} a la cuenta {NumeroCuenta}. Nuevo saldo: {Saldo:C}");
        }

        public void RetirarDirecto(string destinoDetalle, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a retirar debe ser mayor a cero.");
                return;
            }
            if (monto > Saldo)
            {
                Console.WriteLine("Fondos insuficientes para realizar el retiro.");
                return;
            }
            Saldo -= monto;
            // Registramos como monto negativo para indicar salida de dinero
            RegistrarMovimiento(new Movimiento("Retiro realizado", -monto, destinoDetalle));
            Console.WriteLine($"Se han retirado {monto:C} de la cuenta {NumeroCuenta}. Nuevo saldo: {Saldo:C}");
        }

        // (Opcional) métodos antiguos que llamaban RegistrarMovimiento con 3 parámetros
        // Si tu proyecto usa Consignar() y Retirar() con esa firma, mantenlos:
        public void Consignar(string numeroCuentaOrigen, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a consignar debe ser mayor a cero.");
                return;
            }

            Saldo += monto;
            RegistrarMovimiento($"Cuenta {numeroCuentaOrigen}", "Consignación recibida", monto);
            Console.WriteLine($"Se han consignado {monto:C} a la cuenta {NumeroCuenta}. Nuevo saldo: {Saldo:C}");
        }

        public void Retirar(string numeroCuentaDestino, decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto a retirar debe ser mayor a cero.");
                return;
            }
            if (monto > Saldo)
            {
                Console.WriteLine("Fondos insuficientes para realizar el retiro.");
                return;
            }
            Saldo -= monto;
            RegistrarMovimiento($"Cuenta {numeroCuentaDestino}", "Retiro realizado", -monto);
            Console.WriteLine($"Se han retirado {monto:C} de la cuenta {NumeroCuenta}. Nuevo saldo: {Saldo:C}");
        }
    }
}

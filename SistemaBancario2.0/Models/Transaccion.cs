using System;

namespace SistemaBancario2._0.Models
{
    // Clase base para mostrar herencia y polimorfismo
    public abstract class Transaccion
    {
        public DateTime FechaHora { get; }
        public decimal Monto { get; }
        public string Tipo { get; protected set; }
        public string Detalle { get; protected set; }

        protected Transaccion(decimal monto, string detalle)
        {
            FechaHora = DateTime.Now;
            Monto = monto;
            Detalle = detalle;
        }

        // Implementación polimórfica: cada transacción sabe cómo ejecutarse
        public abstract bool Ejecutar(Banco banco, Usuario origen, Usuario destino);
        
        // Convierte la transacción a movimiento para registrar en historial
        public Movimiento ToMovimiento(decimal signoMonto, string detalle)
        {
            return new Movimiento(Tipo, signoMonto, detalle);
        }
    }

    public class Retiro : Transaccion
    {
        public Retiro(decimal monto, string detalle = "Retiro") : base(monto, detalle)
        {
            Tipo = "Retiro";
        }

        // origen es quien retira, destino puede ser null (cajero)
        public override bool Ejecutar(Banco banco, Usuario origen, Usuario destino)
        {
            if (origen == null)
            {
                Console.WriteLine("Cuenta origen inválida para retiro.");
                return false;
            }
            if (!origen.Estado)
            {
                Console.WriteLine("La cuenta está bloqueada; no se puede retirar.");
                return false;
            }
            if (Monto <= 0)
            {
                Console.WriteLine("El monto debe ser mayor a cero.");
                return false;
            }
            if (origen.CuentaBancaria.Saldo < Monto)
            {
                Console.WriteLine("Fondos insuficientes para realizar el retiro.");
                return false;
            }

            origen.CuentaBancaria.Saldo -= Monto;
            origen.CuentaBancaria.HistorialMovimientos.Add(ToMovimiento(-Monto, Detalle));
            Console.WriteLine($"Retiro de {Monto:C} realizado. Nuevo saldo: {origen.CuentaBancaria.Saldo:C}");
            return true;
        }
    }

    public class Consignacion : Transaccion
    {
        public Consignacion(decimal monto, string detalle = "Consignación") : base(monto, detalle)
        {
            Tipo = "Consignación";
        }

        // destino es la cuenta que recibe la consignación
        public override bool Ejecutar(Banco banco, Usuario origen, Usuario destino)
        {
            if (destino == null)
            {
                Console.WriteLine("Cuenta destino inválida para consignación.");
                return false;
            }
            if (!destino.Estado)
            {
                Console.WriteLine("La cuenta destino está bloqueada; no se puede consignar.");
                return false;
            }
            if (Monto <= 0)
            {
                Console.WriteLine("El monto debe ser mayor a cero.");
                return false;
            }

            // En consignación normalmente no se debita una cuenta origen (depósito externo) 
            // Si quieres que venga desde otra cuenta, usa Transferencia.
            destino.CuentaBancaria.Saldo += Monto;
            destino.CuentaBancaria.HistorialMovimientos.Add(ToMovimiento(Monto, Detalle));
            Console.WriteLine($"Consignación de {Monto:C} en cuenta {destino.CuentaBancaria.NumeroCuenta}. Nuevo saldo: {destino.CuentaBancaria.Saldo:C}");
            return true;
        }
    }

    public class Transferencia : Transaccion
    {
        public Transferencia(decimal monto, string detalle = "Transferencia") : base(monto, detalle)
        {
            Tipo = "Transferencia";
        }

        public override bool Ejecutar(Banco banco, Usuario origen, Usuario destino)
        {
            if (origen == null || destino == null)
            {
                Console.WriteLine("Cuentas inválidas para la transferencia.");
                return false;
            }
            if (!origen.Estado)
            {
                Console.WriteLine("La cuenta origen está bloqueada; no se puede transferir.");
                return false;
            }
            if (!destino.Estado)
            {
                Console.WriteLine("La cuenta destino está bloqueada; no se puede recibir la transferencia.");
                return false;
            }
            if (Monto <= 0)
            {
                Console.WriteLine("El monto debe ser mayor a cero.");
                return false;
            }
            if (origen.CuentaBancaria.Saldo < Monto)
            {
                Console.WriteLine("Fondos insuficientes en la cuenta origen.");
                return false;
            }
            if (origen.CuentaBancaria.NumeroCuenta == destino.CuentaBancaria.NumeroCuenta)
            {
                Console.WriteLine("No se puede transferir a la misma cuenta.");
                return false;
            }

            // Ejecutar: debitar origen, acreditar destino, registrar movimientos en ambos historiales
            origen.CuentaBancaria.Saldo -= Monto;
            destino.CuentaBancaria.Saldo += Monto;

            origen.CuentaBancaria.HistorialMovimientos.Add(ToMovimiento(-Monto, $"Transferencia a {destino.CuentaBancaria.NumeroCuenta}"));
            destino.CuentaBancaria.HistorialMovimientos.Add(ToMovimiento(Monto, $"Transferencia de {origen.CuentaBancaria.NumeroCuenta}"));

            Console.WriteLine($"Transferencia de {Monto:C} completa. Saldo origen: {origen.CuentaBancaria.Saldo:C}, Saldo destino: {destino.CuentaBancaria.Saldo:C}");
            return true;
        }
    }
}
using System;

namespace SistemaBancario2._0.Models
{
    public class Movimiento
    {
        public DateTime FechaHora { get; }
        public string Tipo { get; }
        public decimal Monto { get; }
        public string Detalle { get; }

        public Movimiento(string tipo, decimal monto, string detalle)
        {
            FechaHora = DateTime.Now;
            Tipo = tipo;
            Monto = monto;
            Detalle = detalle;
        }

        public override string ToString()
        {
            return $"{FechaHora:yyyy-MM-dd HH:mm:ss} | {Tipo} | {Monto:C} | {Detalle}";
        }
    }
}
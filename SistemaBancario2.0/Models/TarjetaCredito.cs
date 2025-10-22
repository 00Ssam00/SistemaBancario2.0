using System;

namespace SistemaBancario2._0.Models
{
    public class TarjetaCredito : Cuenta
    {
        public decimal LimiteCredito { get; set; }
        public decimal CreditoDisponible { get; private set; }
        public decimal DeudaTotal { get; private set; }

        public TarjetaCredito(string numeroCuenta, decimal limiteCredito)
            : base(numeroCuenta, 0) // Tarjeta de crédito empieza con saldo 0
        {
            LimiteCredito = limiteCredito;
            CreditoDisponible = limiteCredito;
            DeudaTotal = 0;
        }

        // Método para realizar una compra con cuotas
        public bool RealizarCompra(decimal monto, int numeroCuotas, string descripcion)
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto debe ser mayor a cero.");
                return false;
            }

            if (numeroCuotas <= 0)
            {
                Console.WriteLine("El número de cuotas debe ser mayor a cero.");
                return false;
            }

            if (monto > CreditoDisponible)
            {
                Console.WriteLine($"Crédito insuficiente. Tienes disponible: ${CreditoDisponible:N0}");
                return false;
            }

            // Calcular intereses según el número de cuotas
            decimal tasaInteresMensual = ObtenerTasaInteres(numeroCuotas);
            decimal interesTotal = 0;
            decimal pagoMensual = 0;

            if (tasaInteresMensual > 0)
            {
                // Fórmula de cuota fija con interés compuesto
                decimal factorInteres = (decimal)Math.Pow((double)(1 + tasaInteresMensual), numeroCuotas);
                pagoMensual = monto * (tasaInteresMensual * factorInteres) / (factorInteres - 1);
                interesTotal = (pagoMensual * numeroCuotas) - monto;
            }
            else
            {
                // Sin intereses
                pagoMensual = monto / numeroCuotas;
            }

            // Actualizar crédito disponible y deuda
            CreditoDisponible -= monto;
            DeudaTotal += monto + interesTotal;

            // Registrar movimiento
            string detalleMovimiento = $"{descripcion} - {numeroCuotas} cuotas";
            if (tasaInteresMensual > 0)
            {
                detalleMovimiento += $" - Interés: {tasaInteresMensual:P2}";
            }
            else
            {
                detalleMovimiento += " - Sin interés";
            }

            RegistrarMovimiento(new Movimiento("Compra con tarjeta", -monto, detalleMovimiento));

            // Mostrar información de la compra
            Console.WriteLine("\n----- RESUMEN DE COMPRA -----");
            Console.WriteLine($"Monto de compra: ${monto:N0}");
            Console.WriteLine($"Número de cuotas: {numeroCuotas}");

            if (tasaInteresMensual > 0)
            {
                Console.WriteLine($"Tasa de interés mensual: {tasaInteresMensual:P2}");
                Console.WriteLine($"Interés total: ${interesTotal:N0}");
                Console.WriteLine($"Total a pagar: ${(monto + interesTotal):N0}");
            }
            else
            {
                Console.WriteLine("Sin intereses");
            }

            Console.WriteLine($"Pago mensual: ${pagoMensual:N0}");
            Console.WriteLine($"Crédito disponible: ${CreditoDisponible:N0}");
            Console.WriteLine($"Deuda total: ${DeudaTotal:N0}");
            Console.WriteLine("----------------------------\n");

            return true;
        }

        // Determina la tasa de interés según el número de cuotas
        private decimal ObtenerTasaInteres(int numeroCuotas)
        {
            if (numeroCuotas <= 2)
            {
                return 0m; // Sin interés
            }
            else if (numeroCuotas <= 6)
            {
                return 0.019m; // 1.9%
            }
            else // 7 o más cuotas
            {
                return 0.023m; // 2.3%
            }
        }

        // Método para realizar un pago a la tarjeta
        public void RealizarPago(decimal monto, string descripcion = "Pago a tarjeta")
        {
            if (monto <= 0)
            {
                Console.WriteLine("El monto debe ser mayor a cero.");
                return;
            }

            if (monto > DeudaTotal)
            {
                Console.WriteLine($"El monto excede la deuda total. Tu deuda actual es de: ${DeudaTotal:N0}");
                return;
            }

            // Actualizar deuda y crédito disponible
            DeudaTotal -= monto;
            CreditoDisponible += monto;

            // Registrar movimiento
            RegistrarMovimiento(new Movimiento("Pago recibido", monto, descripcion));

            Console.WriteLine($"Pago de ${monto:N0} aplicado correctamente.");
            Console.WriteLine($"Deuda restante: ${DeudaTotal:N0}");
            Console.WriteLine($"Crédito disponible: ${CreditoDisponible:N0}");
        }

        // Sobrescribir métodos heredados para que no se usen (tarjeta no permite retiros directos)
        public new void RetirarDirecto(string destinoDetalle, decimal monto)
        {
            Console.WriteLine("Las tarjetas de crédito no permiten retiros directos. Use 'RealizarCompra' en su lugar.");
        }

        public new void ConsignarDirecto(string origenDetalle, decimal monto)
        {
            // En tarjeta de crédito, "consignar" es equivalente a pagar
            RealizarPago(monto, origenDetalle);
        }
    }
}

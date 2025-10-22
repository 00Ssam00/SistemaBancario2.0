using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SistemaBancario2._0.Models
{
    public class Usuario
    {
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public bool Estado { get; set; } // True -> Activo, False -> Bloqueado
        public Cuenta CuentaBancaria { get; set; } // Usuario tiene una cuenta bancaria

        // Constructor de la clase Usuario
        public Usuario(string nombre, string clave, bool estado, Cuenta cuenta)
        {
            Nombre = nombre;
            Clave = clave;
            Estado = estado;
            CuentaBancaria = cuenta;
        }

        // Método registrar -> devuelve un objeto Usuario con los datos ingresados y validados
        public static Usuario Registrar(HashSet<string> numerosExistentes)
        {
            string nombre;
            string clave;
            decimal saldoInicial = 0;

            Console.Write("Ingrese su nombre: ");
            nombre = Console.ReadLine();

            while (string.IsNullOrEmpty(nombre) ||
                   nombre.Trim().Length <= 2 ||
                   nombre.Any(ch => !char.IsLetter(ch)) ||
                   nombre.StartsWith(" "))
            {
                Console.Clear();
                Console.WriteLine("El nombre no puede estar vacío, contener números o espacios al inicio, caracteres, ni tener menos de 3 caracteres.");
                Thread.Sleep(2000);
                Console.Clear();
                Console.Write("Ingrese su nombre nuevamente: ");
                nombre = Console.ReadLine();
            }

            Console.Write("Ingrese su clave de 4 dígitos: ");
            clave = Console.ReadLine();

            // Validaciones de la clave
            while (string.IsNullOrEmpty(clave) || clave.Length != 4 ||
                   clave.Any(ch => !char.IsDigit(ch)))
            {
                Console.Clear();
                Console.WriteLine("La clave debe ser de 4 dígitos numéricos.");
                Thread.Sleep(2000);
                Console.Clear();
                Console.Write("Ingrese su clave nuevamente: ");
                clave = Console.ReadLine();
            }

            // Solicitar saldo inicial
            Console.Write("¿Desea ingresar un saldo inicial? (s/n): ");
            string respuestaSaldo = Console.ReadLine()?.ToLower();

            if (respuestaSaldo == "s")
            {
                bool saldoValido = false;
                while (!saldoValido)
                {
                    Console.Write("Ingrese el saldo inicial (mínimo $0): ");
                    if (decimal.TryParse(Console.ReadLine(), out saldoInicial) && saldoInicial >= 0)
                    {
                        saldoValido = true;
                    }
                    else
                    {
                        Console.WriteLine("El saldo debe ser un número mayor o igual a cero.");
                    }
                }
            }

            // Crear numero de cuenta unico
            string numeroCuenta = Cuenta.GenerarNumeroCuentaUnico(numerosExistentes);
            numerosExistentes.Add(numeroCuenta);

            // Crear cuenta con saldo inicial
            Cuenta nuevaCuenta = new Cuenta(numeroCuenta, saldoInicial);

            // Registrar primer movimiento si hay saldo inicial
            if (saldoInicial > 0)
            {
                nuevaCuenta.RegistrarMovimiento(new Movimiento("Saldo inicial", saldoInicial, "Sistema"));
            }

            Console.WriteLine("Usuario registrado con éxito.");
            Console.WriteLine($"{nombre} Su numero de cuenta es: {numeroCuenta}");
            if (saldoInicial > 0)
            {
                Console.WriteLine($"Saldo inicial: {saldoInicial:C}");
            }

            return new Usuario(nombre, clave, true, nuevaCuenta);
        }

        public bool IniciarSesion(string clave)
        {
            int intentos = 0;
            while (clave != Clave && intentos < 2) // 2 porque después se pedirá una vez más dentro del if
            {
                Console.Clear();
                Console.WriteLine($"Clave incorrecta. Intentos restantes: {2 - intentos}");
                Console.Write("Ingrese su clave nuevamente: ");
                clave = Console.ReadLine();
                intentos++;
            }

            if (clave != Clave)
            {
                Estado = false;
                Console.WriteLine("Usuario bloqueado por exceso de intentos.");
                return false;
            }

            if (Estado)
            {
                Console.WriteLine("Inicio de sesión exitoso.");
                return true;
            }

            Console.WriteLine("El usuario está bloqueado.");
            return false;
        }
    }
}

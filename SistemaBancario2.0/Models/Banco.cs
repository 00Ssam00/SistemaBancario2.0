using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SistemaBancario2._0.Models
{
    public class Banco
    {
        public string Nombre { get; set; }
        public List<Usuario> Usuarios { get; } = new List<Usuario>();
        private HashSet<string> numerosExistentes = new HashSet<string>();

        public Banco(string nombre)
        {
            Nombre = nombre;
        }

        public void AgregarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                Console.WriteLine("No se puede agregar un usuario nulo.");
                return;
            }

            if (Usuarios.Any(u => u.CuentaBancaria.NumeroCuenta == usuario.CuentaBancaria.NumeroCuenta))
            {
                Console.WriteLine($"Ya existe un usuario con la cuenta #{usuario.CuentaBancaria.NumeroCuenta}.");
                return;
            }


            Usuarios.Add(usuario);
            numerosExistentes.Add(usuario.CuentaBancaria.NumeroCuenta);
            Console.WriteLine($"Usuario {usuario.Nombre} agregado al banco {Nombre}.");
        }

        public Usuario? BuscarUsuarioPorCuenta(string numeroCuenta)
        {
            return Usuarios.FirstOrDefault(u => u.CuentaBancaria.NumeroCuenta == numeroCuenta);
        }

        // Transferencia: usa la clase Transferencia (polimorfismo)
        public void Transferir(string cuentaOrigen, string cuentaDestino, decimal monto)
        {
            Usuario? origen = BuscarUsuarioPorCuenta(cuentaOrigen);
            Usuario? destino = BuscarUsuarioPorCuenta(cuentaDestino);

            var tran = new Transferencia(monto);
            bool exito = tran.Ejecutar(this, origen, destino);
            if (exito)
            {
                // ya registra movimientos internamente en la ejecución
            }
        }

        // Consignación "interactiva": solicita destino y usa Transferencia si es entre cuentas,
        // o Consignacion si es depósito externo (aquí permitimos transferencia entre cuentas)
        public void ConsignarACuenta(string numeroCuentaOrigen, decimal monto)
        {
            while (true)
            {
                Console.Write("Ingrese la cuenta a la que desea consignar (o 'cancelar' para salir): ");
                string? numeroCuentaDestino = Console.ReadLine();
                if (numeroCuentaDestino?.ToLower() == "cancelar")
                {
                    Console.WriteLine("Operación cancelada.");
                    return;
                }

                if (string.IsNullOrEmpty(numeroCuentaDestino) || numeroCuentaDestino.Length != 5 ||
                    numeroCuentaDestino.Any(ch => !char.IsDigit(ch)))
                {
                    Console.WriteLine("El número de cuenta debe ser de 5 dígitos numéricos.");
                    Console.WriteLine("Intente nuevamente o escriba 'cancelar' para salir.\n");
                    continue;
                }

                Usuario? userOrigen = BuscarUsuarioPorCuenta(numeroCuentaOrigen);
                if (userOrigen == null)
                {
                    Console.WriteLine("Error: Cuenta origen no encontrada.");
                    return;
                }
                // si la cuenta de origen es distinta a la destino, validar saldo < monto, si no es, no se valida
                if (numeroCuentaOrigen != numeroCuentaDestino && userOrigen.CuentaBancaria.Saldo < monto) {
                    Console.WriteLine($"Saldo insuficiente. Su saldo actual es: {userOrigen.CuentaBancaria.Saldo:C}");
                    Console.WriteLine("¿Desea intentar con otro monto? (s/n): ");
                    string? respuesta = Console.ReadLine()?.ToLower();
                    if (respuesta == "s")
                    {
                        Console.Write("Ingrese el nuevo monto: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal nuevoMonto) && nuevoMonto > 0)
                        {
                            monto = nuevoMonto;
                            continue;
                        }
                    }
                    return;
                }
                if (numeroCuentaOrigen == numeroCuentaDestino)
                {
                    // Permitir consignar a la misma cuenta (depósito externo)
                    userOrigen.CuentaBancaria.ConsignarDirecto("Consignación externa", monto);
                    return;
                }

                Usuario? userDestino = BuscarUsuarioPorCuenta(numeroCuentaDestino);
                if (userDestino == null)
                {
                    Console.WriteLine("Cuenta destino no encontrada.");
                    Console.WriteLine("Intente nuevamente o escriba 'cancelar' para salir.\n");
                    continue;
                }

                // Ejecutar transferencia polimórfica:
                Transferir(numeroCuentaOrigen, numeroCuentaDestino, monto);
                return;
            }
        }

        public void MostrarUsuarios()
        {
            if (Usuarios.Count == 0)
            {
                Console.WriteLine("No hay usuarios registrados en el banco.");
                return;
            }

            foreach (var u in Usuarios)
            {
                Console.WriteLine($"Usuario: {u.Nombre} | Cuenta: {u.CuentaBancaria.NumeroCuenta} | Saldo: {u.CuentaBancaria.Saldo:C}");
            }
        }

        public void RegistrarUsuarioInteractivo()
        {
            // Crear usuario de prueba solo si no existe
            if (!numerosExistentes.Contains("12345"))
            {
                string numeroCuentaPrueba = "12345";
                numerosExistentes.Add(numeroCuentaPrueba);
                Cuenta cuentaPrueba = new Cuenta(numeroCuentaPrueba, 1000000);
                Usuario usuarioPrueba = new Usuario("UsuarioPrueba", "1234", true, cuentaPrueba);
                Usuarios.Add(usuarioPrueba);
                Console.WriteLine("Usuario de prueba creado - Cuenta: 12345, Clave: 1234, Saldo: $1,000,000");
            }

            Usuario nuevo = Usuario.Registrar(numerosExistentes);
            Usuarios.Add(nuevo);
        }

        public Usuario? IniciarSesion()
        {
            Console.Write("Ingrese su número de cuenta: ");
            string? numeroCuenta = Console.ReadLine();

            while (string.IsNullOrEmpty(numeroCuenta) || numeroCuenta.Length != 5 ||
                   numeroCuenta.Any(ch => !char.IsDigit(ch)))
            {
                Console.Clear();
                Console.WriteLine("El número de cuenta debe ser de 5 dígitos numéricos.");
                Thread.Sleep(2000);
                Console.Clear();
                Console.Write("Ingrese su número de cuenta nuevamente: ");
                numeroCuenta = Console.ReadLine();
            }

            Usuario? user = Usuarios.FirstOrDefault(u => u.CuentaBancaria.NumeroCuenta == numeroCuenta);
            if (user == null)
            {
                Console.WriteLine("Número de cuenta no encontrado.");
                return null;
            }

            Console.Write("Ingrese su clave: ");
            string? clave = Console.ReadLine();
            if (user.IniciarSesion(clave!))
            {
                return user;
            }
            return null;
        }

        // Cambiar clave y bloquear usuario se pueden mantener como métodos aquí (si quieres)
        public void CambiarClaveInteractivo()
        {
            Console.Write("INGRESE SU NÚMERO DE CUENTA: ");
            string numeroCuenta = Console.ReadLine();
            Usuario? user = BuscarUsuarioPorCuenta(numeroCuenta);
            if (user == null)
            {
                Console.WriteLine("Número de cuenta no encontrado.");
                return;
            }

            if (!user.Estado)
            {
                Console.WriteLine("El usuario está bloqueado. No se puede cambiar la clave. \n");
                return;
            }

            Console.Write("Ingrese su clave actual: ");
            string claveActual = Console.ReadLine();
            if (claveActual != user.Clave)
            {
                Console.WriteLine("Clave incorrecta.");
                return;
            }

            bool claveValida = false;
            while (!claveValida)
            {
                Console.Write("Ingrese su nueva clave de 4 dígitos: ");
                string nuevaClave = Console.ReadLine();

                if (string.IsNullOrEmpty(nuevaClave) || nuevaClave.Length != 4 ||
                    nuevaClave.Any(ch => !char.IsDigit(ch)))
                {
                    Console.WriteLine("La clave debe ser de 4 dígitos numéricos.");
                    continue;
                }

                if (nuevaClave == user.Clave)
                {
                    Console.WriteLine("\nLa nueva clave no puede ser igual a la actual.\n");
                    continue;
                }

                Console.Write("Ingrese su nueva clave otra vez: ");
                string confirmacion = Console.ReadLine();

                if (nuevaClave == confirmacion)
                {
                    user.Clave = nuevaClave;
                    Console.WriteLine("CLAVE CAMBIADA CON ÉXITO.");
                    claveValida = true;
                }
                else
                {
                    Console.WriteLine("Las claves no coinciden. Intente nuevamente.");
                }
            }
        }
    }
}



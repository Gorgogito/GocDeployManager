using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GocDeployManager.Notifications.Tests
{
    /// <summary>
    /// Servidor SMTP mínimo sobre un socket TCP real de loopback — no es un
    /// mock de <c>SmtpClient</c>, es un servidor real que habla el protocolo
    /// SMTP lo suficiente para que <c>SmtpClient</c> complete un envío
    /// (mismo criterio del proyecto de preferir infraestructura real sobre
    /// mocks siempre que sea práctico).
    /// </summary>
    internal sealed class ServidorSmtpDePrueba : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Thread _hilo;

        public int Puerto { get; }
        public string UltimoMensajeRecibido { get; private set; }

        public ServidorSmtpDePrueba()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Puerto = ((IPEndPoint)_listener.LocalEndpoint).Port;

            _hilo = new Thread(AtenderConexion) { IsBackground = true };
            _hilo.Start();
        }

        private void AtenderConexion()
        {
            try
            {
                using (var cliente = _listener.AcceptTcpClient())
                using (var stream = cliente.GetStream())
                using (var lector = new System.IO.StreamReader(stream, Encoding.ASCII))
                using (var escritor = new System.IO.StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\r\n" })
                {
                    escritor.WriteLine("220 localhost servidor SMTP de prueba");

                    string linea;
                    var enDatos = false;
                    var cuerpo = new StringBuilder();

                    while ((linea = lector.ReadLine()) != null)
                    {
                        if (enDatos)
                        {
                            if (linea == ".")
                            {
                                enDatos = false;
                                UltimoMensajeRecibido = cuerpo.ToString();
                                escritor.WriteLine("250 OK: mensaje recibido");
                                continue;
                            }
                            cuerpo.AppendLine(linea);
                            continue;
                        }

                        if (linea.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                        {
                            escritor.WriteLine("354 Comience el mensaje, termine con .");
                            enDatos = true;
                        }
                        else if (linea.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                        {
                            escritor.WriteLine("221 Adiós");
                            break;
                        }
                        else
                        {
                            escritor.WriteLine("250 OK");
                        }
                    }
                }
            }
            catch
            {
                // El listener se detuvo al llamar Dispose() — no hay nada que reportar.
            }
        }

        public void Dispose()
        {
            _listener.Stop();
        }
    }
}

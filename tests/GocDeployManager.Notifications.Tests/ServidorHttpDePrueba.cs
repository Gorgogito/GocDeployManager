using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GocDeployManager.Notifications.Tests
{
    /// <summary>
    /// Servidor HTTP mínimo sobre un socket TCP real de loopback (no
    /// <see cref="HttpListener"/>, que en algunos entornos exige permisos
    /// elevados o una reserva de URL ACL) — suficiente para que
    /// <see cref="System.Net.Http.HttpClient"/> complete un <c>POST</c> real
    /// contra él, como un Incoming Webhook de Teams.
    /// </summary>
    internal sealed class ServidorHttpDePrueba : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Thread _hilo;

        public int Puerto { get; }
        public string UltimoCuerpoRecibido { get; private set; }
        public int CodigoRespuesta { get; set; } = 200;

        public ServidorHttpDePrueba()
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
                {
                    var longitudContenido = 0;
                    string linea;

                    using (var lector = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true))
                    {
                        while (!string.IsNullOrEmpty(linea = lector.ReadLine()))
                        {
                            if (linea.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                                longitudContenido = int.Parse(linea.Substring("Content-Length:".Length).Trim());
                        }

                        var buffer = new char[longitudContenido];
                        var leidos = 0;
                        while (leidos < longitudContenido)
                        {
                            var leidoAhora = lector.Read(buffer, leidos, longitudContenido - leidos);
                            if (leidoAhora == 0)
                                break;
                            leidos += leidoAhora;
                        }
                        UltimoCuerpoRecibido = new string(buffer, 0, leidos);
                    }

                    var respuesta = Encoding.ASCII.GetBytes(
                        $"HTTP/1.1 {CodigoRespuesta} OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                    stream.Write(respuesta, 0, respuesta.Length);
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

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Infrastructure.SqlServer
{
    /// <summary>
    /// Persistencia de ambientes en las tablas Ambiente/AmbienteSistema —
    /// reemplaza a JsonAmbienteRepository porque estos datos deben ser
    /// iguales para todos los usuarios, no una preferencia por laptop.
    /// </summary>
    public sealed class SqlServerAmbienteRepository : IAmbienteRepository
    {
        private readonly string _cadenaConexion;

        public SqlServerAmbienteRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
            SqlServerEsquema.Verificar(_cadenaConexion);
        }

        public IReadOnlyList<Ambiente> ObtenerTodos()
        {
            var ordenAmbientes = new List<string>();
            var notificacionesPorAmbiente = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var sistemasPorAmbiente = new Dictionary<string, List<AmbienteSistema>>(StringComparer.OrdinalIgnoreCase);

            using (var conexion = AbrirConexion())
            {
                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = "SELECT Nombre, NotificacionesHabilitadas FROM Ambiente ORDER BY Nombre";
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var nombre = (string)lector["Nombre"];
                            ordenAmbientes.Add(nombre);
                            notificacionesPorAmbiente[nombre] = (bool)lector["NotificacionesHabilitadas"];
                            sistemasPorAmbiente[nombre] = new List<AmbienteSistema>();
                        }
                    }
                }

                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = "SELECT AmbienteNombre, SistemaCodigo, SistemaNombre, RutaDestino FROM AmbienteSistema ORDER BY Id";
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var ambienteNombre = (string)lector["AmbienteNombre"];
                            if (!sistemasPorAmbiente.TryGetValue(ambienteNombre, out var lista))
                                continue;

                            var sistema = new Sistema((string)lector["SistemaCodigo"], (string)lector["SistemaNombre"]);
                            lista.Add(new AmbienteSistema(sistema, (string)lector["RutaDestino"]));
                        }
                    }
                }
            }

            return ordenAmbientes
                .Select(nombre => new Ambiente(nombre, sistemasPorAmbiente[nombre], notificacionesPorAmbiente[nombre]))
                .ToList()
                .AsReadOnly();
        }

        public void Guardar(IReadOnlyList<Ambiente> ambientes)
        {
            Guard.ContraNulo(ambientes, nameof(ambientes));

            using (var conexion = AbrirConexion())
            using (var transaccion = conexion.BeginTransaction())
            {
                Ejecutar(conexion, transaccion, "DELETE FROM AmbienteSistema");
                Ejecutar(conexion, transaccion, "DELETE FROM Ambiente");

                foreach (var ambiente in ambientes)
                {
                    using (var comando = conexion.CreateCommand())
                    {
                        comando.Transaction = transaccion;
                        comando.CommandText = "INSERT INTO Ambiente (Nombre, NotificacionesHabilitadas) VALUES (@nombre, @notificaciones)";
                        comando.Parameters.AddWithValue("@nombre", ambiente.Nombre);
                        comando.Parameters.AddWithValue("@notificaciones", ambiente.NotificacionesHabilitadas);
                        comando.ExecuteNonQuery();
                    }

                    foreach (var ambienteSistema in ambiente.Sistemas)
                    {
                        using (var comando = conexion.CreateCommand())
                        {
                            comando.Transaction = transaccion;
                            comando.CommandText = @"
                                INSERT INTO AmbienteSistema (AmbienteNombre, SistemaCodigo, SistemaNombre, RutaDestino)
                                VALUES (@ambienteNombre, @sistemaCodigo, @sistemaNombre, @rutaDestino)";
                            comando.Parameters.AddWithValue("@ambienteNombre", ambiente.Nombre);
                            comando.Parameters.AddWithValue("@sistemaCodigo", ambienteSistema.Sistema.Codigo);
                            comando.Parameters.AddWithValue("@sistemaNombre", ambienteSistema.Sistema.Nombre);
                            comando.Parameters.AddWithValue("@rutaDestino", ambienteSistema.RutaDestino);
                            comando.ExecuteNonQuery();
                        }
                    }
                }

                transaccion.Commit();
            }
        }

        private static void Ejecutar(SqlConnection conexion, SqlTransaction transaccion, string sql)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.Transaction = transaccion;
                comando.CommandText = sql;
                comando.ExecuteNonQuery();
            }
        }

        private SqlConnection AbrirConexion()
        {
            var conexion = new SqlConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Infrastructure.SqlServer
{
    public sealed class SqlServerNotificationOutboxRepository : INotificationOutboxRepository
    {
        private readonly string _cadenaConexion;

        public SqlServerNotificationOutboxRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
            SqlServerEsquema.Verificar(_cadenaConexion);
        }

        public int Encolar(NotificacionPendiente notificacion)
        {
            Guard.ContraNulo(notificacion, nameof(notificacion));

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    INSERT INTO NotificationOutbox
                        (DeployHistoryId, FechaHoraCreacion, Canal, Destinatarios, Asunto, Contenido,
                         Estado, IntentosRealizados, ProximoIntentoEn, MensajeError)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@deployHistoryId, @fechaHoraCreacion, @canal, @destinatarios, @asunto, @contenido,
                         @estado, 0, NULL, NULL)";

                comando.Parameters.AddWithValue("@deployHistoryId", (object)notificacion.DespliegueId ?? DBNull.Value);
                comando.Parameters.AddWithValue("@fechaHoraCreacion", DateTime.Now);
                comando.Parameters.AddWithValue("@canal", notificacion.Canal);
                comando.Parameters.AddWithValue("@destinatarios", notificacion.Destinatarios);
                comando.Parameters.AddWithValue("@asunto", (object)notificacion.Asunto ?? DBNull.Value);
                comando.Parameters.AddWithValue("@contenido", notificacion.Contenido);
                comando.Parameters.AddWithValue("@estado", EstadoNotificacion.Pendiente.ToString());

                return (int)comando.ExecuteScalar();
            }
        }

        public IReadOnlyList<NotificacionPendiente> ObtenerPendientes(DateTime ahora)
        {
            var resultado = new List<NotificacionPendiente>();

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT * FROM NotificationOutbox
                    WHERE Estado IN (@pendiente, @reintentando)
                      AND (ProximoIntentoEn IS NULL OR ProximoIntentoEn <= @ahora)
                    ORDER BY FechaHoraCreacion ASC";

                comando.Parameters.AddWithValue("@pendiente", EstadoNotificacion.Pendiente.ToString());
                comando.Parameters.AddWithValue("@reintentando", EstadoNotificacion.Reintentando.ToString());
                comando.Parameters.AddWithValue("@ahora", ahora);

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        resultado.Add(Mapear(lector));
                }
            }

            return resultado.AsReadOnly();
        }

        public void MarcarEnviada(int id)
        {
            EjecutarActualizacion(id, @"
                UPDATE NotificationOutbox
                SET Estado = @estado, MensajeError = NULL
                WHERE Id = @id",
                comando =>
                {
                    comando.Parameters.AddWithValue("@estado", EstadoNotificacion.Enviado.ToString());
                });
        }

        public void MarcarFallidaConReintento(int id, string mensajeError, DateTime proximoIntentoEn)
        {
            EjecutarActualizacion(id, @"
                UPDATE NotificationOutbox
                SET Estado = @estado, IntentosRealizados = IntentosRealizados + 1,
                    ProximoIntentoEn = @proximoIntentoEn, MensajeError = @mensajeError
                WHERE Id = @id",
                comando =>
                {
                    comando.Parameters.AddWithValue("@estado", EstadoNotificacion.Reintentando.ToString());
                    comando.Parameters.AddWithValue("@proximoIntentoEn", proximoIntentoEn);
                    comando.Parameters.AddWithValue("@mensajeError", (object)mensajeError ?? DBNull.Value);
                });
        }

        public void MarcarFallidaDefinitiva(int id, string mensajeError)
        {
            EjecutarActualizacion(id, @"
                UPDATE NotificationOutbox
                SET Estado = @estado, IntentosRealizados = IntentosRealizados + 1, MensajeError = @mensajeError
                WHERE Id = @id",
                comando =>
                {
                    comando.Parameters.AddWithValue("@estado", EstadoNotificacion.Fallido.ToString());
                    comando.Parameters.AddWithValue("@mensajeError", (object)mensajeError ?? DBNull.Value);
                });
        }

        private void EjecutarActualizacion(int id, string sql, Action<SqlCommand> agregarParametros)
        {
            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = sql;
                comando.Parameters.AddWithValue("@id", id);
                agregarParametros(comando);
                comando.ExecuteNonQuery();
            }
        }

        private SqlConnection AbrirConexion()
        {
            var conexion = new SqlConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }

        private static NotificacionPendiente Mapear(SqlDataReader lector)
        {
            var estado = (EstadoNotificacion)Enum.Parse(typeof(EstadoNotificacion), (string)lector["Estado"]);
            var asunto = lector["Asunto"] == DBNull.Value ? null : (string)lector["Asunto"];
            var deployHistoryId = lector["DeployHistoryId"] == DBNull.Value ? (int?)null : (int)lector["DeployHistoryId"];
            var proximoIntentoEn = lector["ProximoIntentoEn"] == DBNull.Value ? (DateTime?)null : (DateTime)lector["ProximoIntentoEn"];
            var mensajeError = lector["MensajeError"] == DBNull.Value ? null : (string)lector["MensajeError"];

            return NotificacionPendiente.Reconstruir(
                (int)lector["Id"], deployHistoryId, (string)lector["Canal"], (string)lector["Destinatarios"],
                asunto, (string)lector["Contenido"], estado, (int)lector["IntentosRealizados"], proximoIntentoEn, mensajeError);
        }
    }
}

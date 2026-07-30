using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Infrastructure.SqlServer
{
    public sealed class SqlServerDeployHistoryRepository : IDeployHistoryRepository
    {
        private readonly string _cadenaConexion;

        public SqlServerDeployHistoryRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
            SqlServerEsquema.Verificar(_cadenaConexion);
        }

        public int Registrar(Despliegue despliegue)
        {
            Guard.ContraNulo(despliegue, nameof(despliegue));

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    INSERT INTO DeployHistory
                        (FechaHora, UsuarioAplicacion, UsuarioWindows, Equipo, Goc, Rama, Ambiente, Sistemas,
                         TiempoCompilacionSegundos, TiempoDespliegueSegundos, Resultado, Errores, Observaciones)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@fechaHora, @usuarioAplicacion, @usuarioWindows, @equipo, @goc, @rama, @ambiente, @sistemas,
                         @tiempoCompilacion, @tiempoDespliegue, @resultado, @errores, @observaciones)";

                comando.Parameters.AddWithValue("@fechaHora", despliegue.FechaHora);
                comando.Parameters.AddWithValue("@usuarioAplicacion", despliegue.UsuarioAplicacion);
                comando.Parameters.AddWithValue("@usuarioWindows", despliegue.UsuarioWindows);
                comando.Parameters.AddWithValue("@equipo", despliegue.Equipo);
                comando.Parameters.AddWithValue("@goc", despliegue.Goc);
                comando.Parameters.AddWithValue("@rama", despliegue.Rama);
                comando.Parameters.AddWithValue("@ambiente", despliegue.Ambiente);
                comando.Parameters.AddWithValue("@sistemas", string.Join(",", despliegue.Sistemas));
                comando.Parameters.AddWithValue("@tiempoCompilacion", despliegue.TiempoCompilacion.TotalSeconds);
                comando.Parameters.AddWithValue("@tiempoDespliegue", despliegue.TiempoDespliegue.TotalSeconds);
                comando.Parameters.AddWithValue("@resultado", despliegue.Resultado.ToString());
                comando.Parameters.AddWithValue("@errores", (object)despliegue.Errores ?? DBNull.Value);
                comando.Parameters.AddWithValue("@observaciones", (object)despliegue.Observaciones ?? DBNull.Value);

                return (int)comando.ExecuteScalar();
            }
        }

        public IReadOnlyList<Despliegue> Buscar(FiltroHistorial filtro)
        {
            filtro = filtro ?? new FiltroHistorial();

            var condiciones = new List<string>();
            var parametros = new Dictionary<string, object>();

            if (filtro.FechaDesde.HasValue)
            {
                condiciones.Add("FechaHora >= @fechaDesde");
                parametros["@fechaDesde"] = filtro.FechaDesde.Value;
            }

            if (filtro.FechaHasta.HasValue)
            {
                condiciones.Add("FechaHora <= @fechaHasta");
                parametros["@fechaHasta"] = filtro.FechaHasta.Value;
            }

            if (!string.IsNullOrWhiteSpace(filtro.Goc))
            {
                condiciones.Add("Goc = @goc");
                parametros["@goc"] = filtro.Goc;
            }

            if (!string.IsNullOrWhiteSpace(filtro.Ambiente))
            {
                condiciones.Add("Ambiente = @ambiente");
                parametros["@ambiente"] = filtro.Ambiente;
            }

            if (!string.IsNullOrWhiteSpace(filtro.Sistema))
            {
                condiciones.Add("Sistemas LIKE @sistema");
                parametros["@sistema"] = "%" + filtro.Sistema + "%";
            }

            if (filtro.Resultado.HasValue)
            {
                condiciones.Add("Resultado = @resultado");
                parametros["@resultado"] = filtro.Resultado.Value.ToString();
            }

            var clausulaWhere = condiciones.Count > 0 ? "WHERE " + string.Join(" AND ", condiciones) : "";

            var resultado = new List<Despliegue>();

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = $"SELECT * FROM DeployHistory {clausulaWhere} ORDER BY FechaHora DESC";
                foreach (var parametro in parametros)
                    comando.Parameters.AddWithValue(parametro.Key, parametro.Value);

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        resultado.Add(Mapear(lector));
                }
            }

            return resultado.AsReadOnly();
        }

        private SqlConnection AbrirConexion()
        {
            var conexion = new SqlConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }

        private static Despliegue Mapear(SqlDataReader lector)
        {
            var resultado = (ResultadoDespliegue)Enum.Parse(typeof(ResultadoDespliegue), (string)lector["Resultado"]);
            var sistemas = ((string)lector["Sistemas"]).Split(',');
            var fechaHora = (DateTime)lector["FechaHora"];
            var tiempoCompilacion = TimeSpan.FromSeconds(Convert.ToDouble(lector["TiempoCompilacionSegundos"]));
            var tiempoDespliegue = TimeSpan.FromSeconds(Convert.ToDouble(lector["TiempoDespliegueSegundos"]));
            var errores = lector["Errores"] == DBNull.Value ? null : (string)lector["Errores"];
            var observaciones = lector["Observaciones"] == DBNull.Value ? null : (string)lector["Observaciones"];

            return resultado == ResultadoDespliegue.Exitoso
                ? Despliegue.RegistrarExitoso(
                    (string)lector["UsuarioAplicacion"], (string)lector["UsuarioWindows"], (string)lector["Equipo"],
                    (string)lector["Goc"], (string)lector["Rama"], (string)lector["Ambiente"], sistemas,
                    tiempoCompilacion, tiempoDespliegue, observaciones, fechaHora)
                : Despliegue.RegistrarFallido(
                    (string)lector["UsuarioAplicacion"], (string)lector["UsuarioWindows"], (string)lector["Equipo"],
                    (string)lector["Goc"], (string)lector["Rama"], (string)lector["Ambiente"], sistemas,
                    tiempoCompilacion, tiempoDespliegue, errores, observaciones, fechaHora);
        }
    }
}

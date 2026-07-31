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
    /// Persistencia del catálogo de sistemas en las tablas
    /// ConfiguracionSistema/PasoDeBuild — reemplaza a JsonSistemaRepository
    /// por el mismo motivo que <see cref="SqlServerAmbienteRepository"/>.
    /// </summary>
    public sealed class SqlServerSistemaRepository : ISistemaRepository
    {
        private readonly string _cadenaConexion;

        public SqlServerSistemaRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
            SqlServerEsquema.Verificar(_cadenaConexion);
        }

        public IReadOnlyList<Sistema> ObtenerSistemasConocidos() =>
            ObtenerTodasLasConfiguraciones().Select(c => c.Sistema).ToList().AsReadOnly();

        public Result<ConfiguracionSistema> ObtenerConfiguracion(Sistema sistema)
        {
            Guard.ContraNulo(sistema, nameof(sistema));

            var configuracion = ObtenerTodasLasConfiguraciones()
                .FirstOrDefault(c => string.Equals(c.Sistema.Codigo, sistema.Codigo, StringComparison.OrdinalIgnoreCase));

            return configuracion == null
                ? Result.Fail<ConfiguracionSistema>($"No hay configuración registrada para el sistema '{sistema.Codigo}'.")
                : Result.Ok(configuracion);
        }

        public IReadOnlyList<ConfiguracionSistema> ObtenerTodasLasConfiguraciones()
        {
            var ordenCodigos = new List<string>();
            var datosBasicos = new Dictionary<string, (string Nombre, string RepositorioUrl, string CarpetaPrecompilada)>(StringComparer.OrdinalIgnoreCase);
            var pasosPorSistema = new Dictionary<string, List<PasoDeBuild>>(StringComparer.OrdinalIgnoreCase);

            using (var conexion = AbrirConexion())
            {
                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = "SELECT Codigo, Nombre, RepositorioUrl, CarpetaPrecompilada FROM ConfiguracionSistema ORDER BY Codigo";
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var codigo = (string)lector["Codigo"];
                            ordenCodigos.Add(codigo);
                            datosBasicos[codigo] = ((string)lector["Nombre"], (string)lector["RepositorioUrl"], (string)lector["CarpetaPrecompilada"]);
                            pasosPorSistema[codigo] = new List<PasoDeBuild>();
                        }
                    }
                }

                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = "SELECT SistemaCodigo, Orden, CarpetaProyecto, ParametrosMsBuild FROM PasoDeBuild ORDER BY SistemaCodigo, Orden";
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var sistemaCodigo = (string)lector["SistemaCodigo"];
                            if (!pasosPorSistema.TryGetValue(sistemaCodigo, out var lista))
                                continue;

                            var parametros = lector["ParametrosMsBuild"] == DBNull.Value ? null : (string)lector["ParametrosMsBuild"];
                            lista.Add(new PasoDeBuild((int)lector["Orden"], (string)lector["CarpetaProyecto"], parametros));
                        }
                    }
                }
            }

            return ordenCodigos.Select(codigo =>
            {
                var (nombre, repositorioUrl, carpetaPrecompilada) = datosBasicos[codigo];
                var sistema = new Sistema(codigo, nombre);
                var secuencia = new SecuenciaDeBuild(sistema, pasosPorSistema[codigo]);
                return new ConfiguracionSistema(sistema, repositorioUrl, carpetaPrecompilada, secuencia);
            }).ToList().AsReadOnly();
        }

        public void Guardar(IReadOnlyList<ConfiguracionSistema> configuraciones)
        {
            Guard.ContraNulo(configuraciones, nameof(configuraciones));

            using (var conexion = AbrirConexion())
            using (var transaccion = conexion.BeginTransaction())
            {
                Ejecutar(conexion, transaccion, "DELETE FROM PasoDeBuild");
                Ejecutar(conexion, transaccion, "DELETE FROM ConfiguracionSistema");

                foreach (var configuracion in configuraciones)
                {
                    using (var comando = conexion.CreateCommand())
                    {
                        comando.Transaction = transaccion;
                        comando.CommandText = @"
                            INSERT INTO ConfiguracionSistema (Codigo, Nombre, RepositorioUrl, CarpetaPrecompilada)
                            VALUES (@codigo, @nombre, @repositorioUrl, @carpetaPrecompilada)";
                        comando.Parameters.AddWithValue("@codigo", configuracion.Sistema.Codigo);
                        comando.Parameters.AddWithValue("@nombre", configuracion.Sistema.Nombre);
                        comando.Parameters.AddWithValue("@repositorioUrl", configuracion.RepositorioUrl);
                        comando.Parameters.AddWithValue("@carpetaPrecompilada", configuracion.CarpetaPrecompilada);
                        comando.ExecuteNonQuery();
                    }

                    foreach (var paso in configuracion.SecuenciaDeBuild.Pasos)
                    {
                        using (var comando = conexion.CreateCommand())
                        {
                            comando.Transaction = transaccion;
                            comando.CommandText = @"
                                INSERT INTO PasoDeBuild (SistemaCodigo, Orden, CarpetaProyecto, ParametrosMsBuild)
                                VALUES (@sistemaCodigo, @orden, @carpetaProyecto, @parametrosMsBuild)";
                            comando.Parameters.AddWithValue("@sistemaCodigo", configuracion.Sistema.Codigo);
                            comando.Parameters.AddWithValue("@orden", paso.Orden);
                            comando.Parameters.AddWithValue("@carpetaProyecto", paso.CarpetaProyecto);
                            comando.Parameters.AddWithValue("@parametrosMsBuild", (object)paso.ParametrosMsBuild ?? DBNull.Value);
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

using System;
using System.Data.SqlClient;
using System.IO;

namespace GocDeployManager.Infrastructure.Tests
{
    /// <summary>
    /// Base de datos real (no mockeada) contra LocalDB, única por prueba y
    /// eliminada automáticamente al final. Aplica sql/schema-sql-server.sql
    /// tal cual lo aplicaría un DBA real, para probar los repositorios contra
    /// el mismo esquema que se documenta para producción.
    /// </summary>
    internal sealed class BaseDeDatosSqlServerDePrueba : IDisposable
    {
        private const string CadenaConexionMaster = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;";

        public string NombreBaseDatos { get; }
        public string CadenaConexion { get; }

        public BaseDeDatosSqlServerDePrueba()
        {
            NombreBaseDatos = "GocDeployManagerTests_" + Guid.NewGuid().ToString("N");
            CadenaConexion = $@"Server=(localdb)\MSSQLLocalDB;Database={NombreBaseDatos};Integrated Security=true;";

            EjecutarContraMaster($"CREATE DATABASE [{NombreBaseDatos}]");
            AplicarEsquema();
        }

        private void AplicarEsquema()
        {
            var rutaScript = ResolverRutaScriptEsquema();
            var contenido = File.ReadAllText(rutaScript);

            using (var conexion = new SqlConnection(CadenaConexion))
            {
                conexion.Open();

                foreach (var lote in contenido.Split(new[] { "\nGO", "\rGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var texto = lote.Trim();
                    if (texto.Length == 0)
                        continue;

                    using (var comando = conexion.CreateCommand())
                    {
                        comando.CommandText = texto;
                        comando.ExecuteNonQuery();
                    }
                }
            }
        }

        private static string ResolverRutaScriptEsquema()
        {
            var candidato = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (candidato != null)
            {
                var rutaScript = Path.Combine(candidato.FullName, "sql", "schema-sql-server.sql");
                if (File.Exists(rutaScript))
                    return rutaScript;

                candidato = candidato.Parent;
            }

            throw new InvalidOperationException("No se encontró sql/schema-sql-server.sql subiendo desde " + AppDomain.CurrentDomain.BaseDirectory);
        }

        public void Dispose()
        {
            // Sin esto, DROP DATABASE falla si queda alguna conexión pool abierta.
            SqlConnection.ClearAllPools();

            EjecutarContraMaster($@"
                IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{NombreBaseDatos}')
                BEGIN
                    ALTER DATABASE [{NombreBaseDatos}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{NombreBaseDatos}];
                END");
        }

        private static void EjecutarContraMaster(string sql)
        {
            using (var conexion = new SqlConnection(CadenaConexionMaster))
            using (var comando = conexion.CreateCommand())
            {
                conexion.Open();
                comando.CommandText = sql;
                comando.ExecuteNonQuery();
            }
        }
    }
}

using System;
using System.Data.SqlClient;

namespace GocDeployManager.Infrastructure.SqlServer
{
    /// <summary>
    /// Verifica que las tablas de GocDeployManager existan en la base SQL
    /// Server destino. A diferencia del esquema anterior de SQLite, NUNCA
    /// crea las tablas — eso lo hace sql/schema-sql-server.sql, ejecutado una
    /// vez por un DBA/administrador (el login de la app puede no tener
    /// permisos de CREATE TABLE en un servidor corporativo compartido).
    /// </summary>
    public static class SqlServerEsquema
    {
        public static void Verificar(string cadenaConexion)
        {
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "SELECT COUNT(*) FROM sys.tables WHERE name IN (" +
                        "'AppUser', 'DeployHistory', 'NotificationOutbox', " +
                        "'Ambiente', 'AmbienteSistema', 'ConfiguracionSistema', 'PasoDeBuild')";

                    var tablasExistentes = Convert.ToInt32(comando.ExecuteScalar());
                    if (tablasExistentes < 7)
                        throw new InvalidOperationException(
                            "La base de datos no tiene las tablas de GocDeployManager. " +
                            "Ejecuta sql/schema-sql-server.sql contra esta base antes de usar la aplicación.");
                }
            }
        }
    }
}

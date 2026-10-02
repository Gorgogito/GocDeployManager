using System.Collections.Generic;
using System.Data.SqlClient;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Infrastructure.SqlServer
{
    /// <summary>
    /// Persistencia de las ramas estándar en la tabla RamaConfigurada.
    /// Al ser una funcionalidad posterior al esquema original, la tabla se
    /// crea sola (con master y develop) la primera vez que se usa si el DBA
    /// no la creó — así los despliegues ya instalados no requieren ejecutar
    /// ningún script. Si el login de la app no puede crear tablas, el error
    /// indica ejecutar sql/schema-sql-server.sql.
    /// </summary>
    public sealed class SqlServerRamaRepository : IRamaConfiguradaRepository
    {
        private const string SqlCrearTabla = @"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RamaConfigurada')
            BEGIN
                CREATE TABLE RamaConfigurada (
                    Orden   INT             NOT NULL PRIMARY KEY,
                    Nombre  NVARCHAR(200)   NOT NULL UNIQUE
                );
                INSERT INTO RamaConfigurada (Orden, Nombre) VALUES (1, 'master'), (2, 'develop');
            END";

        private readonly string _cadenaConexion;

        public SqlServerRamaRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
        }

        public IReadOnlyList<string> ObtenerTodas()
        {
            var ramas = new List<string>();

            using (var conexion = AbrirConexionConTabla())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT Nombre FROM RamaConfigurada ORDER BY Orden";
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        ramas.Add((string)lector["Nombre"]);
                }
            }

            return ramas.AsReadOnly();
        }

        public void Guardar(IReadOnlyList<string> ramas)
        {
            Guard.ContraNulo(ramas, nameof(ramas));

            using (var conexion = AbrirConexionConTabla())
            using (var transaccion = conexion.BeginTransaction())
            {
                using (var borrar = conexion.CreateCommand())
                {
                    borrar.Transaction = transaccion;
                    borrar.CommandText = "DELETE FROM RamaConfigurada";
                    borrar.ExecuteNonQuery();
                }

                for (var i = 0; i < ramas.Count; i++)
                {
                    using (var comando = conexion.CreateCommand())
                    {
                        comando.Transaction = transaccion;
                        comando.CommandText = "INSERT INTO RamaConfigurada (Orden, Nombre) VALUES (@orden, @nombre)";
                        comando.Parameters.AddWithValue("@orden", i + 1);
                        comando.Parameters.AddWithValue("@nombre", ramas[i]);
                        comando.ExecuteNonQuery();
                    }
                }

                transaccion.Commit();
            }
        }

        private SqlConnection AbrirConexionConTabla()
        {
            var conexion = new SqlConnection(_cadenaConexion);
            conexion.Open();

            try
            {
                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = SqlCrearTabla;
                    comando.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                conexion.Dispose();
                throw new System.InvalidOperationException(
                    "No existe la tabla RamaConfigurada y el usuario de la base de datos no pudo crearla. " +
                    "Ejecuta sql/schema-sql-server.sql contra esta base (" + ex.Message + ").", ex);
            }

            return conexion;
        }
    }
}

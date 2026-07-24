using Microsoft.Data.Sqlite;

namespace GocDeployManager.Infrastructure.Sqlite
{
    /// <summary>
    /// Crea las tablas de GocDeployManager.db si no existen. Un único archivo
    /// SQLite con AppUser y DeployHistory (sección 16 del análisis).
    /// </summary>
    internal static class SqliteEsquema
    {
        public static void Asegurar(string cadenaConexion)
        {
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                using (var comando = conexion.CreateCommand())
                {
                    comando.CommandText = @"
                        CREATE TABLE IF NOT EXISTS AppUser (
                            NombreUsuario TEXT PRIMARY KEY,
                            NombreVisible TEXT NOT NULL,
                            Rol TEXT NOT NULL,
                            Activo INTEGER NOT NULL,
                            HashContrasena TEXT NOT NULL,
                            SalContrasena TEXT NOT NULL,
                            UsuarioBitbucket TEXT NULL,
                            ContrasenaBitbucketProtegida TEXT NULL
                        );

                        CREATE TABLE IF NOT EXISTS DeployHistory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FechaHora TEXT NOT NULL,
                            UsuarioAplicacion TEXT NOT NULL,
                            UsuarioWindows TEXT NOT NULL,
                            Equipo TEXT NOT NULL,
                            Goc TEXT NOT NULL,
                            Rama TEXT NOT NULL,
                            Ambiente TEXT NOT NULL,
                            Sistemas TEXT NOT NULL,
                            TiempoCompilacionSegundos REAL NOT NULL,
                            TiempoDespliegueSegundos REAL NOT NULL,
                            Resultado TEXT NOT NULL,
                            Errores TEXT NULL,
                            Observaciones TEXT NULL
                        );";
                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}

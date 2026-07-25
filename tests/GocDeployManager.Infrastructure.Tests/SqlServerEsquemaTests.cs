using System;
using System.Data.SqlClient;
using GocDeployManager.Infrastructure.SqlServer;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqlServerEsquemaTests
    {
        [Test]
        public void Verificar_SiFaltanLasTablas_LanzaExcepcionConMensajeQueApuntaAlScript()
        {
            var nombreBaseDatos = "GocDeployManagerTests_SinEsquema_" + Guid.NewGuid().ToString("N");
            var cadenaConexionMaster = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;";
            var cadenaConexion = $@"Server=(localdb)\MSSQLLocalDB;Database={nombreBaseDatos};Integrated Security=true;";

            EjecutarContraMaster(cadenaConexionMaster, $"CREATE DATABASE [{nombreBaseDatos}]");

            try
            {
                var excepcion = Assert.Throws<InvalidOperationException>(() => SqlServerEsquema.Verificar(cadenaConexion));
                Assert.That(excepcion.Message, Does.Contain("schema-sql-server.sql"));
            }
            finally
            {
                SqlConnection.ClearAllPools();
                EjecutarContraMaster(cadenaConexionMaster, $@"
                    ALTER DATABASE [{nombreBaseDatos}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{nombreBaseDatos}];");
            }
        }

        private static void EjecutarContraMaster(string cadenaConexion, string sql)
        {
            using (var conexion = new SqlConnection(cadenaConexion))
            using (var comando = conexion.CreateCommand())
            {
                conexion.Open();
                comando.CommandText = sql;
                comando.ExecuteNonQuery();
            }
        }
    }
}

using System.IO;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Sqlite;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqliteAppUserRepositoryTests
    {
        [Test]
        public void AgregarYBuscar_HaceRoundTripCompletoIncluidasLasCredencialesDeBitbucket()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteAppUserRepository(cadenaConexion);

                var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Administrador, "hash123", "sal123");
                usuario.EstablecerCredencialesBitbucket("jtorres.bb", "protegida-base64");
                repositorio.Agregar(usuario);

                var recuperado = repositorio.BuscarPorNombreUsuario("jtorres");

                Assert.That(recuperado, Is.Not.Null);
                Assert.That(recuperado.NombreVisible, Is.EqualTo("Jorge Torres"));
                Assert.That(recuperado.Rol, Is.EqualTo(RolUsuario.Administrador));
                Assert.That(recuperado.UsuarioBitbucket, Is.EqualTo("jtorres.bb"));
                Assert.That(recuperado.ContrasenaBitbucketProtegida, Is.EqualTo("protegida-base64"));
            }
        }

        [Test]
        public void BuscarPorNombreUsuario_SiNoExiste_DevuelveNulo()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteAppUserRepository(cadenaConexion);

                Assert.That(repositorio.BuscarPorNombreUsuario("no-existe"), Is.Null);
            }
        }

        [Test]
        public void Actualizar_PersisteElResetDeContrasenaYElCambioDeEstado()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteAppUserRepository(cadenaConexion);

                var usuario = new AppUser("mlopez", "María López", RolUsuario.Consulta, "hashViejo", "salVieja");
                repositorio.Agregar(usuario);

                usuario.ResetearContrasena("hashNuevo", "salNueva");
                usuario.Desactivar();
                repositorio.Actualizar(usuario);

                var recuperado = repositorio.BuscarPorNombreUsuario("mlopez");

                Assert.That(recuperado.HashContrasena, Is.EqualTo("hashNuevo"));
                Assert.That(recuperado.Activo, Is.False);
            }
        }

        [Test]
        public void ObtenerTodos_DevuelveTodosLosUsuariosOrdenadosPorNombre()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteAppUserRepository(cadenaConexion);

                repositorio.Agregar(new AppUser("zulema", "Zulema", RolUsuario.Operador, "h", "s"));
                repositorio.Agregar(new AppUser("ana", "Ana", RolUsuario.Operador, "h", "s"));

                var todos = repositorio.ObtenerTodos();

                Assert.That(todos, Has.Count.EqualTo(2));
                Assert.That(todos[0].NombreUsuario, Is.EqualTo("ana"));
            }
        }
    }
}

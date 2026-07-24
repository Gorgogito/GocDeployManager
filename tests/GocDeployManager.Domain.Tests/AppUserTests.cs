using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class AppUserTests
    {
        [TestCase(RolUsuario.Administrador, true, true)]
        [TestCase(RolUsuario.Operador, true, false)]
        [TestCase(RolUsuario.Consulta, false, false)]
        public void LosPermisosDependenDelRol(RolUsuario rol, bool puedeDesplegar, bool puedeAdministrar)
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", rol, "hash", "sal");

            Assert.That(usuario.PuedeDesplegar, Is.EqualTo(puedeDesplegar));
            Assert.That(usuario.PuedeAdministrar, Is.EqualTo(puedeAdministrar));
        }

        [Test]
        public void ResetearContrasena_ReemplazaHashYSal()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hashViejo", "salVieja");

            usuario.ResetearContrasena("hashNuevo", "salNueva");

            Assert.That(usuario.HashContrasena, Is.EqualTo("hashNuevo"));
            Assert.That(usuario.SalContrasena, Is.EqualTo("salNueva"));
        }

        [Test]
        public void NuevoUsuario_QuedaActivoPorDefecto()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");

            Assert.That(usuario.Activo, Is.True);

            usuario.Desactivar();
            Assert.That(usuario.Activo, Is.False);
        }
    }
}

using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class MensajeSalidaDespliegueTests
    {
        [Test]
        public void Crear_NoMarcaInicioDeEtapa()
        {
            var mensaje = MensajeSalidaDespliegue.Crear(NivelMensajeSalida.Info, "Compilando Sit.BusinessEntities...");

            Assert.That(mensaje.Nivel, Is.EqualTo(NivelMensajeSalida.Info));
            Assert.That(mensaje.Texto, Is.EqualTo("Compilando Sit.BusinessEntities..."));
            Assert.That(mensaje.Etapa, Is.Null);
        }

        [Test]
        public void InicioDeEtapa_MarcaLaEtapaYUsaNivelInfo()
        {
            var mensaje = MensajeSalidaDespliegue.InicioDeEtapa(EtapaProgreso.Clonado, "CLONADO — SIT");

            Assert.That(mensaje.Etapa, Is.EqualTo(EtapaProgreso.Clonado));
            Assert.That(mensaje.Nivel, Is.EqualTo(NivelMensajeSalida.Info));
            Assert.That(mensaje.Texto, Is.EqualTo("CLONADO — SIT"));
        }

        [Test]
        public void Crear_SiElTextoEsNuloOVacio_Falla()
        {
            Assert.Throws<System.ArgumentException>(() => MensajeSalidaDespliegue.Crear(NivelMensajeSalida.Error, string.Empty));
        }
    }
}

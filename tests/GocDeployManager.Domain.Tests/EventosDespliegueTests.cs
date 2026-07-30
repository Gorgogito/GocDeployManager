using System;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class EventosDespliegueTests
    {
        [Test]
        public void DespliegueIniciadoEvento_ExponeLosDatosDeLaSolicitud()
        {
            var ahora = DateTime.Now;
            var evento = new DespliegueIniciadoEvento("GOC-00001", "Testing", new[] { "SIT" }, "jtorres", ahora);

            Assert.That(evento.Goc, Is.EqualTo("GOC-00001"));
            Assert.That(evento.Ambiente, Is.EqualTo("Testing"));
            Assert.That(evento.Sistemas, Is.EquivalentTo(new[] { "SIT" }));
            Assert.That(evento.FechaHora, Is.EqualTo(ahora));
        }

        [Test]
        public void DespliegueExitosoEvento_CalculaLaDuracionComoFinMenosInicio()
        {
            var inicio = new DateTime(2026, 7, 28, 10, 0, 0);
            var fin = new DateTime(2026, 7, 28, 10, 12, 30);

            var evento = new DespliegueExitosoEvento(
                despliegueId: 42, goc: "GOC-00001", rama: "feature/GOC-00001", ambiente: "Testing",
                sistemas: new[] { "SIT" }, usuarioAplicacion: "jtorres", fechaHoraInicio: inicio, fechaHoraFin: fin);

            Assert.That(evento.Duracion, Is.EqualTo(TimeSpan.FromMinutes(12.5)));
            Assert.That(evento.DespliegueId, Is.EqualTo(42));
        }

        [Test]
        public void DespliegueFallidoEvento_ExponeEtapaYMensajeResumido()
        {
            var evento = new DespliegueFallidoEvento(
                despliegueId: 7, goc: "GOC-00002", ambiente: "Testing", sistemas: new[] { "SIT" },
                usuarioAplicacion: "jtorres", fechaHora: DateTime.Now,
                etapa: EtapaDespliegue.Compilacion, mensajeError: "error de compilación en Sit.BusinessEntities");

            Assert.That(evento.Etapa, Is.EqualTo(EtapaDespliegue.Compilacion));
            Assert.That(evento.MensajeError, Does.Contain("compilación"));
        }

        [Test]
        public void DespliegueFallidoEvento_RechazaMensajeVacio()
        {
            Assert.Throws<ArgumentException>(() => new DespliegueFallidoEvento(
                despliegueId: 1, goc: "GOC-00003", ambiente: "Testing", sistemas: new[] { "SIT" },
                usuarioAplicacion: "jtorres", fechaHora: DateTime.Now,
                etapa: EtapaDespliegue.Clonado, mensajeError: ""));
        }
    }
}

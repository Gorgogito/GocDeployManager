using System;
using System.Collections.Generic;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class AmbienteTests
    {
        private static readonly Sistema Sit = new Sistema("SIT", "SIT");
        private static readonly Sistema Idi = new Sistema("IDI", "IDI");
        private static readonly Sistema ProPag = new Sistema("PROPAG", "ProPag");

        [Test]
        public void BuscarSistema_DevuelveLaRutaCuandoElAmbienteLoSoporta()
        {
            var ambiente = new Ambiente("Desarrollo", new[]
            {
                new AmbienteSistema(Sit, @"\\Sdpeapp00009\Aplicaciones\SIT"),
                new AmbienteSistema(Idi, @"\\Sdpeapp00009\Aplicaciones\IDI"),
            });

            var encontrado = ambiente.BuscarSistema(Sit);

            Assert.That(encontrado, Is.Not.Null);
            Assert.That(encontrado.RutaDestino, Is.EqualTo(@"\\Sdpeapp00009\Aplicaciones\SIT"));
        }

        [Test]
        public void BuscarSistema_DevuelveNuloCuandoElAmbienteNoLoSoporta()
        {
            // Un ambiente puede definir un solo sistema (confirmado con el cliente).
            var ambiente = new Ambiente("ProPag-Desarrollo", new[]
            {
                new AmbienteSistema(ProPag, @"\\Sdpeapp00009\Aplicaciones\ProvisionPagos"),
            });

            Assert.That(ambiente.BuscarSistema(Sit), Is.Null);
            Assert.That(ambiente.SoportaSistema(ProPag), Is.True);
        }

        [Test]
        public void Constructor_RechazaAmbienteSinSistemas()
        {
            Assert.Throws<ArgumentException>(() => new Ambiente("Vacío", new AmbienteSistema[0]));
        }

        [Test]
        public void Constructor_NotificacionesHabilitadasPorDefecto()
        {
            var ambiente = new Ambiente("Testing", new[] { new AmbienteSistema(Sit, @"\\Sdpeapp00009\Aplicaciones\SIT") });

            Assert.That(ambiente.NotificacionesHabilitadas, Is.True);
        }

        [Test]
        public void Constructor_PermiteDeshabilitarNotificaciones()
        {
            var ambiente = new Ambiente(
                "Desarrollo", new[] { new AmbienteSistema(Sit, @"\\Sdpeapp00009\Aplicaciones\SIT") },
                notificacionesHabilitadas: false);

            Assert.That(ambiente.NotificacionesHabilitadas, Is.False);
        }
    }
}

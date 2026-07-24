using System;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class DespliegueTests
    {
        [Test]
        public void RegistrarExitoso_NoLlevaErrores()
        {
            var despliegue = Despliegue.RegistrarExitoso(
                "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00001", "feature/GOC-00001",
                "Desarrollo", new[] { "SIT" }, TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(30));

            Assert.That(despliegue.Resultado, Is.EqualTo(ResultadoDespliegue.Exitoso));
            Assert.That(despliegue.Errores, Is.Null);
        }

        [Test]
        public void RegistrarFallido_ExigeElDetalleDelError()
        {
            Assert.Throws<ArgumentException>(() =>
                Despliegue.RegistrarFallido(
                    "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00001", "feature/GOC-00001",
                    "Desarrollo", new[] { "SIT" }, TimeSpan.FromMinutes(1), TimeSpan.Zero,
                    errores: ""));
        }

        [Test]
        public void RegistrarFallido_ConDetalleQuedaRegistradoComoFallido()
        {
            var despliegue = Despliegue.RegistrarFallido(
                "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00002", "feature/GOC-00002",
                "Testing", new[] { "SIT", "IDI" }, TimeSpan.FromMinutes(1), TimeSpan.Zero,
                errores: "Falló la compilación de Sit.BusinessLayer");

            Assert.That(despliegue.Resultado, Is.EqualTo(ResultadoDespliegue.Fallido));
            Assert.That(despliegue.Errores, Is.EqualTo("Falló la compilación de Sit.BusinessLayer"));
            Assert.That(despliegue.Sistemas, Has.Count.EqualTo(2));
        }
    }
}

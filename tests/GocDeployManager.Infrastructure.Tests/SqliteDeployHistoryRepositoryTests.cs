using System;
using System.IO;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Sqlite;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqliteDeployHistoryRepositoryTests
    {
        [Test]
        public void Registrar_PreservaLaFechaHoraOriginalAlRecuperar()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteDeployHistoryRepository(cadenaConexion);

                var fechaOriginal = new DateTime(2026, 3, 5, 14, 30, 0);
                var despliegue = Despliegue.RegistrarExitoso(
                    "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00001", "feature/GOC-00001",
                    "Desarrollo", new[] { "SIT" }, TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(20),
                    fechaHora: fechaOriginal);

                repositorio.Registrar(despliegue);

                var recuperados = repositorio.Buscar(new FiltroHistorial());

                Assert.That(recuperados, Has.Count.EqualTo(1));
                Assert.That(recuperados[0].FechaHora, Is.EqualTo(fechaOriginal));
                Assert.That(recuperados[0].Sistemas, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Buscar_FiltraPorGocAmbienteYResultado()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var cadenaConexion = $"Data Source={Path.Combine(temp.Ruta, "goc.db")}";
                var repositorio = new SqliteDeployHistoryRepository(cadenaConexion);

                repositorio.Registrar(Despliegue.RegistrarExitoso(
                    "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00001", "feature/GOC-00001",
                    "Desarrollo", new[] { "SIT" }, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10)));

                repositorio.Registrar(Despliegue.RegistrarFallido(
                    "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00002", "feature/GOC-00002",
                    "Testing", new[] { "IDI" }, TimeSpan.FromMinutes(1), TimeSpan.Zero, "falló el build"));

                var soloGoc1 = repositorio.Buscar(new FiltroHistorial { Goc = "GOC-00001" });
                var soloFallidos = repositorio.Buscar(new FiltroHistorial { Resultado = ResultadoDespliegue.Fallido });

                Assert.That(soloGoc1, Has.Count.EqualTo(1));
                Assert.That(soloGoc1[0].Ambiente, Is.EqualTo("Desarrollo"));

                Assert.That(soloFallidos, Has.Count.EqualTo(1));
                Assert.That(soloFallidos[0].Errores, Is.EqualTo("falló el build"));
            }
        }
    }
}

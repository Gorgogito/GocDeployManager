using System.Collections.Generic;
using System.IO;
using GocDeployManager.Infrastructure.Deploy;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class FileDeployerTests
    {
        [Test]
        public void Copiar_NuncaSobrescribeUnArchivoExcluidoQueYaExisteEnDestino()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var origen = Path.Combine(temp.Ruta, "origen");
                var destino = Path.Combine(temp.Ruta, "destino");
                Directory.CreateDirectory(origen);
                Directory.CreateDirectory(destino);

                File.WriteAllText(Path.Combine(origen, "web.config"), "config nueva");
                File.WriteAllText(Path.Combine(destino, "web.config"), "config de producción, no tocar");

                var deployer = new FileDeployer();
                var resultado = deployer.Copiar(origen, destino, new List<string> { "web.config" });

                Assert.That(resultado.IsSuccess, Is.True);
                Assert.That(File.ReadAllText(Path.Combine(destino, "web.config")), Is.EqualTo("config de producción, no tocar"));
            }
        }

        [Test]
        public void Copiar_CopiaWebConfigSiNoExisteAunEnLaListaDeExclusion()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var origen = Path.Combine(temp.Ruta, "origen");
                var destino = Path.Combine(temp.Ruta, "destino");
                Directory.CreateDirectory(origen);

                File.WriteAllText(Path.Combine(origen, "web.config"), "config nueva");

                var deployer = new FileDeployer();
                var resultado = deployer.Copiar(origen, destino, new List<string> { "web.config" });

                Assert.That(resultado.IsSuccess, Is.True);
                Assert.That(File.ReadAllText(Path.Combine(destino, "web.config")), Is.EqualTo("config nueva"));
            }
        }

        [Test]
        public void Copiar_SobrescribeArchivosNoExcluidos()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var origen = Path.Combine(temp.Ruta, "origen");
                var destino = Path.Combine(temp.Ruta, "destino");
                Directory.CreateDirectory(origen);
                Directory.CreateDirectory(destino);

                File.WriteAllText(Path.Combine(origen, "Default.aspx"), "version nueva");
                File.WriteAllText(Path.Combine(destino, "Default.aspx"), "version vieja");

                var deployer = new FileDeployer();
                var resultado = deployer.Copiar(origen, destino, new List<string> { "web.config" });

                Assert.That(resultado.IsSuccess, Is.True);
                Assert.That(File.ReadAllText(Path.Combine(destino, "Default.aspx")), Is.EqualTo("version nueva"));
            }
        }

        [Test]
        public void Copiar_EliminaDelDestinoLoQueYaNoExisteEnOrigen()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var origen = Path.Combine(temp.Ruta, "origen");
                var destino = Path.Combine(temp.Ruta, "destino");
                Directory.CreateDirectory(origen);
                Directory.CreateDirectory(destino);

                File.WriteAllText(Path.Combine(destino, "PaginaObsoleta.aspx"), "ya no existe en origen");

                var deployer = new FileDeployer();
                deployer.Copiar(origen, destino, new List<string> { "web.config" });

                Assert.That(File.Exists(Path.Combine(destino, "PaginaObsoleta.aspx")), Is.False);
            }
        }
    }
}

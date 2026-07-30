using System;
using System.Linq;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class NotificationDispatcherTests
    {
        private GrupoDestinatariosRepositoryEnMemoria _grupos;
        private ConfiguracionCanalTeamsRepositoryEnMemoria _canalTeams;
        private PlantillaRepositoryEnMemoria _plantillas;
        private NotificationOutboxRepositoryEnMemoria _outbox;
        private NotificationDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            var ana = Destinatario.Crear("Ana", "ana@empresa.com").Value;
            var luis = Destinatario.Crear("Luis", "luis@empresa.com").Value;

            _grupos = new GrupoDestinatariosRepositoryEnMemoria(new[]
            {
                new GrupoDestinatarios("Desarrollo", new[] { ana, luis }),
            });
            _canalTeams = new ConfiguracionCanalTeamsRepositoryEnMemoria(new[]
            {
                new MapeoCanalTeams(null, null, "https://webhook/sura-peru-teams"),
            });
            _plantillas = new PlantillaRepositoryEnMemoria();
            _outbox = new NotificationOutboxRepositoryEnMemoria();

            _dispatcher = new NotificationDispatcher(_grupos, _canalTeams, _plantillas, new Plantillas.PlantillaRendererDeTokens(), _outbox, new AppLoggerDePrueba());
        }

        [Test]
        public void Manejar_EventoIniciado_EncolaCorreoYTeamsSinDespliegueId()
        {
            var evento = new DespliegueIniciadoEvento(
                "GOC-00001", "Testing", new[] { "SIT" }, "jtorres", DateTime.Now,
                gruposDestinatariosSeleccionados: new[] { "Desarrollo" });

            _dispatcher.Manejar(evento);

            Assert.That(_outbox.Todas, Has.Count.EqualTo(2));
            var correo = _outbox.Todas.Single(n => n.Canal == "Email");
            var teams = _outbox.Todas.Single(n => n.Canal == "Teams");

            Assert.That(correo.DespliegueId, Is.Null);
            Assert.That(correo.Destinatarios, Does.Contain("ana@empresa.com"));
            Assert.That(correo.Destinatarios, Does.Contain("luis@empresa.com"));
            Assert.That(correo.Asunto, Does.Contain("iniciado"));

            Assert.That(teams.Destinatarios, Is.EqualTo("https://webhook/sura-peru-teams"));
            Assert.That(teams.Contenido, Does.Contain("GOC-00001"));
        }

        [Test]
        public void Manejar_EventoExitoso_EncolaConElDespliegueIdDelEvento()
        {
            var evento = new DespliegueExitosoEvento(
                despliegueId: 55, goc: "GOC-00002", rama: "feature/GOC-00002", ambiente: "Testing",
                sistemas: new[] { "SIT" }, usuarioAplicacion: "jtorres",
                fechaHoraInicio: DateTime.Now.AddMinutes(-5), fechaHoraFin: DateTime.Now,
                gruposDestinatariosSeleccionados: new[] { "Desarrollo" });

            _dispatcher.Manejar(evento);

            Assert.That(_outbox.Todas.All(n => n.DespliegueId == 55), Is.True);
        }

        [Test]
        public void Manejar_SinGruposNiAdicionales_NoEncolaCorreo()
        {
            var evento = new DespliegueIniciadoEvento("GOC-00003", "Testing", new[] { "SIT" }, "jtorres", DateTime.Now);

            _dispatcher.Manejar(evento);

            Assert.That(_outbox.Todas.Any(n => n.Canal == "Email"), Is.False);
        }

        [Test]
        public void Manejar_SinMapeosDeTeamsConfigurados_NoEncolaTeams()
        {
            var dispatcherSinTeams = new NotificationDispatcher(
                _grupos, new ConfiguracionCanalTeamsRepositoryEnMemoria(), _plantillas, new Plantillas.PlantillaRendererDeTokens(), _outbox, new AppLoggerDePrueba());

            var evento = new DespliegueIniciadoEvento(
                "GOC-00004", "Testing", new[] { "SIT" }, "jtorres", DateTime.Now,
                gruposDestinatariosSeleccionados: new[] { "Desarrollo" });

            dispatcherSinTeams.Manejar(evento);

            Assert.That(_outbox.Todas.Any(n => n.Canal == "Teams"), Is.False);
        }

        [Test]
        public void Manejar_SiSoloSeSeleccionoTeams_NoEncolaCorreoAunConGruposConfigurados()
        {
            var evento = new DespliegueIniciadoEvento(
                "GOC-00006", "Testing", new[] { "SIT" }, "jtorres", DateTime.Now,
                gruposDestinatariosSeleccionados: new[] { "Desarrollo" },
                canalesSeleccionados: new[] { "Teams" });

            _dispatcher.Manejar(evento);

            Assert.That(_outbox.Todas.Any(n => n.Canal == "Email"), Is.False);
            Assert.That(_outbox.Todas.Any(n => n.Canal == "Teams"), Is.True);
        }

        [Test]
        public void Manejar_EventoFallido_EscapaComillasEnElMensajeDeErrorParaTeams()
        {
            var evento = new DespliegueFallidoEvento(
                despliegueId: 9, goc: "GOC-00005", ambiente: "Testing", sistemas: new[] { "SIT" },
                usuarioAplicacion: "jtorres", fechaHora: DateTime.Now, etapa: EtapaDespliegue.Compilacion,
                mensajeError: "error: el símbolo \"Foo\" no existe",
                gruposDestinatariosSeleccionados: new[] { "Desarrollo" });

            _dispatcher.Manejar(evento);

            var teams = _outbox.Todas.Single(n => n.Canal == "Teams");
            Assert.That(teams.Contenido, Does.Contain("\\\"Foo\\\""));
        }
    }
}

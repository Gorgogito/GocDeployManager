using System;
using System.Collections.Generic;

namespace GocDeployManager.Notifications.Plantillas
{
    /// <summary>
    /// Contenido de fábrica de cada plantilla — lo que devuelve "Restaurar
    /// plantilla por defecto" en el editor, y lo que se usa mientras un
    /// Administrador no haya guardado una versión propia.
    /// </summary>
    public static class PlantillasPorDefecto
    {
        private static readonly Dictionary<(string Canal, string TipoEvento), string> Contenido =
            new Dictionary<(string, string), string>
            {
                [(NombresDeCanal.Email, TiposDeEvento.Iniciado)] =
                    "<h2>Despliegue iniciado — {{Goc}}</h2>" +
                    "<table><tr><td>Usuario responsable</td><td>{{UsuarioAplicacion}}</td></tr>" +
                    "<tr><td>Ambiente</td><td>{{Ambiente}}</td></tr>" +
                    "<tr><td>Sistemas</td><td>{{Sistemas}}</td></tr>" +
                    "<tr><td>Fecha y hora</td><td>{{FechaHora}}</td></tr></table>",

                [(NombresDeCanal.Email, TiposDeEvento.Exitoso)] =
                    "<h2 style=\"color:#2E7D5B\">Despliegue exitoso — {{Goc}}</h2>" +
                    "<table><tr><td>Rama</td><td>{{Rama}}</td></tr>" +
                    "<tr><td>Ambiente</td><td>{{Ambiente}}</td></tr>" +
                    "<tr><td>Sistemas</td><td>{{Sistemas}}</td></tr>" +
                    "<tr><td>Usuario responsable</td><td>{{UsuarioAplicacion}}</td></tr>" +
                    "<tr><td>Inicio</td><td>{{FechaHoraInicio}}</td></tr>" +
                    "<tr><td>Fin</td><td>{{FechaHora}}</td></tr>" +
                    "<tr><td>Duración</td><td>{{Duracion}}</td></tr></table>",

                [(NombresDeCanal.Email, TiposDeEvento.Fallido)] =
                    "<h2 style=\"color:#B14343\">Despliegue fallido — {{Goc}}</h2>" +
                    "<table><tr><td>Etapa</td><td>{{Etapa}}</td></tr>" +
                    "<tr><td>Error</td><td>{{MensajeError}}</td></tr>" +
                    "<tr><td>Ambiente</td><td>{{Ambiente}}</td></tr>" +
                    "<tr><td>Sistemas</td><td>{{Sistemas}}</td></tr>" +
                    "<tr><td>Usuario responsable</td><td>{{UsuarioAplicacion}}</td></tr>" +
                    "<tr><td>Fecha y hora</td><td>{{FechaHora}}</td></tr></table>" +
                    "<p>Consulta el detalle completo en la pantalla Historial de GocDeployManager.</p>",

                [(NombresDeCanal.Teams, TiposDeEvento.Iniciado)] =
                    "{\"type\":\"message\",\"attachments\":[{\"contentType\":\"application/vnd.microsoft.card.adaptive\"," +
                    "\"content\":{\"type\":\"AdaptiveCard\",\"version\":\"1.4\",\"body\":[" +
                    "{\"type\":\"TextBlock\",\"text\":\"Despliegue iniciado — {{Goc}}\",\"weight\":\"Bolder\",\"size\":\"Medium\"}," +
                    "{\"type\":\"FactSet\",\"facts\":[" +
                    "{\"title\":\"Usuario\",\"value\":\"{{UsuarioAplicacion}}\"}," +
                    "{\"title\":\"Ambiente\",\"value\":\"{{Ambiente}}\"}," +
                    "{\"title\":\"Sistemas\",\"value\":\"{{Sistemas}}\"}," +
                    "{\"title\":\"Fecha\",\"value\":\"{{FechaHora}}\"}]}]}}]}",

                [(NombresDeCanal.Teams, TiposDeEvento.Exitoso)] =
                    "{\"type\":\"message\",\"attachments\":[{\"contentType\":\"application/vnd.microsoft.card.adaptive\"," +
                    "\"content\":{\"type\":\"AdaptiveCard\",\"version\":\"1.4\",\"body\":[" +
                    "{\"type\":\"TextBlock\",\"text\":\"Despliegue exitoso — {{Goc}}\",\"weight\":\"Bolder\",\"size\":\"Medium\",\"color\":\"Good\"}," +
                    "{\"type\":\"FactSet\",\"facts\":[" +
                    "{\"title\":\"Rama\",\"value\":\"{{Rama}}\"}," +
                    "{\"title\":\"Ambiente\",\"value\":\"{{Ambiente}}\"}," +
                    "{\"title\":\"Sistemas\",\"value\":\"{{Sistemas}}\"}," +
                    "{\"title\":\"Usuario\",\"value\":\"{{UsuarioAplicacion}}\"}," +
                    "{\"title\":\"Duración\",\"value\":\"{{Duracion}}\"}]}]}}]}",

                [(NombresDeCanal.Teams, TiposDeEvento.Fallido)] =
                    "{\"type\":\"message\",\"attachments\":[{\"contentType\":\"application/vnd.microsoft.card.adaptive\"," +
                    "\"content\":{\"type\":\"AdaptiveCard\",\"version\":\"1.4\",\"body\":[" +
                    "{\"type\":\"TextBlock\",\"text\":\"Despliegue fallido — {{Goc}}\",\"weight\":\"Bolder\",\"size\":\"Medium\",\"color\":\"Attention\"}," +
                    "{\"type\":\"FactSet\",\"facts\":[" +
                    "{\"title\":\"Etapa\",\"value\":\"{{Etapa}}\"}," +
                    "{\"title\":\"Error\",\"value\":\"{{MensajeError}}\"}," +
                    "{\"title\":\"Ambiente\",\"value\":\"{{Ambiente}}\"}," +
                    "{\"title\":\"Usuario\",\"value\":\"{{UsuarioAplicacion}}\"}]}]}}]}",
            };

        public static string Obtener(string canal, string tipoEvento)
        {
            if (Contenido.TryGetValue((canal, tipoEvento), out var plantilla))
                return plantilla;

            throw new ArgumentException($"No hay plantilla por defecto para canal='{canal}' tipoEvento='{tipoEvento}'.");
        }
    }
}

using System;
using System.Collections.Generic;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace GocDeployManager.Infrastructure.Sqlite
{
    public sealed class SqliteAppUserRepository : IAppUserRepository
    {
        private readonly string _cadenaConexion;

        public SqliteAppUserRepository(string cadenaConexion)
        {
            _cadenaConexion = Guard.ContraNuloOVacio(cadenaConexion, nameof(cadenaConexion));
            SqliteEsquema.Asegurar(_cadenaConexion);
        }

        public AppUser BuscarPorNombreUsuario(string nombreUsuario)
        {
            Guard.ContraNuloOVacio(nombreUsuario, nameof(nombreUsuario));

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT * FROM AppUser WHERE NombreUsuario = $nombreUsuario";
                comando.Parameters.AddWithValue("$nombreUsuario", nombreUsuario);

                using (var lector = comando.ExecuteReader())
                    return lector.Read() ? Mapear(lector) : null;
            }
        }

        public IReadOnlyList<AppUser> ObtenerTodos()
        {
            var resultado = new List<AppUser>();

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT * FROM AppUser ORDER BY NombreUsuario";
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        resultado.Add(Mapear(lector));
                }
            }

            return resultado.AsReadOnly();
        }

        public void Agregar(AppUser usuario)
        {
            Guard.ContraNulo(usuario, nameof(usuario));

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    INSERT INTO AppUser
                        (NombreUsuario, NombreVisible, Rol, Activo, HashContrasena, SalContrasena, UsuarioBitbucket, ContrasenaBitbucketProtegida)
                    VALUES
                        ($nombreUsuario, $nombreVisible, $rol, $activo, $hash, $sal, $usuarioBb, $passBb)";

                AgregarParametros(comando, usuario);
                comando.ExecuteNonQuery();
            }
        }

        public void Actualizar(AppUser usuario)
        {
            Guard.ContraNulo(usuario, nameof(usuario));

            using (var conexion = AbrirConexion())
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    UPDATE AppUser SET
                        NombreVisible = $nombreVisible,
                        Rol = $rol,
                        Activo = $activo,
                        HashContrasena = $hash,
                        SalContrasena = $sal,
                        UsuarioBitbucket = $usuarioBb,
                        ContrasenaBitbucketProtegida = $passBb
                    WHERE NombreUsuario = $nombreUsuario";

                AgregarParametros(comando, usuario);
                comando.ExecuteNonQuery();
            }
        }

        private SqliteConnection AbrirConexion()
        {
            var conexion = new SqliteConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }

        private static void AgregarParametros(SqliteCommand comando, AppUser usuario)
        {
            comando.Parameters.AddWithValue("$nombreUsuario", usuario.NombreUsuario);
            comando.Parameters.AddWithValue("$nombreVisible", usuario.NombreVisible);
            comando.Parameters.AddWithValue("$rol", usuario.Rol.ToString());
            comando.Parameters.AddWithValue("$activo", usuario.Activo ? 1 : 0);
            comando.Parameters.AddWithValue("$hash", usuario.HashContrasena);
            comando.Parameters.AddWithValue("$sal", usuario.SalContrasena);
            comando.Parameters.AddWithValue("$usuarioBb", (object)usuario.UsuarioBitbucket ?? DBNull.Value);
            comando.Parameters.AddWithValue("$passBb", (object)usuario.ContrasenaBitbucketProtegida ?? DBNull.Value);
        }

        private static AppUser Mapear(SqliteDataReader lector)
        {
            var rol = (RolUsuario)Enum.Parse(typeof(RolUsuario), (string)lector["Rol"]);

            var usuario = new AppUser(
                (string)lector["NombreUsuario"],
                (string)lector["NombreVisible"],
                rol,
                (string)lector["HashContrasena"],
                (string)lector["SalContrasena"]);

            if (Convert.ToInt32(lector["Activo"]) == 0)
                usuario.Desactivar();

            if (lector["UsuarioBitbucket"] != DBNull.Value)
                usuario.EstablecerCredencialesBitbucket((string)lector["UsuarioBitbucket"], (string)lector["ContrasenaBitbucketProtegida"]);

            return usuario;
        }
    }
}

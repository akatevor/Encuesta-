using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Encuesta.Models;
using Admin.Models;
namespace Encuesta.Data
{
    public class ConexionMySql
    {
        private readonly string connectionString;

        public ConexionMySql()
        {
            connectionString = "Server=localhost;Database=encuestaBD;User ID=root;Password=123qwe;Port=3306;SslMode=Preferred;";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // ======================= LOGIN =======================
        public Usuario ObtenerUsuario(string usuario, string contraseña)
        {
            Usuario user = null;

            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT idUSER, username, rol FROM user WHERE username=@usuario AND password=@password LIMIT 1";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@password", contraseña);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new Usuario
                                {
                                    Id = reader.GetInt32("idUSER"),
                                    Username = reader.GetString("username"),
                                    Rol = reader.GetString("rol")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener usuario: " + ex.Message);
            }

            return user;
        }

        // ======================= FACULTADES =======================
        public List<string> ObtenerFacultades()
        {
            List<string> facultades = new List<string>();

            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT nombre FROM FACULTAD";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            facultades.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error MySQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }

            return facultades;
        }

        // ======================= CIUDADES =======================
        public List<string> ObtenerCiudades()
        {
            List<string> ciudades = new List<string>();

            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT Municipio, Departamento FROM CIUDAD";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nombreCompleto = $"{reader.GetString(0)}, {reader.GetString(1)}";
                            ciudades.Add(nombreCompleto);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error MySQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }

            return ciudades;
        }

        // ======================= REGISTRO =======================
        public bool RegistrarUsuario(
            string nombre, string apellido, string facultad, string ciudad, string sexo,
            string username, string password)
        {
            try
            {
                using (var conn = this.GetConnection())
                {
                    conn.Open();

                    // 1. Obtener id de ciudad
                    string[] ciudadSplit = ciudad.Split(',');
                    int idCiudad = 0;
                    string queryCiudad = "SELECT idCIUDAD FROM CIUDAD WHERE Municipio=@municipio AND Departamento=@departamento LIMIT 1";
                    using (var cmd = new MySqlCommand(queryCiudad, conn))
                    {
                        cmd.Parameters.AddWithValue("@municipio", ciudadSplit[0].Trim());
                        cmd.Parameters.AddWithValue("@departamento", ciudadSplit[1].Trim());
                        var result = cmd.ExecuteScalar();
                        if (result != null) idCiudad = Convert.ToInt32(result);
                        else throw new Exception("Ciudad no encontrada");
                    }

                    // 2. Obtener id de facultad
                    int idFacultad = 0;
                    string queryFacultad = "SELECT idFACULTAD FROM FACULTAD WHERE nombre=@nombre LIMIT 1";
                    using (var cmd = new MySqlCommand(queryFacultad, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", facultad);
                        var result = cmd.ExecuteScalar();
                        if (result != null) idFacultad = Convert.ToInt32(result);
                        else throw new Exception("Facultad no encontrada");
                    }

                    // 3. Insertar en GENERALES
                    string insertGenerales = @"INSERT INTO GENERALES (nombre, apellido, sexo, CIUDAD_idCIUDAD, FACULTAD_idFACULTAD)
                                        VALUES (@nombre, @apellido, @sexo, @ciudad, @facultad);
                                        SELECT LAST_INSERT_ID();";
                    int idGenerales = 0;
                    using (var cmd = new MySqlCommand(insertGenerales, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@apellido", apellido);
                        cmd.Parameters.AddWithValue("@sexo", sexo);
                        cmd.Parameters.AddWithValue("@ciudad", idCiudad);
                        cmd.Parameters.AddWithValue("@facultad", idFacultad);

                        idGenerales = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 4. Insertar en usuario (asumiendo encuesta_completada DEFAULT 0 en la tabla user)
                    string insertUsuario = @"INSERT INTO user
                                        (username, password, rol, GENERALES_idGENERALES, GENERALES_CIUDAD_idCIUDAD, GENERALES_FACULTAD_idFACULTAD)
                                        VALUES (@username, @password, 'USER', @idGenerales, @idCiudad, @idFacultad)";
                    using (var cmd = new MySqlCommand(insertUsuario, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@idGenerales", idGenerales);
                        cmd.Parameters.AddWithValue("@idCiudad", idCiudad);
                        cmd.Parameters.AddWithValue("@idFacultad", idFacultad);

                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al registrar usuario: " + ex.Message);
                return false;
            }
        }

        // ======================= ENCUESTA (SIN user_id) =======================
        public bool GuardarRespuesta(int userId, string[] respuestas)
        {
            try
            {
                if (respuestas == null || respuestas.Length != 10)
                {
                    Console.WriteLine("❌ ERROR: Se deben enviar exactamente 10 respuestas");
                    return false;
                }

                using (var conn = this.GetConnection())
                {
                    conn.Open();

                    // 1) Verificar que existe el usuario
                    string checkUserQuery = "SELECT COUNT(*) FROM user WHERE idUSER=@userId";
                    using (var checkUserCmd = new MySqlCommand(checkUserQuery, conn))
                    {
                        checkUserCmd.Parameters.AddWithValue("@userId", userId);
                        int existe = Convert.ToInt32(checkUserCmd.ExecuteScalar());
                        if (existe == 0)
                        {
                            Console.WriteLine("❌ Usuario no existe");
                            return false;
                        }
                    }

                    // 2) Verificar si ya marcó encuesta_completada
                    string checkFlagQuery = "SELECT encuesta_completada FROM user WHERE idUSER=@userId";
                    using (var checkFlagCmd = new MySqlCommand(checkFlagQuery, conn))
                    {
                        checkFlagCmd.Parameters.AddWithValue("@userId", userId);
                        var flagObj = checkFlagCmd.ExecuteScalar();
                        int flag = (flagObj == null || flagObj == DBNull.Value) ? 0 : Convert.ToInt32(flagObj);
                        if (flag == 1)
                        {
                            Console.WriteLine("⚠️ Usuario ya había completado la encuesta");
                            return false;
                        }
                    }

                    // 3) Insertar SOLO respuestas en ENCUESTA (sin user_id)
                    string insertQuery = @"INSERT INTO ENCUESTA 
                                           (`0`,`1`,`2`,`3`,`4`,`5`,`6`,`7`,`8`,`9`)
                                           VALUES 
                                           (@r0,@r1,@r2,@r3,@r4,@r5,@r6,@r7,@r8,@r9)";

                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            string valor = string.IsNullOrWhiteSpace(respuestas[i]) ? "NUNCA" : respuestas[i];
                            cmd.Parameters.AddWithValue($"@r{i}", valor);
                        }

                        int rows = cmd.ExecuteNonQuery();
                        if (rows <= 0) return false;
                    }

                    // 4) Marcar al usuario como que ya completó la encuesta
                    string updateUser = "UPDATE user SET encuesta_completada = 1 WHERE idUSER=@userId";
                    using (var up = new MySqlCommand(updateUser, conn))
                    {
                        up.Parameters.AddWithValue("@userId", userId);
                        int upd = up.ExecuteNonQuery();
                        return upd > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR en GuardarRespuesta: " + ex.Message);
                return false;
            }
        }

        public bool EncuestaCompletada(int userId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    // SIN relación en ENCUESTA: consultamos solo el flag en la tabla user
                    string query = "SELECT encuesta_completada FROM user WHERE idUSER=@userId LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        var result = cmd.ExecuteScalar();
                        int flag = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                        return flag == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al verificar encuesta: " + ex.Message);
                return false;
            }
        }

        public string[] ObtenerRespuestas(int userId)
        {
            // SIN user_id en ENCUESTA: devolvemos la última encuesta registrada (global)
            string[] respuestas = new string[10];

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT `0`,`1`,`2`,`3`,`4`,`5`,`6`,`7`,`8`,`9` FROM ENCUESTA ORDER BY idENCUESTA DESC LIMIT 1";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 0; i < 10; i++)
                                respuestas[i] = reader.IsDBNull(i) ? "" : reader.GetString(i);
                        }
                        else
                        {
                            for (int i = 0; i < 10; i++) respuestas[i] = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener respuestas: " + ex.Message);
                for (int i = 0; i < 10; i++) respuestas[i] = "";
            }

            return respuestas;
        }

        public List<EncuestaModelo> ObtenerEncuestas(string search = "")
        {
            var lista = new List<EncuestaModelo>();
            using (var conn = GetConnection())
            {
                conn.Open();

                // Solo seleccionamos todo de ENCUESTA
                var query = "SELECT * FROM ENCUESTA";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var encuesta = new EncuestaModelo
                            {
                                Id = reader.GetInt32("idEncuesta"),
                                // Ponemos "ANÓNIMO" porque no mostramos quién hizo la encuesta
                                UserName = "ANÓNIMO",
                                Respuestas = new string[10]
                                {
                            reader.GetString("0"),
                            reader.GetString("1"),
                            reader.GetString("2"),
                            reader.GetString("3"),
                            reader.GetString("4"),
                            reader.GetString("5"),
                            reader.GetString("6"),
                            reader.GetString("7"),
                            reader.GetString("8"),
                            reader.GetString("9"),
                                }
                            };
                            lista.Add(encuesta);
                        }
                    }
                }
            }
            return lista;
        }



        public bool EliminarEncuesta(int idEncuesta)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var query = "DELETE FROM ENCUESTA WHERE idEncuesta = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idEncuesta);
                    int afectados = cmd.ExecuteNonQuery();
                    return afectados > 0;
                }
            }
        }
    }
}

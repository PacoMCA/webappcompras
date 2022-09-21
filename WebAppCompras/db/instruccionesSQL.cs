using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Configuration;

namespace WebAppCompras.db
{
    public class instruccionesSQL
    {
        public static SqlConnection conectar()
        {
            //UtilityObjects.Log($"instruccionesSQL.conectar()");
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["appCompras"].ConnectionString;
            //Console.WriteLine("Abriendo conexión con BDs ...");
            Console.WriteLine(conn.ToString());
            return conn;
        }

        public static SqlConnection conectar_login()
        {
            //GlobalObjects.UtilityObjects.Log($"instruccionesSQL.conectar_login()");
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["login"].ConnectionString;
            //Console.WriteLine("Abriendo conexión con BDs ...");
            //Console.WriteLine(conn.ToString());
            return conn;
        }

        public static DataSet EjecutarSelect(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EjecutarSelect({query})");
            SqlConnection conexion = conectar_login();
            conexion.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(query, conectar_login());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            conexion.Close();
            return ds;
        }

        public static DataSet EjecutarSelect2(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EjecutarSelect2({query})");
            SqlConnection conexion = conectar();
            conexion.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(query, conectar());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            conexion.Close();
            return ds;
        }

        public static DataSet EjecutarSelectBusqueda(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EjecutarSelectBusqueda({query})");
            SqlConnection conexion = conectar_login();
            conexion.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(query, conectar());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            conexion.Close();
            return ds;
        }

        public static IDictionary<string, Object> EjecutarSelectCatchingError(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EjecutarSelectCatchingError({query})");
            var objRespuesta = new ExpandoObject() as IDictionary<string, Object>;
            objRespuesta["success"] = true;

            try
            {
                SqlConnection conexion = conectar_login();
                conexion.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, conectar_login());
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                conexion.Close();
                objRespuesta.Add("ds", ds);
                UtilityObjects.Log($"instruccionesSQL.EjecutarSelectCatchingError.Return({JsonConvert.SerializeObject(objRespuesta)})");
                return objRespuesta;
            }
            catch (Exception e)
            {
                objRespuesta["success"] = false;
                objRespuesta.Add("Error SQL : ", e.Message);
                return objRespuesta;
            }
        }

        public static IDictionary<string, Object> EjecutarSelectCatching(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EjecutarSelectCatching({query})");
            var objRespuesta = new ExpandoObject() as IDictionary<string, Object>;
            objRespuesta["success"] = true;

            try
            {
                SqlConnection conexion = conectar();
                conexion.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, conectar());
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                conexion.Close();
                objRespuesta.Add("ds", ds);
                UtilityObjects.Log($"instruccionesSQL.EjecutarSelectCatching.Response({JsonConvert.SerializeObject(objRespuesta)})");
                return objRespuesta;
            }
            catch (Exception e)
            {
                objRespuesta["success"] = false;
                objRespuesta.Add("Error SQL : ", e.Message);
                UtilityObjects.Log($"instruccionesSQL.EjecutarSelectCatching.Error({e.Message})");
                return objRespuesta;
            }
        }
        public static bool InsertarFila(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.InsertarFila({query})");
            try
            {
                SqlConnection conexion = conectar();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Dispose();
                conexion.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool EditarFila(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EditarFila({query})");
            try
            {
                SqlConnection conexion = conectar();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Dispose();
                conexion.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string SQLCommand(string query, string b)
        {
            UtilityObjects.Log($"instruccionesSQL.SQLCommand({query},{b})");
            try
            {
                SqlConnection conexion = conectar_login();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                command.Parameters.Add("@id", SqlDbType.Int, 4).Direction = ParameterDirection.Output;  // Indica que devuelva el Id de lo insertado
                command.ExecuteNonQuery();

                var idDevuelto = command.Parameters["@id"].Value;    // Recuperar el Id de lo insertado

                command.Dispose();
                conexion.Close();

                //return new { success = "true", payload = idDevuelto };
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string SQLCommand(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.SQLCommand({query})");
            try
            {
                SqlConnection conexion = conectar();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                command.Parameters.Add("@id", SqlDbType.Int, 4).Direction = ParameterDirection.Output;  // Indica que devuelva el Id de lo insertado
                command.ExecuteNonQuery();

                var idDevuelto = command.Parameters["@id"].Value;    // Recuperar el Id de lo insertado

                command.Dispose();
                conexion.Close();

                //return new { success = "true", payload = idDevuelto };
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static object SQLCommandSPidentity(string storedProcedure, JObject objToInsert)
        {
            UtilityObjects.Log($"instruccionesSQL.SQLCommandSPidentity({storedProcedure},{objToInsert})");
            try
            {
                IDictionary<string, Object> response = new Dictionary<string, Object>();

                SqlConnection conn = conectar();
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    var props = objToInsert.Properties();
                    foreach (JProperty property in props)
                    {
                        var propName = property.Name;
                        var propValue = property.Value;
                        cmd.Parameters.Add("@" + property.Name, SqlDbType.VarChar, 500);
                        cmd.Parameters["@" + property.Name].Value = property.Value;
                    }
                    // open connection and execute stored procedure
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    var idDevuelto = cmd.Parameters["@id"].Value;
                    cmd.Dispose();
                    conn.Close();
                    response.Add("success", "true");
                    response.Add("payload", idDevuelto);
                    return response;

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static object SQLCommandReturnsIdentity(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.SQLCommandReturnsIdentity({query})");
            try
            {
                IDictionary<string, Object> response = new Dictionary<string, Object>();

                SqlConnection conexion = conectar();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                // command.Parameters.Add("@id", SqlDbType.Int, 4).Direction = ParameterDirection.Output;  // Indica que devuelva el Id de lo insertado                
                command.Parameters.Add("@id", SqlDbType.VarChar, 250);
                command.Parameters["@id"].Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();

                //var idDevuelto = command.Parameters["@id"].Value;    // Recuperar el Id de lo insertado
                var idDevuelto = command.Parameters["@id"].Value.ToString();    // Recuperar el Id de lo insertado

                command.Dispose();
                conexion.Close();

                response.Add("success", "true");
                response.Add("payload", idDevuelto);
                return response;
                //return new { success = "true", payload = idDevuelto };
                //return "true";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static bool EliminarFila(string query)
        {
            UtilityObjects.Log($"instruccionesSQL.EliminarFila({query})");
            try
            {
                SqlConnection conexion = conectar();
                SqlCommand command = new SqlCommand(query, conexion);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Dispose();
                conexion.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static String RegresaUnaCadena(string StoredProcedure)
        {
            UtilityObjects.Log($"instruccionesSQL.RegresaUnaCadena({StoredProcedure})");
            String StrResultado = string.Empty;
            SqlDataReader SqlReader;
            SqlConnection Cnx = new SqlConnection(conectar().ConnectionString.ToString());
            SqlCommand Cmnd = new SqlCommand(StoredProcedure, Cnx);
            Cmnd.CommandType = CommandType.StoredProcedure;

            Cnx.Open();
            SqlReader = Cmnd.ExecuteReader();
            while (SqlReader.Read())
            {
                StrResultado = SqlReader[0].ToString();
            }
            SqlReader.Close();
            Cnx.Close();
            Cnx.Dispose();
            Cmnd.Dispose();

            return StrResultado;
        }

        public static Int64 RegresaUnEntero(string StoredProcedure)
        {
            UtilityObjects.Log($"instruccionesSQL.RegresaUnEntero({StoredProcedure})");
            Int64 EnteroResultado = 0;
            SqlDataReader SqlReader;
            SqlConnection Cnx = new SqlConnection(conectar().ConnectionString.ToString());
            SqlCommand Cmnd = new SqlCommand(StoredProcedure, Cnx);
            Cmnd.CommandType = CommandType.Text;
            Cnx.Open();
            SqlReader = Cmnd.ExecuteReader();
            while (SqlReader.Read())
            {
                EnteroResultado = Convert.ToInt32(SqlReader[0]);
            }
            SqlReader.Close();
            Cnx.Close();
            Cnx.Dispose();
            Cmnd.Dispose();

            return EnteroResultado;
        }
    }
}
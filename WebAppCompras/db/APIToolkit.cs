using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace WebAppCompras.db
{
    public class APIToolkit
    {
		public static IDictionary<string, Object> FormatData(string PropName, string ModelDataType, string value)
		{
			var objResponse = new ExpandoObject() as IDictionary<string, Object>;
			objResponse.Add("success", true);
			if (ModelDataType == "DateTime")
			{
				string dateTime = value;
				try
				{
					if (dateTime == null || dateTime == string.Empty)
					{
						dateTime = null;
					}
					else
					{
						DateTime dt = Convert.ToDateTime(dateTime);
						string dateString = dt.ToString("yyyy-MM-dd HH:mm:ss");
						objResponse.Add("value", dateString);
						return objResponse;
					}
				}
				catch (Exception ex)
				{
					objResponse["success"] = false;
					objResponse.Add("error ", "El campo " + PropName + " No tiene formato FechaHora correcto");
					return objResponse;

				}

			}
			if (ModelDataType == "String")
			{
				objResponse.Add("value", value);
				return objResponse;
			}
			if (ModelDataType == "Boolean")
			{
				if (value == "True")
				{
					value = "1";

				}
				if (value == "False")
				{
					value = "0";
				}
				if (value == "0" || value == "1")
				{
					objResponse.Add("value", value);
					return objResponse;

				}
				else
				{
					objResponse["success"] = false;
					objResponse.Add("e", "El campo " + PropName + " Debe valer  'true/ 1' Ó 'false/0'");
					return objResponse;
				}

			}
			else
			{
				objResponse.Add("value", value);
				return objResponse;
			}
		}

		public static IDictionary<string, Object> QueryInsertFromObject(string table, JObject objToInsert, object model)
		{
			//string queryInsertFormat = "insert into @table (@fieldsList) values(@values)";
			//string queryInsertFormat = "insert into @table (@fieldsList) output inserted.id values(@values)";
			//string queryInsertFormat = "insert into @table (@fieldsList) values(@values) set @id = SCOPE_IDENTITY()";   // Funciona
			//string queryInsertFormat = "insert into @table (@fieldsList) values(@values) set @id = OUR_FUNCTION()";
			string queryInsertFormat = "declare @outputVar varchar(max); " +
									   "exec sp_getComputedIdFromInserted @tabInput = '" + table + "', " +
																		 "@fLInput = '@fieldsList' , " +
																		 "@valInput = @values, " +
																		 "@respuestaOutput = @outputVar OUTPUT " +
																		 "set @id = @outputVar";

			string fieldsList = "";
			string values = "";
			try
			{
				var props = objToInsert.Properties();
				var r = "";
				var index = 0;
				var warnings = 0;
				var errors = 0;
				var objResultado = getModelSchemma(model, objToInsert);
				// objResultado.Add("success", true);
				if ((bool)objResultado["success"] == false) //verifica requeridos 
				{
					return objResultado;
				}
				else
				{
					foreach (JProperty property in props)
					{
						var propName = property.Name;
						var propValue = property.Value;
						var modelProp = model.GetType().GetProperty(propName);

						if (modelProp == null)
						{
							objResultado.Add("warning " + warnings, "No se reconoce el campo " + propName);
							warnings++;
						}
						else
						{
							AttributeCollection attributes = TypeDescriptor.GetProperties(model)[propName].Attributes;
							ReadOnlyAttribute myAttribute = (ReadOnlyAttribute)attributes[typeof(ReadOnlyAttribute)];

							if (myAttribute.IsReadOnly)
							{

								objResultado.Add("warning " + warnings, "El campo " + propName + " es de solo lectura ");
								warnings++;

							}
							else
							{
								// if (propValue == null || propValue.ToString() == String.Empty) // si el valor de la propiedad  es nulo o vacio
								if (propValue == null) // si el valor de la propiedad  es nulo o vacio
								{
									objResultado.Add("warning " + warnings, "El campo  " + propName + "es nulo o vacío");
									warnings++;
								}
								else// si todo esta bien para esta propiedad
								{
									fieldsList += propName + ",";
									//values += "'" +  propValue.Type +"'" + ",";
									var formatoValor = FormatData(propName, modelProp.PropertyType.Name, propValue.ToString());

									if ((bool)formatoValor["success"] == true)
									{
										if (modelProp.PropertyType.Name == "DateTime")
										{
											values += "''" + propValue + "''" + ",";
										}
										else
										{
											string valorFormateado = (string)formatoValor["value"];
											// values += "'" + valorFormateado + "'" + ",";    
											values += "''" + valorFormateado + "''" + ",";
										}
									}
									else
									{
										objResultado["success"] = false;
										string errorMsg = (string)formatoValor["e"];
										objResultado.Add("error " + errors, errorMsg);
										errors++;
									}




								}

							}
						}

						index++;
					}

					if (fieldsList == String.Empty)
					{
						objResultado["sucess"] = false;
						objResultado.Add("error " + errors, "No se reconocieron campos para insertar");
					}
					else
					{
						fieldsList = fieldsList.Remove(fieldsList.Length - 1);  // Quitar última coma
						values = values.Remove(values.Length - 1);              // Quitar última coma
						values = values.Insert(0, "'");                         // Colocar ' al inicio
						values = values.Insert(values.Length, "'");             // Colocar ' al final
						var query = queryInsertFormat
						  .Replace("@table", table)
						  .Replace("@fieldsList", fieldsList)
						  .Replace("@values", values);

						objResultado.Add("query", query);

					}

					return objResultado;

				}
			}
			catch (Exception)
			{
				var objError = new ExpandoObject() as IDictionary<string, Object>;
				objError.Add("error", "No se reconoce el objeto enviado");
				objError.Add("success", false);
				return objError;
			}


		}
		public static IDictionary<string, Object> QueryInsertFromObjectSP(string storedProcedure, JObject objToInsert, object model)
		{
			string queryInsertFormat = storedProcedure + " fieldsList";
			//string queryInsertFormat = "sp_insertaProcesoConCPA @idCPA = 'cpa9', 
			//                                @s_nombreProceso = 'Proceso de prueba de stored procedure', 
			//              @s_tipoProceso = 'cambio', 
			//              @s_descripcion = 'Una descripción por decir un ejemplo', 
			//              @n_ticketsIM = 6";
			//string queryInsertFormat = "insert into @table (@fieldsList) values(@values)";
			string fieldsList = "";
			string values = "";
			try
			{
				var props = objToInsert.Properties();
				var r = "";
				var index = 0;
				var warnings = 0;
				var errors = 0;
				var objResultado = getModelSchemma(model, objToInsert);
				// objResultado.Add("success", true);
				if ((bool)objResultado["success"] == false) //verifica requeridos 
				{
					return objResultado;
				}
				else
				{
					foreach (JProperty property in props)
					{
						var propName = property.Name;
						var propValue = property.Value;
						var modelProp = model.GetType().GetProperty(propName);

						if (modelProp == null)
						{
							objResultado.Add("warning " + warnings, "No se reconoce el campo " + propName);
							warnings++;
						}
						else
						{
							AttributeCollection attributes = TypeDescriptor.GetProperties(model)[propName].Attributes;
							ReadOnlyAttribute myAttribute = (ReadOnlyAttribute)attributes[typeof(ReadOnlyAttribute)];

							if (myAttribute.IsReadOnly)
							{

								objResultado.Add("warning " + warnings, "El campo " + propName + " es de solo lectura ");
								warnings++;

							}
							else
							{
								if (propValue == null || propValue.ToString() == String.Empty) // si el valor de la propiedad  es nulo o vacio
								{
									objResultado.Add("warning " + warnings, "El campo  " + propName + "es nulo o vacío");
									warnings++;
								}
								else// si todo esta bien para esta propiedad
								{
									// fieldsList += propName + ",";
									//values += "'" +  propValue.Type +"'" + ",";
									var formatoValor = FormatData(propName, modelProp.PropertyType.Name, propValue.ToString());

									if ((bool)formatoValor["success"] == true)
									{
										string valorFormateado = (string)formatoValor["value"];
										values += "'" + valorFormateado + "'" + ",";
										fieldsList += '@' + propName + "= '" + valorFormateado + "',";
									}
									else
									{
										objResultado["success"] = false;
										string errorMsg = (string)formatoValor["e"];
										objResultado.Add("error " + errors, errorMsg);
										errors++;
									}

								}

							}
						}

						index++;
					}

					if (fieldsList == String.Empty)
					{
						objResultado["sucess"] = false;
						objResultado.Add("error " + errors, "No se reconocieron campos para insertar");
					}
					else
					{
						fieldsList = fieldsList.Remove(fieldsList.Length - 1);
						values = values.Remove(values.Length - 1);
						var query = queryInsertFormat
						  .Replace("fieldsList", fieldsList);

						objResultado.Add("query", query);

					}

					return objResultado;

				}
			}
			catch (Exception)
			{
				var objError = new ExpandoObject() as IDictionary<string, Object>;
				objError.Add("error", "No se reconoce el objeto enviado");
				objError.Add("success", false);
				return objError;
			}


		}
		public static IDictionary<string, Object> QueryUpdateFromObject(string table, JObject objToUpdate, object model, string idColumnName)
		{

			string queryUpdateFormat = "update @table set @updateFields where @conditions";
			string updateFields = "";
			string conditions = "";
			try
			{
				var props = objToUpdate.Properties();
				var index = 0;
				var warnings = 1;
				var errors = 1;
				var objResultado = new ExpandoObject() as IDictionary<string, Object>;
				objResultado.Add("success", true);
				foreach (JProperty property in props)
				{
					var propName = property.Name;
					var propValue = property.Value;
					var modelProp = model.GetType().GetProperty(propName);


					if (modelProp == null)
					{
						objResultado.Add("warning " + warnings, "No se reconoce el campo " + propName);
						warnings++;
					}
					else
					{
						AttributeCollection attributes = TypeDescriptor.GetProperties(model)[propName].Attributes;
						ReadOnlyAttribute myAttribute = (ReadOnlyAttribute)attributes[typeof(ReadOnlyAttribute)];
						if (propName == idColumnName)
						{
							conditions += propName + "=" + "'" + propValue + "'";
						}
						else if (myAttribute.IsReadOnly)
						{

							objResultado.Add("warning " + warnings, "El campo " + propName + " es de solo lectura ");
							warnings++;

						}
						else
						{
							//if (propValue == null || propValue.ToString() == String.Empty) // si el valor de la propiedad  es nulo o vacio
							//{
							//    objResultado.Add("warning " + warnings, "El campo  " + propName + "es nulo o vacío");
							//    warnings++;
							//}
							//else// si todo esta bien para esta propiedad
							//{
							var formatoValor = FormatData(propName, modelProp.PropertyType.Name, propValue.ToString());

							if ((bool)formatoValor["success"] == true)
							{
								string valorFormateado = (string)formatoValor["value"];
								updateFields += propName + " = " + "'" + valorFormateado + "'" + ",";
							}
							else
							{
								objResultado["success"] = false;
								string errorMsg = (string)formatoValor["e"];
								objResultado.Add("error " + errors, errorMsg);
								errors++;
							}




							//}

						}
					}

					index++;
				}
				if (updateFields == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "No se reconoció ningun campo para modificar");
					errors++;
				}
				else
				{
					updateFields = updateFields.Remove(updateFields.Length - 1);
				}

				if (conditions == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "El campo " + idColumnName + " es indispensable para hacer la actualización");
				}
				else
				{
					var query = queryUpdateFormat
							 .Replace("@table", table)
							 .Replace("@updateFields", updateFields)
							 .Replace("@conditions", conditions);

					objResultado.Add("query", query);

				}
				return objResultado;

			}
			catch (Exception)
			{
				var objError = new ExpandoObject() as IDictionary<string, Object>;
				objError.Add("error", "No se reconoce el objeto enviado");
				objError.Add("success", false);
				return objError;
			}


		}
		public static IDictionary<string, Object> QueryUpdateFromObjectWithNulls(string table, JObject objToUpdate, object model, string idColumnName)
		{

			string queryUpdateFormat = "update @table set @updateFields where @conditions";
			string updateFields = "";
			string conditions = "";
			try
			{
				var props = objToUpdate.Properties();
				var index = 0;
				var warnings = 1;
				var errors = 1;
				var objResultado = new ExpandoObject() as IDictionary<string, Object>;
				objResultado.Add("success", true);
				foreach (JProperty property in props)
				{
					var propName = property.Name;
					var propValue = property.Value;
					var modelProp = model.GetType().GetProperty(propName);


					if (modelProp == null)
					{
						objResultado.Add("warning " + warnings, "No se reconoce el campo " + propName);
						warnings++;
					}
					else
					{
						AttributeCollection attributes = TypeDescriptor.GetProperties(model)[propName].Attributes;
						ReadOnlyAttribute myAttribute = (ReadOnlyAttribute)attributes[typeof(ReadOnlyAttribute)];
						if (propName == idColumnName)
						{
							conditions += propName + "=" + "'" + propValue + "'";
						}
						else if (myAttribute.IsReadOnly)
						{

							objResultado.Add("warning " + warnings, "El campo " + propName + " es de solo lectura ");
							warnings++;

						}
						else
						{

							var formatoValor = FormatData(propName, modelProp.PropertyType.Name, propValue.ToString());

							if ((bool)formatoValor["success"] == true)
							{
								string valorFormateado = (string)formatoValor["value"];
								updateFields += propName + " = " + "'" + valorFormateado + "'" + ",";
							}
							else
							{
								objResultado["success"] = false;
								string errorMsg = (string)formatoValor["e"];
								objResultado.Add("error " + errors, errorMsg);
								errors++;
							}






						}
					}

					index++;
				}
				if (updateFields == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "No se reconoció ningun campo para modificar");
					errors++;
				}
				else
				{
					updateFields = updateFields.Remove(updateFields.Length - 1);
				}

				if (conditions == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "El campo " + idColumnName + " es indispensable para hacer la actualización");
				}
				else
				{
					var query = queryUpdateFormat
							 .Replace("@table", table)
							 .Replace("@updateFields", updateFields)
							 .Replace("@conditions", conditions);

					objResultado.Add("query", query);

				}
				return objResultado;

			}
			catch (Exception)
			{
				var objError = new ExpandoObject() as IDictionary<string, Object>;
				objError.Add("error", "No se reconoce el objeto enviado");
				objError.Add("success", false);
				return objError;
			}


		}
		public static IDictionary<string, Object> QueryUpdateFromObject(string table, JObject objToUpdate, object model, NameValueCollection ReceivedQueryStringFields)
		{
			string queryUpdateFormat = "update @table set @updateFields where @conditions";
			string updateFields = "";
			string conditions = "";

			try
			{
				var props = objToUpdate.Properties();
				var warnings = 1;
				var errors = 1;
				var objResultado = new ExpandoObject() as IDictionary<string, Object>;
				objResultado.Add("success", true);
				int numParametros = ReceivedQueryStringFields.Count;
				int indice = 1;

				// Form conditions
				foreach (var key in ReceivedQueryStringFields.AllKeys)
				{
					var keyName = key;
					var keyValue = ReceivedQueryStringFields[key];
					var modelProp = model.GetType().GetProperty(keyName);

					if (modelProp == null)
					{
						objResultado["success"] = false;
						objResultado.Add("error " + errors, "No se reconoce el campo '" + keyName + "' enviado en la URL");
						errors++;
					}
					else
					{
						conditions += keyName + "=" + "'" + keyValue + "'";
						if (indice < numParametros)
						{
							conditions += " AND ";
						}
					}

					indice++;
				}

				foreach (JProperty property in props)
				{
					var propName = property.Name;
					var propValue = property.Value;
					var modelProp = model.GetType().GetProperty(propName);


					if (modelProp == null)
					{
						objResultado.Add("warning " + warnings, "No se reconoce el campo " + propName);
						warnings++;
					}
					else
					{
						AttributeCollection attributes = TypeDescriptor.GetProperties(model)[propName].Attributes;
						ReadOnlyAttribute myAttribute = (ReadOnlyAttribute)attributes[typeof(ReadOnlyAttribute)];
						if (myAttribute.IsReadOnly)
						{

							objResultado.Add("warning " + warnings, "El campo " + propName + " es de solo lectura ");
							warnings++;

						}
						else
						{
							if (propValue == null || propValue.ToString() == String.Empty) // si el valor de la propiedad  es nulo o vacio
							{
								objResultado.Add("warning " + warnings, "El campo  " + propName + "es nulo o vacío");
								warnings++;
							}
							else// si todo esta bien para esta propiedad
							{
								var formatoValor = FormatData(propName, modelProp.PropertyType.Name, propValue.ToString());

								if ((bool)formatoValor["success"] == true)
								{
									string valorFormateado = (string)formatoValor["value"];
									updateFields += propName + " = " + "'" + valorFormateado + "'" + ",";
								}
								else
								{
									objResultado["success"] = false;
									string errorMsg = (string)formatoValor["e"];
									objResultado.Add("error " + errors, errorMsg);
									errors++;
								}
							}

						}
					}

				}

				if (updateFields == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "No se reconoció ningun campo para modificar");
					errors++;
				}
				else
				{
					updateFields = updateFields.Remove(updateFields.Length - 1);
				}

				if (conditions == String.Empty)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "Es indispensable enviar una condición de búsqueda para poder realizar la actualización");
					// conditions = "1=1";
				}
				else
				{
					var query = queryUpdateFormat
							 .Replace("@table", table)
							 .Replace("@updateFields", updateFields)
							 .Replace("@conditions", conditions);

					objResultado.Add("query", query);

				}
				return objResultado;

			}
			catch (Exception)
			{
				var objError = new ExpandoObject() as IDictionary<string, Object>;
				objError.Add("error", "No se reconoce el objeto enviado");
				objError.Add("success", false);
				return objError;
			}


		}


		public static IDictionary<string, Object> QueryDeleteFromQueryString(string table, object model, NameValueCollection ReceivedQueryStringFields)
		{

			string queryDeleteFormat = "delete from @table where @conditions";
			string conditions = "";
			var warnings = 1;
			var errors = 1;
			var objResultado = new ExpandoObject() as IDictionary<string, Object>;
			objResultado.Add("success", true);
			int numParametros = ReceivedQueryStringFields.Count;
			int indice = 1;
			//----------------------------
			foreach (var key in ReceivedQueryStringFields.AllKeys)
			{
				var keyName = key;
				var keyValue = ReceivedQueryStringFields[key];
				var modelProp = model.GetType().GetProperty(keyName);

				if (modelProp == null)
				{
					objResultado["success"] = false;
					objResultado.Add("error " + errors, "No se reconoce el campo '" + keyName + "' enviado en la URL");
					errors++;
				}
				else
				{
					conditions += keyName + "=" + "'" + keyValue + "'";
					if (indice < numParametros)
					{
						conditions += " AND ";
					}
				}

				indice++;
			}

			if (conditions == String.Empty)
			{
				objResultado["success"] = false;
				objResultado.Add("error " + errors, "Es indispensable enviar una condición de búsqueda para poder eliminar");
				// conditions = "1=1";
			}
			else
			{
				var query = queryDeleteFormat
						 .Replace("@table", table)
						 .Replace("@conditions", conditions);

				objResultado.Add("query", query);

			}
			return objResultado;



		}
		public static IDictionary<string, Object> getModelSchemma(object model, JObject objToAnalize)
		{

			var objResultado = new ExpandoObject() as IDictionary<string, Object>;
			var ObjShema = new ExpandoObject() as IDictionary<string, Object>;
			objResultado.Add("success", true);


			var props = objToAnalize.Properties();
			var modelProperties = model.GetType().GetProperties();

			var i = 0;
			var errores = 0;
			foreach (var prop in modelProperties)
			{

				var column_name = prop.Name;
				var tipo_dato = prop.PropertyType.Name;
				string obligatorio = "";
				//var obligatorio = dataSetResult.Tables[0].Rows[i]["OBLIGATORIO"].ToString();
				AttributeCollection attributes = TypeDescriptor.GetProperties(model)[column_name].Attributes;
				RequiredAttribute required = (RequiredAttribute)attributes[typeof(RequiredAttribute)];
				if (required != null)
				{
					obligatorio = "obligatorio";
				}

				if (obligatorio == "obligatorio")
				{

					if (objToAnalize.Property(column_name) == null)
					{ // no esta mencionada la propiedad en el objeto enviado

						objResultado["success"] = false;
						objResultado.Add("error " + errores, "El campo " + column_name + " es obligatorio");
						errores++;

						//return "error falta propiedad";
					}
					else if (objToAnalize.Property(column_name).Value == null)// el valor de la propiedad es nullo
					{
						objResultado["success"] = false;
						objResultado.Add("error " + i, "El campo " + column_name + " es obligatorio");
						errores++;

						//return "error falta propiedad";
					}
					column_name = "*" + column_name;

				}
				ObjShema.Add(column_name, tipo_dato);
				i++;
			}
			if ((bool)objResultado["success"] == false)
			{
				objResultado.Add("esquema", ObjShema);
			}

			return objResultado;
		}
		public static IDictionary<string, Object> getSQLSchemma(string table, JObject objToAnalize)
		{

			var queryString = "sp_getSchema '" + table + "'";
			DataSet dataSetResult = instruccionesSQL.EjecutarSelect(queryString);
			var objResultado = new ExpandoObject() as IDictionary<string, Object>;
			var ObjShema = new ExpandoObject() as IDictionary<string, Object>;
			objResultado.Add("success", true);

			var props = objToAnalize.Properties();

			for (int i = 0; i <= dataSetResult.Tables[0].Rows.Count - 1; i++)
			{
				var obligatorio = dataSetResult.Tables[0].Rows[i]["OBLIGATORIO"].ToString();
				var column_name = dataSetResult.Tables[0].Rows[i]["COLUMN_NAME"].ToString();
				var tipo_dato = dataSetResult.Tables[0].Rows[i]["DATA_TYPE"].ToString();
				if (obligatorio == "obligatorio")
				{

					if (objToAnalize.Property(column_name) == null)
					{ // no esta mencionada la propiedad en el objeto enviado

						objResultado["success"] = false;
						objResultado.Add("error " + i, "El campo " + column_name + " es obligatorio");

						//return "error falta propiedad";
					}
					else if (objToAnalize.Property(column_name).Value == null)// el valor de la propiedad es nullo
					{
						objResultado["success"] = false;
						objResultado.Add("error " + i, "El campo " + column_name + " es obligatorio");

						//return "error falta propiedad";
					}
					column_name = "*" + column_name;

				}
				ObjShema.Add(column_name, tipo_dato);
			}
			if ((bool)objResultado["success"] == false)
			{
				objResultado.Add("esquema", ObjShema);
			}

			return objResultado;
		}
		public static string Where(NameValueCollection ReceivedQueryStringFields, object model)
		{
			var httpReceivedQueryDict = ReceivedQueryStringFields;

			IList<string> listaDeProps = new List<string>();
			string nombreLlave;

			string cadenaQueryFiltros = "";
			int numParametros = httpReceivedQueryDict.Count;
			int indice = 0;

			Type modelType = model.GetType();

			List<object> entityList = new List<object>();

			foreach (var k in httpReceivedQueryDict)
			{
				nombreLlave = k.ToString();

				indice++;
				if (model.GetType().GetProperty(nombreLlave) != null) // Valid property?
				{
					if (indice == 1)
					{
						cadenaQueryFiltros = " WHERE ";
						cadenaQueryFiltros += nombreLlave + " = " + "'" + httpReceivedQueryDict[nombreLlave] + "'";
					}
					else
					{

						//cadenaQueryFiltros = " AND ";
						cadenaQueryFiltros += " AND " + nombreLlave + " = " + "'" + httpReceivedQueryDict[nombreLlave] + "'";

					}

					//if (indice < numParametros)
					//{
					//    cadenaQueryFiltros += " AND ";
					//}
				}
			}
			return cadenaQueryFiltros;
		}

		public static IList<Object> Select(string table, NameValueCollection ReceivedQueryStringFields, object model)
		{
			var httpReceivedQueryDict = ReceivedQueryStringFields;

			IList<string> listaDeProps = new List<string>();
			string nombreLlave;
			string cadenaQuery = "SELECT  * FROM " + table;
			string cadenaQueryFiltros = "";
			string cadenaQueryTerminada = "";
			int numParametros = httpReceivedQueryDict.Count;
			int indice = 0;

			Object modelInstance = null;
			Type modelType = model.GetType();

			List<object> entityList = new List<object>();

			foreach (var k in httpReceivedQueryDict)
			{
				nombreLlave = k.ToString();

				indice++;
				if (model.GetType().GetProperty(nombreLlave) != null) // Valid property?
				{
					if (indice == 1)
					{
						cadenaQueryFiltros = " WHERE ";
					}
					cadenaQueryFiltros += nombreLlave + " = " + "'" + httpReceivedQueryDict[nombreLlave] + "'";
					if (indice < numParametros)
					{
						cadenaQueryFiltros += " AND ";
					}
				}
			}

			cadenaQueryTerminada = cadenaQuery + cadenaQueryFiltros;
			DataSet ds = instruccionesSQL.EjecutarSelect(cadenaQueryTerminada);

			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
				var modelProperties = model.GetType().GetProperties();



				foreach (var prop in modelProperties)
				{
					var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();

					var tipoPropiedad = prop.PropertyType.Name;
					var valor = ds.Tables[0].Rows[i][prop.Name];

					if (tipoPropiedad == "DateTime")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (DateTime)valor);
						}
						else
						{
							valor = DateTime.MinValue;
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
						}

					}
					else if (tipoPropiedad == "Boolean")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (bool)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, false);
						}

					}
					else if (tipoPropiedad == "Int32")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (int)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
						}
					}
					else if (tipoPropiedad == "double")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (double)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
						}
					}
					else
					{
						valor = ds.Tables[0].Rows[i][prop.Name].ToString();
						modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
					}

				}

				entityList.Add(modelInstance);
			}

			return entityList;


		}
		public static IList<Object> SelectToExpandoObjects(string table, NameValueCollection ReceivedQueryStringFields, object model)
		{
			var httpReceivedQueryDict = ReceivedQueryStringFields;

			IList<string> listaDeProps = new List<string>();
			string nombreLlave;
			string cadenaQuery = "SELECT  * FROM " + table;
			string cadenaQueryFiltros = "";
			string cadenaQueryTerminada = "";
			int numParametros = httpReceivedQueryDict.Count;
			int indice = 0;

			//Object modelInstance = null;
			Type modelType = model.GetType();

			List<object> entityList = new List<object>();

			foreach (var k in httpReceivedQueryDict)
			{
				nombreLlave = k.ToString();


				indice++;
				if (model.GetType().GetProperty(nombreLlave) != null) // Valid property?
				{
					if (indice == 1)
					{
						cadenaQueryFiltros = " WHERE ";
					}
					cadenaQueryFiltros += nombreLlave + " = " + "'" + httpReceivedQueryDict[nombreLlave] + "'";
					if (indice < numParametros)
					{
						cadenaQueryFiltros += " AND ";
					}
				}
			}

			cadenaQueryTerminada = cadenaQuery + cadenaQueryFiltros;
			DataSet ds = instruccionesSQL.EjecutarSelect(cadenaQueryTerminada);

			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				//modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
				var objRecord = new ExpandoObject() as IDictionary<string, Object>;

				var modelProperties = model.GetType().GetProperties();



				foreach (var prop in modelProperties)
				{
					// var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();
					//var tipoPropiedad = prop.PropertyType.Name;
					//var valor = ds.Tables[0].Rows[i][prop.Name];
					var valor = ds.Tables[0].Rows[i][prop.Name].ToString();
					objRecord.Add(prop.Name, valor);

					// modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);


				}

				entityList.Add(objRecord);
			}

			return entityList;


		}
		public static IList<Object> Select(string table, string query, object model)
		{
			Object modelInstance = null;
			Type modelType = model.GetType();
			List<object> entityList = new List<object>();

			DataSet ds = instruccionesSQL.EjecutarSelect(query);

			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
																			 //var row = ds.Tables[0].Rows[i].ItemArray; 
				var modelProperties = model.GetType().GetProperties();


				foreach (var prop in modelProperties)
				{
					var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();
					var tipoPropiedad = prop.PropertyType.Name;
					var valor = ds.Tables[0].Rows[i][prop.Name];

					if (tipoPropiedad == "DateTime")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (DateTime)valor);
						}
						else
						{
							valor = DateTime.MinValue;
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
						}

					}
					else if (tipoPropiedad == "Boolean")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (bool)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, false);
						}

					}
					else if (tipoPropiedad == "Int32")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (int)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
						}
					}
					else
					{
						valor = ds.Tables[0].Rows[i][prop.Name].ToString();
						modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
					}

				}

				entityList.Add(modelInstance);
			}

			return entityList;

			//return cadenaQuery + cadenaQueryFiltros;
		}
		public static IList<Object> SelectBusqueda(string table, string query, object model)
		{
			Object modelInstance = null;
			Type modelType = model.GetType();
			List<object> entityList = new List<object>();

			DataSet ds = instruccionesSQL.EjecutarSelectBusqueda(query);

			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
																			 //var row = ds.Tables[0].Rows[i].ItemArray; 
				var modelProperties = model.GetType().GetProperties();


				foreach (var prop in modelProperties)
				{
					var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();
					var tipoPropiedad = prop.PropertyType.Name;
					var valor = ds.Tables[0].Rows[i][prop.Name];

					if (tipoPropiedad == "DateTime")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (DateTime)valor);
						}
						else
						{
							valor = DateTime.MinValue;
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
						}

					}
					else if (tipoPropiedad == "Boolean")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (bool)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, false);
						}

					}
					else if (tipoPropiedad == "Int32")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (int)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
						}
					}
					else
					{
						valor = ds.Tables[0].Rows[i][prop.Name].ToString();
						modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
					}

				}

				entityList.Add(modelInstance);
			}

			return entityList;

			//return cadenaQuery + cadenaQueryFiltros;
		}
		public static IList<Object> SelectSP(string query, object model)
		{
			Object modelInstance = null;
			Type modelType = model.GetType();
			List<object> entityList = new List<object>();

			//DataSet ds = instruccionesSQL.EjecutarSelectCatchError(query);
			IDictionary<string, Object> ejecutarSelectAnswer = instruccionesSQL.EjecutarSelectCatchingError(query);
			if ((bool)ejecutarSelectAnswer["success"] == false)
			{
				entityList.Add("success: " + ejecutarSelectAnswer["success"]);
				entityList.Add("Error SQL : " + ejecutarSelectAnswer["Error SQL : "]);
				return entityList;
			}

			DataSet ds = (DataSet)ejecutarSelectAnswer["ds"];

			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
																			 //var row = ds.Tables[0].Rows[i].ItemArray; 
				var modelProperties = model.GetType().GetProperties();


				foreach (var prop in modelProperties)
				{
					var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();
					var tipoPropiedad = prop.PropertyType.Name;
					var valor = ds.Tables[0].Rows[i][prop.Name];

					if (tipoPropiedad == "DateTime")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (DateTime)valor);
						}
						else
						{
							valor = DateTime.MinValue;
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
						}

					}
					else if (tipoPropiedad == "Boolean")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (bool)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, false);
						}

					}
					else if (tipoPropiedad == "Int32")
					{
						if (valor != System.DBNull.Value)
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (int)valor);
						}
						else
						{
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
						}
					}
					else
					{
						valor = ds.Tables[0].Rows[i][prop.Name].ToString();
						modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
					}

				}

				entityList.Add(modelInstance);
			}

			return entityList;

			//return cadenaQuery + cadenaQueryFiltros;
		}
		public static IList<Object> SelectStoredProcedure(string query, object model)
		{
			Object modelInstance = null;
			Type modelType = model.GetType();
			List<object> entityList = new List<object>();

			try
			{


				//DataSet ds = instruccionesSQL.EjecutarSelectCatchError(query);
				IDictionary<string, Object> ejecutarSelectAnswer = instruccionesSQL.EjecutarSelectCatching(query);
				UtilityObjects.Log(query);
				if ((bool)ejecutarSelectAnswer["success"] == false)
				{
					entityList.Add("success: " + ejecutarSelectAnswer["success"]);
					entityList.Add("Error SQL : " + ejecutarSelectAnswer["Error SQL : "]);
					UtilityObjects.Log($"ERROR ejecutando {query}: {ejecutarSelectAnswer["Error SQL : "]}");
					return entityList;
				}

				DataSet ds = (DataSet)ejecutarSelectAnswer["ds"];

				for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
				{
					modelInstance = Activator.CreateInstance(model.GetType());   // Create an instance of the model received as a parameter
																				 //var row = ds.Tables[0].Rows[i].ItemArray; 
					var modelProperties = model.GetType().GetProperties();


					foreach (var prop in modelProperties)
					{
						var tipo = modelInstance.GetType().GetProperty(prop.Name).GetType();
						var tipoPropiedad = prop.PropertyType.Name;
						var valor = ds.Tables[0].Rows[i][prop.Name];

						if (tipoPropiedad == "DateTime")
						{
							if (valor != System.DBNull.Value)
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (DateTime)valor);
							}
							else
							{
								valor = DateTime.MinValue;
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
							}

						}
						else if (tipoPropiedad == "Boolean")
						{
							if (valor != System.DBNull.Value)
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, (bool)valor);
							}
							else
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, false);
							}

						}
						else if (tipoPropiedad == "Int32")
						{
							if (valor != System.DBNull.Value)
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, Convert.ToInt32(valor));
							}
							else
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
							}
						}
						else if (tipoPropiedad == "Double")
						{
							if (valor != System.DBNull.Value)
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance,Convert.ToDouble(valor));
							}
							else
							{
								modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, 0);
							}
						}
						else
						{
							valor = ds.Tables[0].Rows[i][prop.Name].ToString();
							modelInstance.GetType().GetProperty(prop.Name).SetValue(modelInstance, valor);
						}

					}
					entityList.Add(modelInstance);
				}
			}
			catch (Exception e)
			{
				UtilityObjects.Log($"ERROR: {e}");
			}
			return entityList;
			//return cadenaQuery + cadenaQueryFiltros;
		}
		public static IList<Object> SelectSP(string query)
		{
			List<object> entityList = new List<object>();

			try
			{

				//DataSet ds = instruccionesSQL.EjecutarSelectCatchError(query);
				UtilityObjects.Log($"APIToolkit.SelectSP({query})");
				IDictionary<string, Object> ejecutarSelectAnswer = instruccionesSQL.EjecutarSelectCatching(query);
				if ((bool)ejecutarSelectAnswer["success"] == false)
				{
					entityList.Add("success: " + ejecutarSelectAnswer["success"]);
					entityList.Add("Error SQL : " + ejecutarSelectAnswer["Error SQL : "]);
					UtilityObjects.Log($"APIToolkit.SelectSP.ErrorSQL({ejecutarSelectAnswer["Error SQL : "]})");
					return entityList;
				}
				UtilityObjects.Log("APIToolkit.SelectSP.OK");
				DataSet ds = (DataSet)ejecutarSelectAnswer["ds"];
				var table = ds.Tables[0];
				for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
				{
					var objRegistro = new ExpandoObject() as IDictionary<string, Object>;

					foreach (DataColumn column in table.Columns)
					{
						var valor = ds.Tables[0].Rows[i][column.ColumnName];
						objRegistro.Add(column.ColumnName, valor);
						//Console.WriteLine(column.ColumnName);
					}

					entityList.Add(objRegistro);
				}

				return entityList;
			}
			catch (Exception e)
			{
				UtilityObjects.Log($"ERROR selectSP: {e}");
				return entityList;
			}

			//return cadenaQuery + cadenaQueryFiltros;
		}

		public static string RegresaString(string query)
		{
			string respuesta = string.Empty;

			respuesta = instruccionesSQL.RegresaUnaCadena(query);


			return respuesta;
		}

		public static Int64 RegresaEntero(string query)
		{
			Int64 respuesta = 0;

			respuesta = instruccionesSQL.RegresaUnEntero(query);


			return respuesta;
		}

		public static IList<Object> SelectSP(string query, JObject parametros)
		{
			List<object> entityList = new List<object>();
			var props = parametros.Properties();
			string sp_params = "";

			foreach (JProperty property in props)
			{
				var propName = property.Name;
				var propValue = property.Value;

				sp_params += '@' + propName + "= '" + propValue + "',";

			}
			sp_params = sp_params.Substring(0, sp_params.Length - 1);
			query = "exec " + query + " " + sp_params;
			//DataSet ds = instruccionesSQL.EjecutarSelectCatchError(query);
			IDictionary<string, Object> ejecutarSelectAnswer = instruccionesSQL.EjecutarSelectCatching(query);
			if ((bool)ejecutarSelectAnswer["success"] == false)
			{
				entityList.Add("success: " + ejecutarSelectAnswer["success"]);
				entityList.Add("Error SQL : " + ejecutarSelectAnswer["Error SQL : "]);
				return entityList;
			}

			DataSet ds = (DataSet)ejecutarSelectAnswer["ds"];
			var table = ds.Tables[0];
			for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
			{
				var objRegistro = new ExpandoObject() as IDictionary<string, Object>;

				foreach (DataColumn column in table.Columns)
				{
					var valor = ds.Tables[0].Rows[i][column.ColumnName];
					objRegistro.Add(column.ColumnName, valor);
					//Console.WriteLine(column.ColumnName);
				}

				entityList.Add(objRegistro);
			}

			return entityList;

			//return cadenaQuery + cadenaQueryFiltros;
		}

		public string Insert(string table, object objInsertar)
		{

			string query = "insert into " + table;
			string valuesList = "";

			var ObjProperties = objInsertar.GetType().GetProperties();
			foreach (var prop in ObjProperties)
			{
				var tipoPropiedad = prop.PropertyType.Name;
				var nombrePropiedad = prop.Name;
			}

			query = "insert into usuarios values('@idRol'," +
													 "'@idPerfil'," +
													 "'@idArea'," +
													 "'@s_nombre'," +
													 "'@s_puesto'," +
													 "'@s_claveEmpleado'," +
													 "'@s_psw'," +
													 "'@d_fechaCambioPsw'," +
													 "'@s_email'," +
													 "'@s_telefono'," +
													 "'@b_sesionIniciada'," +
													 "'@b_habilitado'," +
													 "'@d_fechaAlta'," +
													 "'@d_fechaDeshabilitado'," +
													 "'@s_pathFoto')";

			return query;
		}
		public static IDictionary<string, Object> SQLInsert(string table, JObject objToInsert, object model)
		{
			var objResultado = QueryInsertFromObject(table, objToInsert, model);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				objResultado.Remove("query");
				// string res =instruccionesSQL.SQLCommand(query);
				var res = instruccionesSQL.SQLCommandReturnsIdentity(query);
				var resCasted = res as IDictionary<string, Object>;
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;

				if (resCasted != null)
				{
					objFinal.Add("success", true);
					objFinal.Add("payload", resCasted["payload"]);
					objFinal.Add("mensaje", objResultado);
					if (resCasted["success"].ToString() == "true")
					{
						return objFinal;
					}
					else
					{
						objFinal["success"] = false;
						objFinal.Add("mensaje", objResultado);
						objFinal.Add("Error SQL : ", res);
						return objFinal;
					}
				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("mensaje", objResultado);
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}


			}
			objResultado.Remove("query");
			return objResultado;

		}
		public static IDictionary<string, Object> SQLInsertSP(string storedProcedure, JObject objToInsert, object model)
		{
			UtilityObjects.Log($"SQLInsertSP({storedProcedure},{objToInsert},{model})");
			var objResultado = QueryInsertFromObjectSP(storedProcedure, objToInsert, model);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				var res = instruccionesSQL.SQLCommandSPidentity(storedProcedure, objToInsert);
				var resCasted = res as IDictionary<string, Object>;
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;

				if (resCasted != null)
				{
					objFinal.Add("success", true);
					objFinal.Add("payload", resCasted["payload"]);
					objFinal.Add("mensaje", objResultado);
					if (resCasted["success"].ToString() == "true")
					{
						return objFinal;
					}
					else
					{
						objFinal["success"] = false;
						objFinal.Add("mensaje", objResultado);
						objFinal.Add("Error SQL : ", res);
						return objFinal;
					}
				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("mensaje", objResultado);
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}

			}
			objResultado.Remove("query");
			return objResultado;

		}
		public static IDictionary<string, Object> SQLUpdate(string table, JObject objToInsert, object model, string idColumnName)
		{
			var objResultado = QueryUpdateFromObject(table, objToInsert, model, idColumnName);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				string res = instruccionesSQL.SQLCommand(query);
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;
				objFinal.Add("success", true);

				if (res == "true")
				{
					return objFinal;
				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}
			}
			objResultado.Remove("query");
			return objResultado;
		}

		public static IDictionary<string, Object> SQLUpdateWithNulls(string table, JObject objToInsert, object model, string idColumnName)
		{
			var objResultado = QueryUpdateFromObjectWithNulls(table, objToInsert, model, idColumnName);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				string res = instruccionesSQL.SQLCommand(query);
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;
				objFinal.Add("success", true);
				if (res == "true")
				{

					return objFinal;

				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}


			}
			objResultado.Remove("query");
			return objResultado;
		}
		public static IDictionary<string, Object> SQLUpdate(string table, JObject objToInsert, object model, NameValueCollection ReceivedQueryStringFields)
		{
			var objResultado = QueryUpdateFromObject(table, objToInsert, model, ReceivedQueryStringFields);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				string res = instruccionesSQL.SQLCommand(query);
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;
				objFinal.Add("success", true);
				if (res == "true")
				{
					return objFinal;
				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}
			}

			objResultado.Remove("query");
			return objResultado;
		}
		public static IDictionary<string, Object> SQLDelete(string table, JObject objToDelete, string idColumnName)
		{
			var objFinal = new ExpandoObject() as IDictionary<string, Object>;

			if (objToDelete.Property(idColumnName) != null)
			{

				var prop = objToDelete.Property(idColumnName);
				string Idvalor = prop.Value.ToString();
				var query = "delete from " + table + " where " + idColumnName + "=" + "'" + Idvalor + "'";

				string res = instruccionesSQL.SQLCommand(query);


				if (res == "true")
				{
					objFinal.Add("success", true);
					return objFinal;

				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}


			}
			else
			{


				objFinal.Add("success", false);
				objFinal.Add("error ", "El campo " + idColumnName + " es requerido para la eliminación");
				return objFinal;

			}


		}
		public static IDictionary<string, Object> SQLDelete(string table, object model, NameValueCollection ReceivedQueryStringFields)
		{
			var objResultado = QueryDeleteFromQueryString(table, model, ReceivedQueryStringFields);
			if ((bool)objResultado["success"])
			{
				var query = (string)objResultado["query"];
				string res = instruccionesSQL.SQLCommand(query);
				var objFinal = new ExpandoObject() as IDictionary<string, Object>;
				objFinal.Add("success", true);
				if (res == "true")
				{
					return objFinal;
				}
				else
				{
					objFinal["success"] = false;
					objFinal.Add("Error SQL : ", res);
					return objFinal;
				}
			}

			objResultado.Remove("query");
			return objResultado;
		}
		public static IDictionary<string, Object> SubirArchivo(string table, string campo_ruta,
			string campo_adjunto, string s_adjunto, string campo_id,
			string id, string ruta)
		{
			var objResultado = new ExpandoObject() as IDictionary<string, Object>;
			string idadjuntararchivo = id;
			string resultadoUpload = UploadArchivo(idadjuntararchivo, ruta);
			string path = UploadPath(idadjuntararchivo, ruta);
			if (resultadoUpload != "false")
			{

				var query = " sp_ActualizaArchivo " + "'" + table + "', '" + campo_ruta + "', '"
					+ path + "' , '" + campo_adjunto + "' , '" + s_adjunto + "', '" + campo_id + " ',' " + id + "'";

				bool res = instruccionesSQL.EditarFila(query);
				if (res == true)
				{
					objResultado.Add("sucess", "true");
					objResultado.Add("mensaje", "El archivo se adjunto exitosamente.");
					return objResultado;
				}
				else
				{
					objResultado.Add("sucess", "false");
					objResultado.Add("mensaje", "No fue posible actualizar el path del archivo adjunto.");
					return objResultado;
				}
			}
			objResultado.Add("sucess", "false");
			objResultado.Add("mensaje", "Ocurrió un error al hacer la carga del archivo");
			return objResultado;
		}
		private static string UploadArchivo(string id, string path)
		{
			var httpRequest = HttpContext.Current.Request;
			var httpPostedFile = System.Web.HttpContext.Current.Request.Files;

			if (httpRequest.Files.Count > 0)
			{
				var postedFile = httpRequest.Files[0];
				var filePath = HttpContext.Current.Server.MapPath(path + id + Path.GetExtension(postedFile.FileName));
				postedFile.SaveAs(filePath);
				return Path.GetExtension(postedFile.FileName).ToString();
			}
			else
			{
				// result = Request.CreateResponse(HttpStatusCode.BadRequest);
				return "false";
			}
		}
		private static string UploadPath(string archivo, string path)
		{
			var httpRequest = HttpContext.Current.Request;
			var httpPostedFile = System.Web.HttpContext.Current.Request.Files;

			if (httpRequest.Files.Count > 0)
			{
				var postedFile = httpRequest.Files[0];
				var filePath = HttpContext.Current.Server.MapPath(path + archivo + Path.GetExtension(postedFile.FileName));
				postedFile.SaveAs(filePath);
				return filePath;
			}
			else
			{
				// result = Request.CreateResponse(HttpStatusCode.BadRequest);
				return "false";
			}
		}
		private static HttpResponseMessage Test(string NombreArchivo, string ruta)
		{
			var path = System.Web.HttpContext.Current.Server.MapPath(ruta + NombreArchivo);
			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
			var stream = new FileStream(path, FileMode.Open);
			result.Content = new StreamContent(stream);
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
			result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(path);
			result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			result.Content.Headers.ContentLength = stream.Length;
			return result;
		}
	}
}
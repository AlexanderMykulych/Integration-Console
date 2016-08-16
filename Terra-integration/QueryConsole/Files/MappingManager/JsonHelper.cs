using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.TsConfiguration;
using Terrasoft.Core.DB;
using System.Data;

namespace Terrasoft.TsConfiguration
{
	public static class JsonEntityHelper
	{
		public static string RefName = @"#ref";
		public static object GetSimpleTypeValue(JToken jToken)
		{
			try
			{
				switch (jToken.Type)
				{
					case JTokenType.String:
						return jToken.Value<string>();
					case JTokenType.Integer:
						return jToken.Value<Int64>();
					case JTokenType.Float:
						return jToken.Value<float>();
					case JTokenType.Date:
						return jToken.Value<DateTime>();
					case JTokenType.TimeSpan:
						return jToken.Value<TimeSpan>();
					case JTokenType.Boolean:
						return jToken.Value<bool>();
					default:
						return null;
				}
			}
			catch (Exception e)
			{
				//IntegrationLogger.Error("Method [GetSimpleTypeValue] catch exception: Message = {0}", e.Message);
				throw;
			}
		}

		public static object GetSimpleTypeValue(object value)
		{
			try
			{
				if (value is DateTime)
				{
					return ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss");
				}
				if (value is bool)
				{
					return (bool)value == true ? true : false;
				}
				return value;
			}
			catch (Exception e)
			{
				//IntegrationLogger.Error("Method [GetSimpleTypeValue] catch exception: Message = {0}", e.Message);
				throw;
			}
		}

		public static List<object> GetColumnValues(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending, Dictionary<string, string> filters = null)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			if (limit > 0)
			{
				esq.RowCount = limit;
			}
			var resColumn = esq.AddColumn(resultColumnName);
			if (!string.IsNullOrEmpty(orderColumnName))
			{
				var orderColumn = esq.AddColumn(orderColumnName);
				orderColumn.SetForcedQueryColumnValueAlias("orderColumn");
				orderColumn.OrderDirection = orderType;
				orderColumn.OrderPosition = 0;
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, entityPath, entityPathValue));
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
				}
			}
			return esq.GetEntityCollection(userConnection).Select(x =>
				x.GetColumnValue(resColumn.IsLookup ? PrepareColumn(resColumn.Name, true) : resColumn.Name)
			).ToList();
		}
		
		public static List<object> CreateColumnValues(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending, Dictionary<string, string> filters = null)
		{
			try
			{
				var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
				var insert = new Insert(userConnection)
								.Into(entityName) as Insert;
				object resultValue;
				var resultColumn = schema.Columns.GetByName(resultColumnName);
				if (resultColumn.DataValueType.ValueType == typeof(Guid))
				{
					resultValue = Guid.NewGuid();
				}
				else
				{
					resultValue = resultColumn.DataValueType.DefValue;
				}
				var resColumn = insert.Set(GetSqlNameByEntity(schema, resultColumnName), Column.Parameter(resultValue));
				insert.Set(GetSqlNameByEntity(schema, entityPath), Column.Parameter(entityPathValue));
				if (filters != null)
				{
					foreach (var filter in filters)
					{
						insert.Set(GetSqlNameByEntity(schema, filter.Key), Column.Parameter(filter.Value));
					}
				}
				insert.Execute();
				return new List<object>() { resultValue };
			} catch(Exception e)
			{
				//TODO
				return new List<object>();
			}
		}

		public static List<object> GetColumnValuesWithFilters(UserConnection userConnection, string entityName, string entityPath, object entityPathValue, string resultColumnName, Dictionary<string, string> filters, int limit = -1,
			string orderColumnName = "CreatedOn", OrderDirection orderType = OrderDirection.Descending)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
			if (limit > 0)
			{
				esq.RowCount = limit;
			}
			var resColumn = esq.AddColumn(resultColumnName);
			if (!string.IsNullOrEmpty(orderColumnName))
			{
				var orderColumn = esq.AddColumn(orderColumnName);
				orderColumn.SetForcedQueryColumnValueAlias("orderColumn");
				orderColumn.OrderDirection = orderType;
				orderColumn.OrderPosition = 0;
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, entityPath, entityPathValue));
			foreach (var filter in filters)
			{
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
			}
			return esq.GetEntityCollection(userConnection).Select(x =>
				x.GetColumnValue(resColumn.IsLookup ? PrepareColumn(resColumn.Name, true) : resColumn.Name)
			).ToList();
		}

		public static string PrepareColumn(string columnName, bool withId = false)
		{
			var endWithId = columnName.EndsWith("Id");
			return withId ? (endWithId ? columnName : columnName + "Id") : (endWithId ? columnName.Substring(0, columnName.Length - 2) : columnName);
		}
		public static bool IsAllNotNullAndEmpty(params object[] values)
		{
			foreach (var value in values)
			{
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
					return false;
			}
			return true;
		}
		public static string GetFirstNotNull(params string[] strings)
		{
			return strings.FirstOrDefault(x => !string.IsNullOrEmpty(x));
		}
		public static List<JObject> GetCompositeJObjects(object colValue, string colName, string entityName, string handlerName, UserConnection userConnection, int maxCount = -1)
		{
			try
			{
				var jObjectsList = new List<JObject>();
				var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, entityName);
				esq.AddAllSchemaColumns();
				var dateColumn = esq.AddColumn("CreatedOn");
				dateColumn.OrderByDesc();
				if (maxCount > 0)
					esq.RowCount = maxCount;
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, colName, colValue));
				var collection = esq.GetEntityCollection(userConnection);
				foreach (var item in collection)
				{
					try
					{
						var integrationInfo = new CsConstant.IntegrationInfo(new JObject(), userConnection, CsConstant.TIntegrationType.Export, null, handlerName, "", item);
						var handler = (new IntegrationEntityHelper()).GetIntegrationHandler(integrationInfo);
						if (handler != null)
						{
							jObjectsList.Add(handler.ToJson(integrationInfo));
						}
					}
					catch (Exception e)
					{
						//IntegrationLogger.Error("Method [] catch exception message = {0}", e.Message);
						//throw;
					}
				}
				return jObjectsList;
			}
			catch (Exception e)
			{
				return new List<JObject>();
			}
		}
		public static Tuple<Dictionary<string, string>, Entity> GetEntityByExternalId(string schemaName, int externalId, UserConnection userConnection, bool addAllColumn, params string[] columns)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, schemaName);
			var columnDict = new Dictionary<string, string>();
			if (addAllColumn)
			{
				esq.AddAllSchemaColumns();
			}
			else
			{
				foreach (var column in columns)
				{
					var columnSchema = esq.AddColumn(column);
					columnDict.Add(column, columnSchema.Name);
				}
			}
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, CsConstant.ServiceColumnInBpm.Identifier, externalId));
			var entity = esq.GetEntityCollection(userConnection).FirstOrDefault();
			return new Tuple<Dictionary<string, string>, Entity>(columnDict, entity);
		}
		public static bool isEntityExist(string schemaName, UserConnection userConnection, Dictionary<string, object> filters)
		{
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, schemaName);
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(schemaName);
			esq.AddColumn(esq.CreateAggregationFunction(AggregationTypeStrict.Count, schema.PrimaryColumn.Name));
			foreach (var filter in filters)
			{
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, filter.Key, filter.Value));
			}
			var select = esq.GetSelectQuery(userConnection);
			using (DBExecutor dbExecutor = userConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return reader.GetColumnValue<int>("Count") > 0;
					}
				}
			}
			return false;
		}

		public static void UpdateOrInsertEntityColumn(string entityName, string setColumn, object setValue, UserConnection userConnection, Dictionary<string, string> optionalColumns, List<Tuple<string, object>> filters)
		{
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			
			filters.AddRange(optionalColumns.Select(x => new Tuple<string, object>(x.Key, x.Value)));
			if (GetEntityCount(entityName, userConnection, filters) > 0)
			{
				var update = new Update(userConnection, entityName);
				var selectUpdate = new Select(userConnection)
									.Top(1)
									.Column("Id")
									.From(entityName)
									.OrderByDesc("CreatedOn");
				if (filters.Any())
				{
					selectUpdate.Where(GetSqlNameByEntity(schema, filters[0].Item1)).IsEqual(Column.Parameter(filters[0].Item2));
					foreach (var filter in filters.Skip(1))
					{
						selectUpdate.And(GetSqlNameByEntity(schema, filter.Item1)).IsEqual(Column.Parameter(filter.Item2));
					}
				}
				update.Where("Id").In(selectUpdate);
				update.Set(GetSqlNameByEntity(schema, setColumn), Column.Parameter(setValue));
				foreach (var optionalColumn in optionalColumns)
				{
					update.Set(GetSqlNameByEntity(schema, optionalColumn.Key), Column.Parameter(optionalColumn.Value));
				}
				update.Execute();
			}
			else
			{
				var insert = new Insert(userConnection).Into(entityName);
				insert.Set(GetSqlNameByEntity(schema, setColumn), Column.Parameter(setValue));
				foreach (var optionalColumn in optionalColumns)
				{
					insert.Set(GetSqlNameByEntity(schema, optionalColumn.Key), Column.Parameter(optionalColumn.Value));
				}
				insert.Execute();
			}
		}
		public static string GetSqlNameByEntity(EntitySchema schema, string columnName)
		{
			return schema.Columns.GetByName(columnName).ColumnValueName;
		}
		public static int GetEntityCount(string entityName, UserConnection userConnection, List<Tuple<string, object>> filters)
		{
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			var select = new Select(userConnection)
						.Column(Func.Count("Id")).As("count")
						.From(entityName);
			if (filters.Any())
			{
				select.Where(GetSqlNameByEntity(schema, filters[0].Item1)).IsEqual(Column.Parameter(filters[0].Item2));
				foreach (var filter in filters.Skip(1))
				{
					select.And(GetSqlNameByEntity(schema, filter.Item1)).IsEqual(Column.Parameter(filter.Item2));
				}
			}
			using (DBExecutor dbExecutor = select.UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "count");
					}
				}
			}
			return 0;
		}

		public static Dictionary<string, string> ParsToDictionary(string text, char first, char second)
		{
			var result = new Dictionary<string, string>();
			if (string.IsNullOrEmpty(text))
			{
				return result;
			}
			var pairs = text.Split(first);
			foreach (var pair in pairs)
			{
				var values = pair.Split(second);
				result.AddIfNotExists(new KeyValuePair<string, string>(values.First(), values.Last()));
			}
			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Configuration;

namespace Terrasoft.TsConfiguration {
	public static class EntityHelper {
		public static void SetColumnWithEmptyCheck(this Entity entity, string columnName, Guid value) {
			if(value != Guid.Empty) {
				entity.SetColumnValue(columnName, value);
			}
		}
		public static int GetExternalIdValue(this Entity entity, string externalIdPath) {
			int result = 0;
			try {
				if(entity.IsColumnValueLoaded(externalIdPath)) {
					result = entity.GetTypedColumnValue<int>(externalIdPath);
				} else {
					var select = new Select(entity.UserConnection)
								.Column(externalIdPath)
								.From(entity.SchemaName)
								.Where(entity.Schema.PrimaryColumn.Name).IsEqual(Column.Parameter(entity.PrimaryColumnValue)) as Select;
					using(var dbExecutor = entity.UserConnection.EnsureDBConnection()) {
						using(var reader = select.ExecuteReader(dbExecutor)) {
							if(reader.Read()) {
								result = DBUtilities.GetColumnValue<int>(reader, externalIdPath);
							}
						}
					}
				}
			} catch(Exception e) {
				IntegrationLogger.Error(e);
			}
			return result;
		}

		public static bool IsExistDuplicateByExternalId(UserConnection userConnection, string entityName, string externalIdPath, int externalId, Action<Exception> onExceptionAction = null)
		{
			try
			{
				var select = new Select(userConnection)
									.Top(1)
									.Column(Func.Count(Column.Asterisk())).As("count")
									.From(entityName)
									//Используем фильтр только по ExternalId, для того чтобы снизить количество чтений, так как существует индекс по полю ExternalId
									.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
				using (var dbExecutor = userConnection.EnsureDBConnection())
				{
					using (var reader = select.ExecuteReader(dbExecutor))
					{
						if (reader.Read())
						{
							return reader.GetColumnValue<int>("count") > 1;
						}
					}
				}
			}
			catch (Exception e)
			{
				if(onExceptionAction != null)
				{
					onExceptionAction(e);
				}
			}
			return false;
		}
		public static void ClearDuplicateExternalIdByIds(UserConnection userConnection, string entityName, string primaryColumnName, string externalIdPath, int externalId, Guid primaryColumnValue, Action<Exception> onExceptionAction = null)
		{
			try
			{
				var update = new Update(userConnection, entityName)
								.Set(externalIdPath, Column.Const(0))
								.Where(externalIdPath).IsEqual(Column.Parameter(externalId))
								.And(primaryColumnName).IsNotEqual(Column.Parameter(primaryColumnValue)) as Update;
				update.Execute();
			} catch(Exception e)
			{
				if (onExceptionAction != null)
				{
					onExceptionAction(e);
				}
			}
		}

		public static string GetColumnNameByCommunicationType(Guid communicationType)
		{
			string columnName;
			switch (communicationType.ToString())
			{
				case CommunicationTypeConsts.WebId:
					columnName = "Web";
					break;
				case CommunicationTypeConsts.MainPhoneId:
					columnName = "Phone";
					break;
				case CommunicationTypeConsts.AdditionalPhoneId:
					columnName = "AdditionalPhone";
					break;
				case CommunicationTypeConsts.FaxId:
					columnName = "Fax";
					break;
				default:
					columnName = string.Empty;
					break;
			}
			return columnName;
		}
	}
}

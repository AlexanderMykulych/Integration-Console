﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using IntegrationInfo = Terrasoft.TsConfiguration.CsConstant.IntegrationInfo;
using TIntegrationType = Terrasoft.TsConfiguration.CsConstant.TIntegrationType;


namespace Terrasoft.TsConfiguration
{
	public class MappingHelper
	{

		public string RefName = @"#ref";
		public bool _isInsertToDB;
		public List<MappingItem> MapConfig;
		public UserConnection UserConnection;
		public Queue<Action> MethodQueue;
		public RulesFactory RulesFactory;
		 

		public bool IsInsertToDB
		{
			get
			{
				try
				{
					_isInsertToDB = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, CsConstant.SysSettingsCode.IsInsertToDB, _isInsertToDB);
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "IsInsertToDB");
					_isInsertToDB = false;
				}
				return _isInsertToDB;
			}
		}
		 

		public MappingHelper()
		{
			_isInsertToDB = false;
			MethodQueue = new Queue<Action>();
			RulesFactory = new RulesFactory();
		}
		 

		public void StartMappByConfig(CsConstant.IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig, bool withHeader = true)
		{
			try
			{
				switch (integrationInfo.IntegrationType)
				{
					case CsConstant.TIntegrationType.Import:
						{
							StartMappImportByConfig(integrationInfo, jName, mapConfig, withHeader);
							break;
						}
					case CsConstant.TIntegrationType.Export:
						{
							StartMappExportByConfig(integrationInfo, jName, mapConfig);
							break;
						}
					case CsConstant.TIntegrationType.ExportResponseProcess:
						{
							StartMappExportResponseProcessByConfig(integrationInfo, jName, mapConfig);
							break;
						}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, "StartMappByConfig");
				throw;
			}
		}

		public void StartMappImportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig, bool withHeader = true)
		{
			if (integrationInfo.IntegratedEntity == null)
				throw new Exception(string.Format("Integration Entity not exist {0} ({1})", jName));
			var entityJObj = withHeader ? integrationInfo.Data[jName] : integrationInfo.Data;
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Import)
				{
					try
					{
						JToken subJObj = null;
						if (!string.IsNullOrEmpty(item.Selector))
						{
							subJObj = entityJObj.SelectToken(item.Selector);
						}
						else
						{
							subJObj = GetJTokenByPath(entityJObj, item.JSourcePath, item.MapIntegrationType, integrationInfo.IntegrationType);
						}
						if (subJObj != null)
						{
							MapColumn(item, ref subJObj, integrationInfo);
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e, "StartMappImportByConfig");
						if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
						{
							throw;
						}
					}
				}
			}
		}

		public void StartMappExportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			integrationInfo.Data = new JObject();
			if (integrationInfo.Data[jName] == null)
				integrationInfo.Data[jName] = new JObject();
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Export)
				{
					var jObjItem = (new JObject()) as JToken;
					try
					{
						MapColumn(item, ref jObjItem, integrationInfo);
					}
					catch (Exception e)
					{
						IntegrationLogger.MappingError(e, item, integrationInfo);
						if (!item.IgnoreError)
						{
							throw;
						}
						jObjItem = null;
					}
					if (integrationInfo.Data[jName][item.JSourcePath] != null && integrationInfo.Data[jName][item.JSourcePath].HasValues)
					{
						if (integrationInfo.Data[jName][item.JSourcePath] is JArray)
							((JArray)integrationInfo.Data[jName][item.JSourcePath]).Add(jObjItem);
					}
					else
					{
						var resultJ = GetJTokenByPath(integrationInfo.Data[jName], item.JSourcePath, item.MapIntegrationType, integrationInfo.IntegrationType);
						if (jObjItem == null && item.EFieldRequier)
							throw new ArgumentNullException("Field " + item.JSourcePath + " required!");
						if (jObjItem == null && !item.SerializeIfNull)
						{
							resultJ.Parent.Remove();
							continue;
						}
						if (jObjItem != null && jObjItem.ToString() == "0" && !item.SerializeIfZero)
						{
							resultJ.Parent.Remove();
							continue;
						}
						resultJ.Replace(jObjItem);
					}
				}
			}
		}

		public void StartMappExportResponseProcessByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			var entityJObj = integrationInfo.Data[jName];
			foreach (var item in mapConfig)
			{
				try
				{
					if (item.SaveOnResponse)
					{
						var subJObj = GetJTokenByPath(entityJObj, item.JSourcePath, item.MapIntegrationType);
						MapColumn(item, ref subJObj, integrationInfo);
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e, "StartMappExportResponseProcessByConfig");
					if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
					{
						throw;
					}
				}
			}
		}

		public void MapColumn(MappingItem mapItem, ref JToken jToken, IntegrationInfo integrationInfo)
		{
			try
			{
				if (UserConnection == null)
					UserConnection = integrationInfo.UserConnection;
				var entity = integrationInfo.IntegratedEntity;
				var integrationType = integrationInfo.IntegrationType;
				Action executedMethod = new Action(() => { });
				var rule = RulesFactory.GetRule(mapItem.MapType.ToString());
				if (rule != null)
				{
					RuleInfo ruleInfo = null;
					switch (integrationInfo.IntegrationType)
					{
						case TIntegrationType.ExportResponseProcess:
						case TIntegrationType.Import:
							ExecuteOverRuleMacros(mapItem, ref jToken, integrationInfo);
							ruleInfo = new RuleImportInfo()
							{
								config = mapItem,
								entity = integrationInfo.IntegratedEntity,
								json = jToken,
								userConnection = UserConnection,
								integrationType = integrationInfo.IntegrationType,
								action = integrationInfo.Action
							};
							executedMethod = () => rule.Import((RuleImportInfo)ruleInfo);
							if (mapItem.MapExecuteType == TMapExecuteType.BeforeEntitySave)
							{
								executedMethod();
							}
							else
							{
								MethodQueue.Enqueue(executedMethod);
							}
							break;
						case TIntegrationType.Export:
							ruleInfo = new RuleExportInfo()
							{
								config = mapItem,
								entity = integrationInfo.IntegratedEntity,
								json = jToken,
								userConnection = UserConnection,
								integrationType = integrationInfo.IntegrationType,
								action = integrationInfo.Action
							};
							rule.Export((RuleExportInfo)ruleInfo);
							jToken = ruleInfo.json;
							ExecuteOverRuleMacros(mapItem, ref jToken, integrationInfo);
							break;
					}
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.MappingError(e, mapItem, integrationInfo);
			}
		}

		public void ExecuteOverRuleMacros(MappingItem mapItem, ref JToken jToken, IntegrationInfo integrationInfo)
		{
			if (mapItem.OverRuleMacros.IsNullOrEmpty() || jToken == null)
			{
				return;
			}
			switch (integrationInfo.IntegrationType)
			{
				case TIntegrationType.ExportResponseProcess:
				case TIntegrationType.Import:
					jToken = TsMacrosHelper.GetMacrosResultImport(mapItem.OverRuleMacros, jToken.Value<JArray>(), MacrosType.OverRule) as JToken;
					break;
				case TIntegrationType.Export:
					jToken = JToken.FromObject(TsMacrosHelper.GetMacrosResultExport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule));
					break;
			}
		}

		public bool CheckIsExist(string entityName, int externalId, string externalIdPath = "TsExternalId", int entityExternalId = 0)
		{
			if (entityExternalId != 0)
			{
				return true;
			}
			if (externalId == 0)
			{
				return false;
			}
			var select = new Select(UserConnection)
							.Column(Func.Count(CsConstant.ServiceColumnInBpm.Identifier)).As("Count")
							.From(entityName)
							.Where(externalIdPath).IsEqual(Column.Parameter(externalId)) as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						return DBUtilities.GetColumnValue<int>(reader, "Count") > 0;
					}
				}
			}
			return false;
		}

		public void SaveEntity(Entity entity, string jName, bool onResponse = false)
		{
			try
			{
				UserConnection = entity.UserConnection;
				bool result = false;
				if (onResponse)
				{
					IntegrationLocker.Lock(entity.SchemaName, entity.PrimaryColumnValue);
				}
				if (IsInsertToDB)
				{
					switch (entity.StoringState)
					{
						case StoringObjectState.New:
							if (entity.PrimaryColumnValue == Guid.Empty)
							{
								result = entity.Save(false);
							}
							else
							{
								result = entity.InsertToDB(false, false);
							}
							break;
						case StoringObjectState.Changed:
							result = entity.UpdateInDB(false);
							break;
					}
				}
				else
				{
					result = entity.Save(false);
				}
				if (onResponse)
				{
					IntegrationLocker.Unlock(entity.SchemaName);
				}
				ExecuteMapMethodQueue();
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e, string.Format("SaveEntity {0} - {1}", entity.GetType().ToString(), jName));
			}
		}

		public JObject GetJObject(string json)
		{
			return !string.IsNullOrEmpty(json) ? JObject.Parse(json) : null;
		}
		 

				private string PrepareColumn(string columnName, bool withId = false)
		{
			var endWithId = columnName.EndsWith("Id");
			return withId ? (endWithId ? columnName : columnName + "Id") : (endWithId ? columnName.Substring(0, columnName.Length - 2) : columnName);
		}

		private bool IsAllNotNullAndEmpty(params object[] values)
		{
			foreach (var value in values)
			{
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
					return false;
			}
			return true;
		}

		private string GetFirstNotNull(params string[] strings)
		{
			return strings.FirstOrDefault(x => !string.IsNullOrEmpty(x));
		}

		private JToken GetJTokenByPath(JToken jToken, string path, TIntegrationType mapType = TIntegrationType.Import, TIntegrationType type = TIntegrationType.Import)
		{
			return type == TIntegrationType.Export ? jToken.GetJTokenByPath(path, mapType) : jToken.GetJTokenValueByPath(path, mapType);
		}

		private void ExecuteMapMethodQueue()
		{
			while (MethodQueue.Any())
			{
				var method = MethodQueue.Dequeue();
				method();
			}
		}
		 
	}

		public enum TMapType
	{
		RefToGuid = 0,
		Simple = 1,
		FirstDestinationField = 2,
		CompositObject = 3,
		ArrayOfCompositObject = 4,
		Const = 5,
		ArrayOfReference = 6,
		ManyToMany = 8,
		ToDetail = 9
	}
	 

		public enum TMapExecuteType
	{
		AfterEntitySave = 0,
		BeforeEntitySave = 1
	}
	 
		public enum TConstType
	{
		String = 0,
		Bool = 1,
		Int = 2,
		Null = 3,
		EmptyArray = 4
	}
	 
}

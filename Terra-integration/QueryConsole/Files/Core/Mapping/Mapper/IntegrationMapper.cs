using IntegrationInfo = Terrasoft.TsIntegration.Configuration.CsConstant.IntegrationInfo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ninject.Infrastructure.Language;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System;
using Terrasoft.Common;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core;
using Terrasoft.UI.WebControls;
using TIntegrationType = Terrasoft.TsIntegration.Configuration.CsConstant.TIntegrationType;
namespace Terrasoft.TsIntegration.Configuration{
	public class IntegrationMapper : IMapper
	{
		public List<MappingItem> MapConfig;
		public Queue<Action> MethodQueue;
		public RulesFactory RulesFactory;
		private IMapperDbWorker _mapperDbWorker;
		public virtual IMapperDbWorker MapperDbWorker {
			set {
				_mapperDbWorker = value;
			}
			get {
				if (_mapperDbWorker == null)
				{
					_mapperDbWorker = new MapperDbWorker();
				}
				return _mapperDbWorker;
			}
		}
		private IIntegrationObjectProvider _integrationObjectProvider;
		public virtual IIntegrationObjectProvider IntegrationObjectProvider {
			set {
				_integrationObjectProvider = value;
			}
			get {
				if (_integrationObjectProvider == null)
				{
					_integrationObjectProvider = new IntegrationObjectProvider();
				}
				return _integrationObjectProvider;
			}
		}
		public IntegrationMapper()
		{
			MethodQueue = new Queue<Action>();
			RulesFactory = new RulesFactory();
		}
		public virtual void StartMappByConfig(CsConstant.IntegrationInfo integrationInfo, string jName, MappingConfig mapConfig)
		{
			try
			{
				if (mapConfig == null)
				{
					return;
				}
				LoggerHelper.DoInLogBlock("Process Mapping", () =>
				{
					IntegrationLogger.InfoMapping(string.Format("Mapping Start\nIntegration Object: \"{0}\"\nIntegration Type: {1}", jName, integrationInfo.IntegrationType));
					switch (integrationInfo.IntegrationType)
					{
						case CsConstant.TIntegrationType.Import:
							{
								StartMappImportByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
						case CsConstant.TIntegrationType.Export:
							{
								StartMappExportByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
						case CsConstant.TIntegrationType.ExportResponseProcess:
							{
								StartMappExportResponseProcessByConfig(integrationInfo, jName, mapConfig.Items);
								break;
							}
					}
				});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				throw;
			}
		}
		public virtual bool CheckIsExist(UserConnection userConnection, string entityName, object externalId, string externalIdPath = "TsExternalId", object entityExternalId = null)
		{
			if (entityExternalId != null && entityExternalId.ToString() != string.Empty && entityExternalId.ToString() != "0")
			{
				return true;
			}
			if (externalId == null || string.IsNullOrEmpty(externalId.ToString()) || externalId.ToString() == "0")
			{
				return false;
			}
			return MapperDbWorker.IsExists(userConnection, entityName, externalIdPath, externalId);
		}
		//Log key=Mapper
		protected virtual void StartMappImportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			if (integrationInfo.IntegratedEntity == null)
				throw new Exception(string.Format("Integration Entity not exist {0} ({1})", jName));
			var entityJObj = integrationInfo.Data;
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Import)
				{
					try
					{
						IIntegrationObject subJObj = null;
						if (!string.IsNullOrEmpty(item.Selector))
						{
							subJObj = entityJObj.GetSubObject(item.Selector);
						}
						else
						{
							var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
							subJObj = entityJObj.GetSubObject(path);
						}
						if (subJObj != null)
						{
							MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
						}
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
						if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
						{
							throw;
						}
					}
				}
			}
		}
		//Log key=Mapper
		protected virtual void StartMappExportByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			integrationInfo.Data = IntegrationObjectProvider.NewInstance(jName);
			foreach (var item in mapConfig)
			{
				if (item.MapIntegrationType == CsConstant.TIntegrationType.All || item.MapIntegrationType == CsConstant.TIntegrationType.Export)
				{
					IIntegrationObject jObjItem = IntegrationObjectProvider.NewInstance(item.JSourcePath);
					try
					{
						MapColumn(integrationInfo.UserConnection, item, ref jObjItem, integrationInfo);
					}
					catch (Exception e)
					{
						IntegrationLogger.ErrorMapping(MappingErrorLoggerInfo.GetMessage(e, item, integrationInfo));
						if (!item.IgnoreError)
						{
							throw;
						}
						jObjItem = null;
					}
					var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
					integrationInfo.Data.SetProperty(path, jObjItem);
				}
			}
		}
		//Log key=Mapper
		protected virtual void StartMappExportResponseProcessByConfig(IntegrationInfo integrationInfo, string jName, List<MappingItem> mapConfig)
		{
			foreach (var item in mapConfig)
			{
				try
				{
					if (item.SaveOnResponse)
					{
						var path = IntegrationPath.GeneratePath(jName, item.JSourcePath);
						if (item.IsArrayItem)
						{
							var subJObjs = integrationInfo.Data.GetSubObjects(path);
							subJObjs.ForEach(subJObj =>
							{
								MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
							});
						}
						else
						{
							var subJObj = integrationInfo.Data.GetSubObject(path);
							MapColumn(integrationInfo.UserConnection, item, ref subJObj, integrationInfo);
						}
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
					if (CsConstant.IntegrationFlagSetting.AllowErrorOnColumnAssign)
					{
						throw;
					}
				}
			}
		}
		//Log key=Mapper
		protected virtual void MapColumn(UserConnection userConnection, MappingItem mapItem, ref IIntegrationObject jToken, IntegrationInfo integrationInfo)
		{
			try
			{
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
								userConnection = userConnection,
								integrationType = integrationInfo.IntegrationType,
								action = integrationInfo.Action
							};
							executedMethod = () => rule.Import((RuleImportInfo)ruleInfo);
							if (mapItem.MapExecuteType == TMapExecuteType.BeforeEntitySave)
							{
								executedMethod();
								var importRuleInfo = ruleInfo as RuleImportInfo;
								if (importRuleInfo.AfterEntitySave != null)
								{
									MethodQueue.Enqueue(importRuleInfo.AfterEntitySave);
								}
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
								userConnection = userConnection,
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
				IntegrationLogger.ErrorMapping(MappingErrorLoggerInfo.GetMessage(e, mapItem, integrationInfo));
			}
		}
		//Log key=Mapper
		protected virtual void ExecuteOverRuleMacros(MappingItem mapItem, ref IIntegrationObject jToken, IntegrationInfo integrationInfo)
		{
			if (mapItem.OverRuleMacros.IsNullOrEmpty() || (!mapItem.AllowNullToOverMacros && jToken == null))
			{
				return;
			}
			switch (integrationInfo.IntegrationType)
			{
				case TIntegrationType.ExportResponseProcess:
				case TIntegrationType.Import:
					jToken = MacrosFactory.GetMacrosResultImport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo) as IIntegrationObject;
					break;
				case TIntegrationType.Export:
					jToken.SetObject(MacrosFactory.GetMacrosResultExport(mapItem.OverRuleMacros, jToken, MacrosType.OverRule, integrationInfo));
					break;
			}
		}
		//Log key=Mapper
		public virtual void ExecuteMapMethodQueue()
		{
			while (MethodQueue.Any())
			{
				var method = MethodQueue.Dequeue();
				try
				{
					method();
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
	}
}
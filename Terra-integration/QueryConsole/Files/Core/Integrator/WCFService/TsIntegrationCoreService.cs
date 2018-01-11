using System;
using System.Web;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Terrasoft.Core;
using System.Runtime.Serialization;
using ServiceStack.ServiceInterface.ServiceModel;
using Terrasoft.Core.Factories;
using Terrasoft.Nui.ServiceModel.DataContract;

namespace Terrasoft.TsIntegration.Configuration
{
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TsIntegrationCoreService
	{
		private global::Common.Logging.ILog _log;

		public global::Common.Logging.ILog Log {
			get {
				if (_log == null)
				{
					_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
						global::Common.Logging.LogManager.GetLogger("Common");
				}
				return _log;
			}
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public MappingServiceResponse SaveMappingConfig(string config, string action, Guid Id)
		{
			var response = new MappingServiceResponse();
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				switch (action)
				{
					case "create":
						var id = helper.CreateNewConfig(config, Id);
						if (id != Guid.Empty)
						{
							response.Data = id.ToString();
						}
						break;
					case "update":
						helper.UpdateConfig(config, Id);
						break;
				}
				helper.ReinitSettings();
			}
			catch (Exception e)
			{
				Log.Error(e);
				response.Exception = e;
			}
			return response;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string GetMappingConfig(Guid Id)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.GetConfigByJson(Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string TestToJson(TestExportInfo testConfig)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.TestToJson(testConfig);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string TestToEntity(TestImportInfo testConfig)
		{
			try
			{
				var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
				var helper = new TsIntegrationCodeServiceHelper(userConnection);
				return helper.TestToEntity(testConfig);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			return string.Empty;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public List<EntitySchemaInfo> GetAllEntityInfo()
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			return helper.GetAllEntityNames();
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public string GetLogBlockForAnalyse(Guid blockId)
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			return helper.GetBlockLogDataForAnalyze(blockId);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public void TestServiceByMock(TestServiceInfo info)
		{
			var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
			var helper = new TsIntegrationCodeServiceHelper(userConnection);
			helper.TestServiceByMock(info);
		}
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public void RunTrigger(TriggerServiceInfo info)
		{
			LoggerHelper.DoInLogBlock("Run Trigger " + info.TriggerName, () =>
			{
				try
				{
					var userConnection = (UserConnection)HttpContext.Current.Session["UserConnection"];
					var triggerEngine = new TriggerEngine();
					var triggerInfo = triggerEngine.GetTriggerByName(info.TriggerName, userConnection);
					if (triggerInfo != null)
					{
						var integrator = ClassFactory.Get<BaseIntegrator>();
						integrator.ExportWithRequest(info.PrimaryColumnValue, info.SchemaName, triggerInfo.Route);
					}
					else
					{
						IntegrationLogger.Error(string.Format("Trigger {0} not found!", info.TriggerName));
					}
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			});
		}
	}

}
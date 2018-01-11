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
	public static class CsConstant
	{
		//IsDebugMode = true только для QueryConsole.
		public static bool IsDebugMode = false;
		public class IntegrationResult
		{
			public bool Success {
				get;
				set;
			}
			public IIntegrationObject Data {
				get;
				set;
			}
			public TResultType Type {
				get;
				set;
			}
			public TResultException Exception {
				get;
				set;
			}
			public string ExceptionMessage {
				get;
				set;
			}
			public IntegrationResult()
			{

			}
			public IntegrationResult(IIntegrationObject data)
			{
				Data = data;
			}
			public IntegrationResult(TResultType type, IIntegrationObject data = null)
			{
				Type = type;
				Data = data;
			}

			public IntegrationResult(TResultException exception, string message = null, IIntegrationObject data = null)
			{
				Type = TResultType.Exception;
				Exception = exception;
				ExceptionMessage = message;
				Data = data;
			}


			public enum TResultException
			{
				OnCreateEntityExist
			}
			public enum TResultType
			{
				Exception,
				Success
			}

		}


		public class IntegrationInfo
		{

			public IIntegrationObject Data {
				get;
				set;
			}
			public string StrData {
				get;
				set;
			}
			public TIntegrationType IntegrationType {
				get;
				set;
			}
			public string Action {
				get;
				set;
			}
			public IntegrationResult Result {
				get;
				set;
			}
			public Entity IntegratedEntity {
				get;
				set;
			}
			public string TsExternalIdPath {
				get;
				set;
			}
			public string TsExternalVersionPath {
				get;
				set;
			}
			public BaseEntityHandler Handler {
				get;
				set;
			}
			public Entity ParentEntity {
				get;
				set;
			}


			public IntegrationInfo(IIntegrationObject data, TIntegrationType integrationType = TIntegrationType.Export,
				string action = "Create", Entity integratedEntity = null)
			{
				Data = data;
				IntegrationType = integrationType;
				Action = action;
				IntegratedEntity = integratedEntity;
			}


			public override string ToString()
			{
				return string.Format("Data = {0}\nIntegrationType={1}", Data, IntegrationType);
			}


			public static IntegrationInfo CreateForImport(string action, IIntegrationObject data)
			{
				return new IntegrationInfo(data, TIntegrationType.Import, action, null);
			}
			public static IntegrationInfo CreateForExport(Entity entity)
			{
				return new IntegrationInfo(null, TIntegrationType.Export, CsConstant.IntegrationActionName.Empty, entity);
			}
			public static IntegrationInfo CreateForResponse(Entity entity)
			{
				return new IntegrationInfo(null, TIntegrationType.ExportResponseProcess, CsConstant.IntegrationActionName.UpdateFromResponse, entity);
			}
		}

		public enum TIntegrationType
		{
			Export = 0,
			Import = 1,
			All = 3,
			ExportResponseProcess = 4
		}
		public static class IntegrationActionName
		{
			public const string Create = @"create";
			public const string Update = @"update";
			public const string Delete = @"delete";
			public const string UpdateFromResponse = @"updateFromResponse";
			public const string Empty = @"";
		}
		public static class IntegrationFlagSetting
		{
			public const bool AllowErrorOnColumnAssign = false;
		}
		public static class TsRequestType
		{
			public static readonly Guid Push = new Guid("bda8d5fb-3c8f-41c6-9823-44615ab20596");
			public static readonly Guid GetResponse = new Guid("173dc5c7-0d32-4512-86b8-e91691b22c19");
		}

		public static class PersonName
		{
			public const string Bpm = @"Bpm`online";
			public const string ClientService = @"Client Service";
			public const string IntegrationService = @"Integration Service";
			public const string OrderService = @"Order Service";
			public const string Unknown = @"Unknown";
		}
		public static class TsRequestStatus
		{
			public static readonly Guid Success = new Guid("5a0d25f5-d718-45ab-b4e3-d615ef7e09c6");
			public static readonly Guid Error = new Guid("88c5e88e-410d-4d67-99c3-722d92f93631");
		}

		public static class LoggerSettings
		{
			public static bool IsLoggedStackTrace = false;
			public static bool IsLoggedDbActive = false;
			public static bool IsLoggedFileActive = true;
		}

		public static class IntegratorSettings
		{
			public static bool IsIntegrationAsync = false;
			public static bool isLockerActive = true;
			public static Dictionary<Type, IntegratorSetting> Settings = new Dictionary<Type, IntegratorSetting>()
			{

			};

			#region Class: Setting
			public class IntegratorSetting
			{
				public string Name;
				public string Auth;
				public bool IsIntegratorActive;
				public bool IsDebugMode;
				public TIntegrationObjectType ObjectType;
			}
			#endregion
		}

		public static class IntegartionObjectTypeIds
		{
			public static Guid Json = new Guid("5D304534-9D7D-467C-918F-95264BF29295");
			public static Guid Xml = new Guid("C5AC9B2B-A50E-41C8-B5E1-9035CDBB24B8");
		}
	}
}
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
	public class TsLogger
	{
		private global::Common.Logging.ILog _log;
		private global::Common.Logging.ILog _updateLog;
		private global::Common.Logging.ILog _emptyLog;

		public global::Common.Logging.ILog Instance {
			get {
				return _log ?? _emptyLog;
			}
		}
		public global::Common.Logging.ILog UpdateInstance {
			get {
				return _updateLog ?? _emptyLog;
			}
		}

		public UserConnection userConnection;

		public TsLogger()
		{
			_log = global::Common.Logging.LogManager.GetLogger("TscIntegration") ??
				   global::Common.Logging.LogManager.GetLogger("Common");
			_updateLog = global::Common.Logging.LogManager.GetLogger("TscUpdateIntegration");
			_emptyLog = global::Common.Logging.LogManager.GetLogger("NotExistingLogger");
		}
		public void CreateBlock(string blockName, Guid blockId, Guid parentBlockId, TLogObjectType type = TLogObjectType.Block)
		{
			SetBlockType(Instance, type);
			SetBlockId(Instance, blockId);
			SetParentBlockId(Instance, parentBlockId);
			SetLoggerVariable(UpdateInstance, "CreatedDate", DateTime.UtcNow);
			Instance.Info(blockName);
		}

		public void UpdateBlockTime(Guid blockId, DateTime endDateTime)
		{
			SetLoggerVariable(UpdateInstance, "FinishDateTime", endDateTime.ToUniversalTime());
			SetBlockId(UpdateInstance, blockId);
			UpdateInstance.Info(string.Empty);
		}
		public void Error(Guid blockId, string errorMessage, TLogObjectType type = TLogObjectType.Error)
		{
			try
			{
				if (blockId == Guid.Empty)
				{
					return;
				}
				SetBlockType(Instance, type);
				SetParentBlockId(Instance, blockId);
				SetBlockId(Instance, Guid.NewGuid());
				Instance.Info(errorMessage);
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}
		public void ErrorMapping(Guid blockId, string errorMessage)
		{
			Error(blockId, errorMessage, TLogObjectType.ErrorMapping);
		}
		public void Info(Guid blockId, string message, TLogObjectType type = TLogObjectType.Info)
		{
			try
			{
				if (blockId == Guid.Empty)
				{
					return;
				}
				SetBlockType(Instance, type);
				SetParentBlockId(Instance, blockId);
				SetBlockId(Instance, Guid.NewGuid());
				Instance.Info(message);
			}
			catch (Exception e)
			{
				Instance.Info(e.ToString());
			}
		}
		public void InfoMapping(Guid blockId, string message)
		{
			Info(blockId, message, TLogObjectType.InfoMapping);
		}
		public static void SetBlockId(global::Common.Logging.ILog Instance, Guid id)
		{
			Instance.ThreadVariablesContext.Set("BlockId", id.ToString("B").ToUpper());
		}
		public static void SetParentBlockId(global::Common.Logging.ILog Instance, Guid id)
		{
			Instance.ThreadVariablesContext.Set("ParentBlockId", id.ToString("B").ToUpper());
		}
		public static void SetBlockType(global::Common.Logging.ILog Instance, TLogObjectType type)
		{
			Instance.ThreadVariablesContext.Set("Type", (int)type);
		}

		public static void SetLoggerVariable(global::Common.Logging.ILog Instance, string name, object value)
		{
			Instance.ThreadVariablesContext.Set(name, value);
		}
	}
}
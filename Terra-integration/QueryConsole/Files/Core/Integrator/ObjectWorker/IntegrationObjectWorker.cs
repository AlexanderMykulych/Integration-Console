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
	public class IntegrationObjectWorker : IIntegrationObjectWorker
	{
		//Log key=Integration Service
		public virtual IIntegrationObject Get(BaseEntityHandler handler, Entity entity)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForExport(entity);
			integrationInfo.Handler = handler;
			return handler.ToJson(integrationInfo);
		}
		//Log key=Integration Service
		public virtual void Import(IServiceHandlerWorkers handlerWorker, ConfigSetting handlerConfig, IIntegrationObject iObject,
			Action<CsConstant.IntegrationInfo> onSuccess = null, Action<CsConstant.IntegrationInfo, Exception> onError = null)
		{
			var integrationInfo = CsConstant.IntegrationInfo.CreateForImport(CsConstant.IntegrationActionName.Create, iObject);
			try
			{
				integrationInfo.Handler = handlerWorker.GetWithConfig(handlerConfig.Handler, handlerConfig);
				Import(integrationInfo);
				if (onSuccess != null)
				{
					onSuccess(integrationInfo);
				}
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
				if (onError != null)
				{
					onError(integrationInfo, e);
				}
			}
		}
		//Log key=Integration Service
		protected virtual void Import(CsConstant.IntegrationInfo integrationInfo)
		{
			var handler = integrationInfo.Handler;
			if (handler == null)
			{
				IntegrationLogger.Warning(string.Format("Обработчик не найден!\n{0}", integrationInfo));
				return;
			}
			string key = handler.GetKeyForLock(integrationInfo);
			LockerHelper.DoWithLock(key, () =>
			{
				if (!handler.IsEntityAlreadyExist(integrationInfo))
				{
					handler.Create(integrationInfo);
				}
				else
				{
					integrationInfo.Action = CsConstant.IntegrationActionName.Update;
					handler.Update(integrationInfo);
					return;
				}
			}, IntegrationLogger.SimpleLoggerErrorAction, throwException: true);
		}
	}
}
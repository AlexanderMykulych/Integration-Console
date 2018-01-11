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
	public class IntegrationEntityHelper
	{
		private static List<Type> IntegrationEntityTypes { get; set; }
		private static ConcurrentDictionary<Type, BaseEntityHandler> EntityHandlers { get; set; }
		private static Type DefaultHandlerType;
		public IntegrationEntityHelper()
		{
			EntityHandlers = new ConcurrentDictionary<Type, BaseEntityHandler>();
			RegisterDefaultHandler();
		}
		/// <summary>
		/// Експортирует или импортирует объекты в зависимости от настроек
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		public void IntegrateEntity(IntegrationInfo integrationInfo)
		{
			ExecuteHandlerMethod(integrationInfo);
		}
		/// <summary>
		/// В зависимости от типа интеграции возвращает соответсвенный атрибут
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>

		/// <summary>
		/// Возвращает все классы помеченые атрибутами интеграции которые розмещены в пространстве имен Terrasoft.Configuration
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <returns></returns>
		public List<Type> GetIntegrationTypes(IntegrationInfo integrationInfo)
		{
			return GetIntegrationTypes(integrationInfo.IntegrationType);
		}
		public List<Type> GetIntegrationTypes(CsConstant.TIntegrationType integrationType)
		{
			if (IntegrationEntityTypes != null && IntegrationEntityTypes.Any())
			{
				return IntegrationEntityTypes;
			}
			var attributeType = typeof(IntegrationHandlerAttribute);
			var assembly = typeof(BaseEntityHandler).Assembly;
			return IntegrationEntityTypes = assembly.GetTypes().Where(x =>
			{
				var attributes = x.GetCustomAttributes(attributeType, true);
				return attributes != null && attributes.Length > 0;
			}).ToList();
		}
		public List<BaseEntityHandler> GetAllIntegrationHandler(List<ConfigSetting> handlerConfigs)
		{
			var handlers = new List<BaseEntityHandler>();
			foreach (var handlerConfig in handlerConfigs)
			{
				var attrType = typeof(IntegrationHandlerAttribute);
				//var handlerType = SettingsManager
				//	.Handlers
				//	.FirstOrDefault(x => x.GetCustomAttributes(attrType, true).Any(y => ((IntegrationHandlerAttribute)y).Name == handlerConfig.Handler));
				//if (handlerType != null)
				//{
				//	var handler = Activator.CreateInstance(handlerType, handlerConfig) as BaseEntityHandler;
				//	handlers.Add(handler);
				//}
			}
			return handlers;
		}
		/// <summary>
		/// В зависимости от настройки интеграции, выполняет соответсвенный метод объкта, который отвечает за интеграцию конкретной сущности
		/// </summary>
		/// <param name="integrationInfo">Настройки интеграции</param>
		/// <param name="handler">объект, который отвечает за интеграцию конкретной сущности</param>
		public void ExecuteHandlerMethod(IntegrationInfo integrationInfo)
		{
			BaseEntityHandler handler = integrationInfo.Handler;
			if (handler != null)
			{
				//Id - для уникальной блокировки интеграции. Блокируем по Id, EntityName, ServiceName и JName
				string key = handler.GetKeyForLock(integrationInfo);
				try
				{
					LockerHelper.DoWithLock(key, () => {
						//Export
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.Export)
						{
							if (handler.IsExport(integrationInfo))
							{
								var result = new CsConstant.IntegrationResult(CsConstant.IntegrationResult.TResultType.Success, handler.ToJson(integrationInfo));
								integrationInfo.Result = result;
							}
							return;
						}
						//Export on Response
						if (integrationInfo.IntegrationType == CsConstant.TIntegrationType.ExportResponseProcess)
						{
							handler.ProcessResponse(integrationInfo);
							return;
						}
						//Import
						if (integrationInfo.Action == CsConstant.IntegrationActionName.Create)
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
						}
						else if (integrationInfo.Action == CsConstant.IntegrationActionName.Update)
						{
							if (handler.IsEntityAlreadyExist(integrationInfo))
							{
								handler.Update(integrationInfo);
							}
							else
							{
								integrationInfo.Action = CsConstant.IntegrationActionName.Create;
								handler.Create(integrationInfo);
							}
						}
						else if (integrationInfo.Action == CsConstant.IntegrationActionName.Delete)
						{
							handler.Delete(integrationInfo);
						}
						else
						{
							handler.Unknown(integrationInfo);
						}
					}, IntegrationLogger.SimpleLoggerErrorAction, string.Format("{0}", handler.EntityName));
				}
				catch (Exception e)
				{
					IntegrationLogger.Error(e);
				}
			}
		}
		/// <summary>
		/// Регистрирует обработчик интеграции по умолчанию
		/// Если таких несколько, то возвращает обработчинк с большим полем Index в атрибуте
		/// </summary>
		public static void RegisterDefaultHandler()
		{
			if (DefaultHandlerType != null)
			{
				return;
			}
			var assembly = typeof(IntegrationEntityHelper).Assembly;
			var attrType = typeof(DefaultHandlerAttribute);
			var defaultHandlerItem = assembly
				.GetTypes()
				.Where(x => x.HasAttribute(attrType))
				.Select(x => new
				{
					type = x,
					attr = x.GetCustomAttributes(attrType, false).FirstOrDefault() as DefaultHandlerAttribute
				})
				.Where(x => x.attr != null)
				.OrderByDescending(x => x.attr.Index)
				.FirstOrDefault();
			if (defaultHandlerItem != null)
			{
				DefaultHandlerType = defaultHandlerItem.type;
			}
		}
	}
}
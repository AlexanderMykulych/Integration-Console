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
	public class HandlerFactory
	{
		public static bool IsRegistred = false;
		public static Type HandlerAttrType = typeof(IntegrationHandlerAttribute);
		public static ConcurrentDictionary<string, Type> Handlers;
		public static void Register()
		{
			if (!IsRegistred)
			{
				var handlerDictionary = typeof(HandlerFactory)
					.Assembly
					.GetTypes()
					.Where(x => x.GetCustomAttributes(HandlerAttrType, true).Any())
					.Select(x => new
					{
						key = (x.GetCustomAttributes(HandlerAttrType, true).First() as IntegrationHandlerAttribute).Name,
						value = x
					})
					.Where(x => x.value != null)
					.ToDictionary(x => x.key, x => x.value);
				Handlers = new ConcurrentDictionary<string, Type>(handlerDictionary);
				IsRegistred = true;
			}
		}
		public static BaseEntityHandler Get(string name, ConfigSetting config)
		{
			Register();
			if (Handlers != null && Handlers.ContainsKey(name))
			{
				return Activator.CreateInstance(Handlers[name], config) as BaseEntityHandler;
			}
			return null;
		}
	}
}
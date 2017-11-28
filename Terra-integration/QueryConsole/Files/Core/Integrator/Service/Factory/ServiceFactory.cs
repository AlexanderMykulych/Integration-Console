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
	public static class ServiceFactory
	{
		public static bool IsRegistred = false;
		public static Type ServiceAttrType = typeof(IntegrationServiceAttribute);
		public static ConcurrentDictionary<string, IIntegrationService> Services;
		public static void Register()
		{
			if (!IsRegistred)
			{
				var serviceDictionary = typeof(ServiceFactory)
					.Assembly
					.GetTypes()
					.Where(x => x.GetCustomAttributes(ServiceAttrType, false).Any())
					.Select(x => new
					{
						key = (x.GetCustomAttributes(ServiceAttrType, true).First() as IntegrationServiceAttribute).Name,
						value = Activator.CreateInstance(x) as IIntegrationService
					})
					.Where(x => x.value != null)
					.ToDictionary(x => x.key, x => x.value);
				Services = new ConcurrentDictionary<string, IIntegrationService>(serviceDictionary);
				IsRegistred = true;
			}
		}
		public static IIntegrationService Get(string name)
		{
			Register();
			if (Services != null && Services.ContainsKey(name))
			{
				return Services[name];
			}
			return null;
		}
	}
}
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
	public static class MacrosFactory
	{
		public static bool IsMacrosRegistred = false;
		public static Dictionary<string, Func<object, object>> MacrosDictImport = new Dictionary<string, Func<object, object>>() { };
		public static Dictionary<string, Func<object, object>> MacrosDictExport = new Dictionary<string, Func<object, object>>() { };
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictImport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() { };
		public static Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>> OverMacrosDictExport = new Dictionary<string, Func<object, CsConstant.IntegrationInfo, object>>() { };
		//TODO доделать реестрацию макросов "перед удалением"
		public static Dictionary<string, Action<object, UserConnection>> BeforeDeleteMacros = new Dictionary<string, Action<object, UserConnection>>() { };
		public static void RegisterMacros()
		{
			if (IsMacrosRegistred)
			{
				return;
			}
			var assembly = typeof(MacrosFactory).Assembly;
			var ruleAttrType = typeof(MacrosImportAttribute);
			var ruleExportType = typeof(MacrosExportAttribute);
			try
			{
				assembly
					.GetTypes()
					.Where(x => x.HasAttribute(ruleAttrType) || x.HasAttribute(ruleExportType))
					.ForEach(x =>
					{
						var attributess = x.GetCustomAttributes(ruleAttrType, true).ToList();
						attributess.AddRange(x.GetCustomAttributes(ruleExportType, true).ToList());
						if (attributess == null || !attributess.Any())
						{
							return;
						}
						attributess
							.ForEach(attr =>
							{
								if (attr is MacrosImportAttribute)
								{
									var macrosAttr = (MacrosImportAttribute)attr;
									if (macrosAttr.Type == MacrosType.Rule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosCreator;
										if (macrosCreator != null)
										{
											MacrosDictImport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
									else if (macrosAttr.Type == MacrosType.OverRule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosOverRuleCreator;
										if (macrosCreator != null)
										{
											OverMacrosDictImport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
								}
								else
								{
									var macrosAttr = (MacrosExportAttribute)attr;
									if (macrosAttr.Type == MacrosType.Rule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosCreator;
										if (macrosCreator != null)
										{
											MacrosDictExport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
									else if (macrosAttr.Type == MacrosType.OverRule)
									{
										var macrosCreator = Activator.CreateInstance(x) as IMacrosOverRuleCreator;
										if (macrosCreator != null)
										{
											OverMacrosDictExport.Add(macrosAttr.Name, macrosCreator.Create());
										}
									}
								}
							});
					});
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
			IsMacrosRegistred = true;
		}

		public static object GetMacrosResultImport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			RegisterMacros();
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictImport.ContainsKey(macrosName) && MacrosDictImport[macrosName] != null)
					{
						return MacrosDictImport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictImport.ContainsKey(macrosName) && OverMacrosDictImport[macrosName] != null)
					{
						return OverMacrosDictImport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}

		}
		public static object GetMacrosResultExport(string macrosName, object value, MacrosType type = MacrosType.Rule, CsConstant.IntegrationInfo integrationInfo = null)
		{
			RegisterMacros();
			switch (type)
			{
				case MacrosType.Rule:
					if (MacrosDictExport.ContainsKey(macrosName) && MacrosDictExport[macrosName] != null)
					{
						return MacrosDictExport[macrosName](value);
					}
					return value;
				case MacrosType.OverRule:
					if (OverMacrosDictExport.ContainsKey(macrosName) && OverMacrosDictExport[macrosName] != null)
					{
						return OverMacrosDictExport[macrosName](value, integrationInfo);
					}
					return value;
				default:
					return value;
			}
		}
		public static void ExecuteBeforeDeleteMacros(string macrosName, object value, UserConnection userConnection)
		{
			RegisterMacros();
			if (BeforeDeleteMacros.ContainsKey(macrosName))
			{
				BeforeDeleteMacros[macrosName](value, userConnection);
			}
		}
	}
}
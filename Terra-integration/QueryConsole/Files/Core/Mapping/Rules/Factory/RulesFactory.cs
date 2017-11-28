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
	public class RulesFactory
	{
		private TIntegrationObjectType? _objectType;
		public TIntegrationObjectType ObjectType {
			get {
				if (_objectType == null)
				{
					try
					{
						_objectType = SettingsManager.GetIntegratorSetting<TIntegrationObjectType>("TsIntegrationObjectType");
					}
					catch (Exception e)
					{
						IntegrationLogger.Error(e);
					}
					if (_objectType == null)
					{
						_objectType = TIntegrationObjectType.Json;
					}
				}
				return _objectType.Value;
			}
		}
		public RulesFactory()
		{
			RegisterRules();
		}

		/// <summary>
		/// Возвращает правила по его имени
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IMappRule GetRule(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			var factoryItem = Rules.FirstOrDefault(x => x.Attribute.Name == name.ToLower() && x.Attribute.DataType == ObjectType);
			return factoryItem == null ? null : factoryItem.Rule;
		}

		#region Static Field
		/// <summary>
		/// Правыла маппинга
		/// </summary>
		public static List<RuleFactoryItem> Rules;
		/// <summary>
		/// Признак что правила прошли реестрацию
		/// </summary>
		public static bool IsRuleRegister = false;
		/// <summary>
		/// Ищет по сборке все классы с атрибутом RuleAttribute и для каждого атрибута
		/// создает инстанс правила
		/// </summary>
		public static void RegisterRules()
		{
			if (Rules == null)
			{
				Rules = new List<RuleFactoryItem>();
			}
			if (IsRuleRegister)
			{
				return;
			}
			var assembly = typeof(RulesFactory).Assembly;
			var ruleAttrType = typeof(RuleAttribute);
			assembly
				.GetTypes()
				.Where(x => x.HasAttribute(ruleAttrType))
				.ForEach(x =>
				{
					var attributess = x.GetCustomAttributes(ruleAttrType, true);
					if (attributess == null || attributess.Length == 0)
					{
						return;
					}
					attributess
						.Where(attr => AttributeDataValidate(attr as RuleAttribute))
						.ForEach(attr => Rules.Add(new RuleFactoryItem((RuleAttribute)attr, CreateRuleInstanse(x))));
				});
			IsRuleRegister = true;
		}
		/// <summary>
		/// Проверяет правило на соответсвия настройкам интеграции
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool AttributeDataValidate(RuleAttribute attribute)
		{
			return true;
		}
		/// <summary>
		/// Создает инстанс правила по его типу
		/// </summary>
		/// <param name="ruleType"></param>
		/// <returns></returns>
		public static IMappRule CreateRuleInstanse(Type ruleType)
		{
			return Activator.CreateInstance(ruleType) as IMappRule;
		}
		#endregion
	}
}
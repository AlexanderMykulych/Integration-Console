using System;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration
{
	public class MappingConfigTriggerCollection : ITriggerCollection<Entity>
	{
		private TriggerCheckerCollection _instance;
		private ISettingProvider _settingProvider;
		public MappingConfigTriggerCollection(UserConnection userConnection, TriggerCheckerCollection obj)
		{
			_instance = obj;
			LoadAllChecker(userConnection);
			_settingProvider = ObjectFactory.Get<ISettingProvider>();
		}
		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			return _instance.Check(eventName, eventInfo, onMath);
		}

		public TriggerSetting GetSetting()
		{
			return _instance.GetSetting();
		}

		public void LoadAllChecker(UserConnection userConnection)
		{
			try
			{
				var triggersConfig = _settingProvider.Get("TriggerConfig").SelectFromList<TriggerSetting>();
				_instance.AddRange(triggersConfig.Select(x => new TriggerCheckerItem(x)));
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e);
			}
		}

		public TriggerSetting GetSettingByName(string name)
		{
			return _instance.GetSettingByName(name);
		}
	}
}

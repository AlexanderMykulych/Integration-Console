using System;
namespace Terrasoft.TsIntegration.Configuration
{
	public interface ITriggerCheckerItem<T>
	{
		bool Check(string eventName, T eventInfo, Action<TriggerSetting> onMath);
		TriggerSetting GetSetting();
	}
}

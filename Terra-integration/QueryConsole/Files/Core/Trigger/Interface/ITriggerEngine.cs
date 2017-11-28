using Terrasoft.Core;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface ITriggerEngine<T>
	{
		void Push(string eventName, T eventInfo);
		TriggerSetting GetTriggerByName(string name, UserConnection userConnection);
	}
}

using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;

namespace Terrasoft.TsIntegration.Configuration
{
	public class TriggerEngine : ITriggerEngine<Entity>
	{
		public static ITriggerCollection<Entity> _triggerCollection;
		public static object _lock = new object();
		public static object _refreshLock = new object();
		//Log name=TriggerEngine, key=Trigger, throw
		public void Push(string eventName, Entity eventInfo)
		{
			var triggerCollection = GetTriggerCollection(eventInfo.UserConnection);
			triggerCollection.Check(eventName, eventInfo, triggerInfo =>
			{
				LoggerHelper.DoInLogBlock("Find Trigger " + triggerInfo.Caption, () =>
				{
					var integrator = ClassFactory.Get<BaseIntegrator>();
					integrator.ExportWithRequest(eventInfo.PrimaryColumnValue, eventInfo.SchemaName, triggerInfo.Route);
				});
			});
		}

		public TriggerSetting GetTriggerByName(string name, UserConnection userConnection)
		{
			return GetTriggerCollection(userConnection).GetSettingByName(name);
		}

		public ITriggerCollection<Entity> GetTriggerCollection(UserConnection userConnection)
		{
			if (_triggerCollection != null)
			{
				return _triggerCollection;
			}
			lock (_lock)
			{
				if (_triggerCollection != null)
				{
					return _triggerCollection;
				}
				_triggerCollection = new MappingConfigTriggerCollection(userConnection, new TriggerCheckerCollection());
				return _triggerCollection;
			}
		}

		public static void ClearTriggerCollection()
		{
			_triggerCollection = null;
		}
	}
}

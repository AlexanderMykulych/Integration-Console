using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;

namespace Terrasoft.TsIntegration.Configuration
{
	public class TriggerCheckerCollection : ITriggerCollection<Entity>
	{
		List<ITriggerCheckerItem<Entity>> _items;
		public TriggerCheckerCollection()
		{
			_items = new List<ITriggerCheckerItem<Entity>>();
		}
		public bool Check(string eventName, Entity eventInfo, Action<TriggerSetting> onMath)
		{
			bool isCheked = false;
			_items.ForEach(x => {
				if (x.Check(eventName, eventInfo, onMath))
				{
					isCheked = true;
				}
			});
			return isCheked;
		}

		public TriggerSetting GetSetting()
		{
			return null;
		}

		public void Add(ITriggerCheckerItem<Entity> entityChecker)
		{
			_items.Add(entityChecker);
		}
		public void AddRange(IEnumerable<ITriggerCheckerItem<Entity>> entityCheckers)
		{
			_items.AddRange(entityCheckers);
		}

		public virtual TriggerSetting GetSettingByName(string name)
		{
			var trigger = _items.FirstOrDefault(x => x.GetSetting().Name == name);
			if (trigger != null)
			{
				return trigger.GetSetting();
			}
			return null;
		}
	}
}

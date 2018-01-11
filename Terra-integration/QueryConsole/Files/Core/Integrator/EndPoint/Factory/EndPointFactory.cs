using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class EndPointFactory: BaseAttributeFactory<IEndPointHandler, EndPointAttribute, string>
	{
		private static Dictionary<EndPointAttribute, IEndPointHandler> _instances;

		protected override Dictionary<EndPointAttribute, IEndPointHandler> Instances
		{
			get
			{
				if (_instances == null)
				{
					_instances = new Dictionary<EndPointAttribute, IEndPointHandler>();
				}
				return _instances;
			}
		}
		private static bool _isRuleRegister;
		protected override bool IsRuleRegister { get { return _isRuleRegister; } set { _isRuleRegister = value; } }
		protected override bool IsInstanceNameEqual(KeyValuePair<EndPointAttribute, IEndPointHandler> instanceKeyValue, string name)
		{
			return instanceKeyValue.Key.Name == name;
		}

		protected override void RegisterRule(EndPointAttribute attr, IEndPointHandler instance)
		{
			var settingProvider = ObjectFactory.Get<ISettingProvider>();
			var endPointConfig = settingProvider.SelectFirstByType<EndPointConfig>(x => x.Name == attr.Name);
			if (endPointConfig != null)
			{
				instance.Config = endPointConfig.HandlerConfigs;
			}
			base.RegisterRule(attr, instance);
		}
	}
}

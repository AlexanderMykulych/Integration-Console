using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class ExceptionProcessStrategyFactory: BaseAttributeFactory<IExceptionProcessStrategy, ExceptionProcessStrategyAttribute, string>
	{
		private static Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> _instances;
		protected override Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> Instances {
			get {
				if (_instances == null)
				{
					_instances = new Dictionary<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy>();
				}
				return _instances;
			}
		}
		private static bool _isRuleRegister;
		protected override bool IsRuleRegister { get { return _isRuleRegister; } set { _isRuleRegister = value; } }
		protected override bool IsInstanceNameEqual(KeyValuePair<ExceptionProcessStrategyAttribute, IExceptionProcessStrategy> instanceKeyValue, string name)
		{
			return instanceKeyValue.Key.Name == name;
		}
	}
}

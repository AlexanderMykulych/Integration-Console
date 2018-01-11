using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class Setting : ISetting
	{
		private readonly object _setting;

		public Setting(object setting)
		{
			_setting = setting;
		}
		public T2 Select<T1, T2>(Func<T1, T2> getter)
		{
			string message = null;
			if (_setting != null)
			{
				if (_setting is T1)
				{
					return getter((T1)_setting);
				}
				message = string.Format("Невозможно привести настройку к типу: {0}", typeof(T1));
			}
			else
			{
				message = "Настройка пустая!";
			}
			IntegrationLogger.Error(message);
			throw new Exception(message);
		}

		public IEnumerable<T2> SelectFromList<T1, T2>(Func<T1, T2> getter)
		{
			return Select<IEnumerable<T1>, IEnumerable<T2>>(x => x.Select(getter));
		}

		public T1 Select<T1>()
		{
			return Select<T1, T1>(x => x);
		}

		public IEnumerable<T1> SelectFromList<T1>()
		{
			return SelectFromList<T1, T1>(x => x);
		}
	}
}

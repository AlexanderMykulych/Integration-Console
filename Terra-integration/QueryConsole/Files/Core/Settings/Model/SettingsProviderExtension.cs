using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public static class SettingsProviderExtension
	{
		public static IEnumerable<T> SelectEnumerableByType<T>(this ISettingProvider settingProvider)
		{
			return settingProvider.Get(typeof(T).Name).SelectFromList<T>();
		}
	}
}

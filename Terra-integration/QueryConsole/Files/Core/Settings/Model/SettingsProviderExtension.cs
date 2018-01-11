using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public static class SettingsProviderExtension
	{
		private static string _globalPrefix = @"Global_";
		public static IEnumerable<T> SelectEnumerableByType<T>(this ISettingProvider settingProvider)
		{
			return settingProvider.Get(typeof(T).Name).SelectFromList<T>();
		}
		public static T SelectFirstByType<T>(this ISettingProvider settingProvider)
		{
			return SelectEnumerableByType<T>(settingProvider).FirstOrDefault();
		}
		public static T SelectFirstByType<T>(this ISettingProvider settingProvider, Func<T, bool> predicate)
		{
			return SelectEnumerableByType<T>(settingProvider).FirstOrDefault(predicate);
		}
		public static T SelectGlobalFirstByType<T>(this ISettingProvider settingProvider)
		{
			return settingProvider.Get(_globalPrefix + typeof(T).Name).SelectFromList<T>().FirstOrDefault();
		}
	}
}

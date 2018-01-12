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
			return SelectEnumerableByType<T>(settingProvider, typeof(T).Name);
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
			return SelectGlobalFirstByName<T>(settingProvider, typeof(T).Name);
		}
		public static T SelectGlobalFirstByName<T>(this ISettingProvider settingProvider, string name)
		{
			return settingProvider.Get(_globalPrefix + name).Select<T>();
		}
		public static IEnumerable<T> SelectEnumerableByType<T>(this ISettingProvider settingProvider, string name)
		{
			return settingProvider.Get(name).SelectFromList<T>();
		}
	}
}

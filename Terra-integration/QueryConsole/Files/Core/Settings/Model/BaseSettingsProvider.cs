using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseSettingsProvider : ISettingProvider
	{
		private IRepositorySettingsProvider _repositoryProvider;
		private static object _locker = new object();
		private bool _isInit = false;
		private List<string> _xmls;
		private IXmlProvider _xmlProvider;
		private IConfigManager _configManager;

		public BaseSettingsProvider(IRepositorySettingsProvider repositoryProvider, IXmlProvider xmlProvider, IConfigManager configManager)
		{
			_repositoryProvider = repositoryProvider;
			_xmlProvider = xmlProvider;
			_configManager = configManager;
		}

		public void Init()
		{
			if (!_isInit)
			{
				lock (_locker)
				{
					if (!_isInit)
					{
						var xmls = _repositoryProvider.GetXmls();
						var xmlData = ProcessXmls(xmls);
						_configManager.InitLoadConfig(xmlData);
						_isInit = true;
					}
				}
			}
		}

		private string ProcessXmls(List<string> xmls)
		{
			return _xmlProvider.MergeXmls(xmls);
		}

		public void Reinit()
		{
			_isInit = false;
			Init();
		}

		public ISetting Get(string settingName)
		{
			if (!_isInit)
			{
				Init();
				if (_isInit)
				{
					return GetUnsafe(settingName);
				}
				var message = string.Format("Try to get setting: {0}, but config is not initialize!", settingName);
				IntegrationLogger.ErrorFormat(message);
				throw new Exception(message);
			}
			return GetUnsafe(settingName);
		}

		public ISetting GetUnsafe(string settingName)
		{
			return new Setting(_configManager.IntegrationConfig.Get(settingName));
		}
	}
}

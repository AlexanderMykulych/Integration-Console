using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsIntegration.Configuration
{
	/// <summary>
	/// Repository provider
	/// </summary>
	public interface IRepositorySettingsProvider
	{
		/// <summary>
		/// Gets the integration xml configs.
		/// </summary>
		/// <returns>List of configs</returns>
		List<string> GetXmls();

		ConcurrentDictionary<string, ValueType> GetGlobalSettings();
	}
}

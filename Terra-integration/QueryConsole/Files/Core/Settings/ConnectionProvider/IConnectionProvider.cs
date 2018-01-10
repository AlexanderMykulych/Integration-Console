using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	/// <summary>
	/// Connection provider
	/// </summary>
	public interface IConnectionProvider
	{
		/// <summary>
		/// Gets this instance.
		/// </summary>
		/// <typeparam name="T">Connection type</typeparam>
		/// <returns>Connection instance</returns>
		T Get<T>() where T : class;
	}
}

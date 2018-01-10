using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BpmConnectionProvider: IConnectionProvider
	{
		public T Get<T>() where T: class 
		{
			if (typeof(T) == typeof(UserConnection))
			{
				var connection = ConnectionProvider.Get();
				if (connection is T)
				{
					return (T) connection;
				}
			}
			return null;
		}
	}
}

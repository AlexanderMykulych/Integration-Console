using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IExecutorSubscriber : ISubscriber
	{
		void Execute(string key, object message = null);
		void Enable();
		void Disable();
	}
}

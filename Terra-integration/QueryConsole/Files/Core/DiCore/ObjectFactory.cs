using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace Terrasoft.TsIntegration.Configuration
{
	public static class ObjectFactory
	{
		public static T Get<T>()
		{
			IKernel kernel = new StandardKernel(new CoreDiModule());
			return kernel.Get<T>();
		}
	}
}

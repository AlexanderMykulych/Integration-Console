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
		private static IKernel _currentKernel;
		public static IKernel CurrentKernel
		{
			set { _currentKernel = value; }
			get { return _currentKernel ?? (_currentKernel = new StandardKernel(new CoreDiModule())); }
		}

		public static T Get<T>()
		{
			return CurrentKernel.Get<T>();
		}
	}
}

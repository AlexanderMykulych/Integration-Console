using QueryConsole.Files.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.TsConfiguration;

namespace QueryConsole.Files.Integrators
{
	public class OrderServiceIntegrator : BaseServiceIntegrator
	{
		public OrderServiceIntegrator(UserConnection userConnection)
			: base(userConnection)
		{
			baseUrls = CsConstant.IntegratorSettings.Urls[this.GetType()];
			integratorHelper = new IntegratorHelper();
			UrlMaker = new ServiceUrlMaker(baseUrls);
			ServiceName = CsConstant.IntegratorSettings.Names[this.GetType()];
			Auth = "Basic YnBtb25saW5lOmJwbW9ubGluZQ==";
		}
	}
}

using QueryConsole.Files;
using QueryConsole.Files.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.CsConfiguration
{
	public class ClientServiceIntegrator : BaseServiceIntegrator
	{
		public ClientServiceIntegrator(UserConnection userConnection)
			: base(userConnection)
		{
			baseUrls = CsConstant.IntegratorSettings.Urls[this.GetType()];
			integratorHelper = new Terrasoft.TsConfiguration.IntegratorHelper();
			UrlMaker = new ServiceUrlMaker(baseUrls);
			ServiceName = CsConstant.IntegratorSettings.Names[this.GetType()];
			Auth = "Basic YnBtb25saW5lMjoxMjM0NTY=";
		}
	}
}

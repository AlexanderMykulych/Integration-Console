using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IEndPointHandler
	{
		List<EndPointHandlerConfig> Config { get; set; }
		string GetImportRoute(IIntegrationObject iObject);
	}
}

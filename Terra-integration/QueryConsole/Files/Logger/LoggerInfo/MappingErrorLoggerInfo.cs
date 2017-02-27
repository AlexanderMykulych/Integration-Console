using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsConfiguration
{
	public class MappingErrorLoggerInfo: BaseLoggerInfo
	{
		public Exception Exception;
		public MappingItem Item;
		public CsConstant.IntegrationInfo IntegrationInfo;
	}
}

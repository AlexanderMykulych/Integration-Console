using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public interface IExceptionProcessStrategy
	{
		Stream Process(ExceptionProcessData exceptionProcessData);
	}
}

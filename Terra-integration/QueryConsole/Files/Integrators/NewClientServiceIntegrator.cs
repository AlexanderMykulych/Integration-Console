using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace Terrasoft.TsConfiguration
{
	public class ClientServiceIntegrator : BaseServiceIntegrator
	{
		public ClientServiceIntegrator(UserConnection userConnection)
			: base(userConnection)
		{}
	}
}

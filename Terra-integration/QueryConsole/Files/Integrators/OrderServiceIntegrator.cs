﻿using System;
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
			baseUrls = new Dictionary<TServiceObject, string>() {
				{ TServiceObject.Dict, "http://api.order-service.bus2.auto3n.ru/v2/dict/AUTO3N" },
				{ TServiceObject.Entity, "http://api.order-service.bus2.auto3n.ru/v2/entity/AUTO3N" }
			};
			integratorHelper = new IntegratorHelper();
			UrlMaker = new ServiceUrlMaker(baseUrls);
			ServiceName = "OrderService";
			Auth = "Basic YnBtb25saW5lOmJwbW9ubGluZQ==";
		}
	}
}

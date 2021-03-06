﻿using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Model;

namespace Terrasoft.TsIntegration.Configuration.Files.Core.Strategy.Instance
{
	public class BaseStrategy<T>: IStrategy<T>
	{
		public BaseStrategy(IStrategyConfigurator strategyConfigurator, IStrategyImplementationBuilder strategyImplementationBuilder)
		{
			_strategyConfigurator = strategyConfigurator;
			_strategyImplementationBuilder = strategyImplementationBuilder;
		}
		private readonly Dictionary<string, IStrategyStepInfo> _steps = new Dictionary<string, IStrategyStepInfo>();
		private readonly IStrategyConfigurator _strategyConfigurator;
		private readonly IStrategyImplementationBuilder _strategyImplementationBuilder;

		public IStrategy<T> ConfigurateStrategy(Action<IStrategyConfigurator> builderConfigurator)
		{
			builderConfigurator(_strategyConfigurator);
			return this;
		}

		public IStrategyImplementation<T> BuildImplementation()
		{
			return _strategyImplementationBuilder.Build<T>(_steps, _strategyConfigurator);
		}
	}
}

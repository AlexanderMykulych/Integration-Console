using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseStepBuilder: IStepBuilder
	{
		private string _currentStepName;
		private readonly Dictionary<string, List<EventStep>> _eventStepMapp = new Dictionary<string, List<EventStep>>();
		public IStepBuilder OnDo(string eventName, string stepName, Action<IStepBuilder> currentStep = null)
		{
			if (string.IsNullOrEmpty(_currentStepName))
			{
				return this;
			}
			if (!_eventStepMapp.ContainsKey(_currentStepName))
			{
				_eventStepMapp.Add(_currentStepName, new List<EventStep>());
			}
			_eventStepMapp[_currentStepName].Add(new EventStep()
			{
				EventName = eventName,
				StepName = stepName
			});
			if (currentStep != null)
			{
				var oldStepName = _currentStepName;
				InitCurrentStepName(stepName);
				currentStep(this);
				InitCurrentStepName(oldStepName);
			}
			return this;
		}

		public IStepBuilder InitCurrentStepName(string stepName)
		{
			_currentStepName = stepName;
			return this;
		}

		public string GetCurrentStepName()
		{
			return _currentStepName;
		}

		public Dictionary<string, List<EventStep>> GetEventSteps()
		{
			return _eventStepMapp;
		}
	}
}

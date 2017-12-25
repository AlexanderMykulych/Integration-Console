using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public abstract class BaseStrategyStep: IStrategyStep
	{
		protected readonly IExecutorSubscriber Subscriber;

		protected BaseStrategyStep()
		{
			Subscriber = ObjectFactory.Get<IExecutorSubscriber>();
			Subscriber.Enable();
		}
		public ISubscriber On(string key, Action<object> onPublish)
		{
			Subscriber.On(key, onPublish);
			return this;
		}

		public ISubscriber Once(string key, Action<object> onPublish)
		{
			Subscriber.Once(key, onPublish);
			return this;
		}

		public ISubscriber Unsubscribe(string key)
		{
			Subscriber.Unsubscribe(key);
			return this;
		}

		public void Unsubscribe()
		{
			Subscriber.Unsubscribe();
		}

		public abstract void Execute(object input);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	public class BaseSubscriber : IExecutorSubscriber
	{
		Dictionary<string, List<Action<object>>> _publisher = new Dictionary<string, List<Action<object>>>();
		readonly Dictionary<string, List<Action<object>>> _oncePublisher = new Dictionary<string, List<Action<object>>>();
		bool _isEnabled = false;
		public ISubscriber On(string key, Action<object> onPublish)
		{
			if (!_publisher.ContainsKey(key))
			{
				_publisher[key] = new List<Action<object>>();
			}
			_publisher[key].Add(onPublish);
			return this;
		}

		public ISubscriber Unsubscribe(string key)
		{
			if (_publisher.ContainsKey(key))
			{
				_publisher.Remove(key);
			}
			return this;
		}

		public void Unsubscribe()
		{
			_publisher.Clear();
		}

		public virtual void Execute(string key, object message)
		{
			if (!_isEnabled)
			{
				return;
			}
			ExecuteOnce(key, message);
			if (_publisher.ContainsKey(key))
			{
				_publisher[key].ForEach(x =>
				{
					try
					{
						x.Invoke(message);
					}
					catch (Exception e)
					{
						ExecuteNoSafe("error", e);
					}
				});
			}
		}
		protected virtual void ExecuteNoSafe(string key, object message)
		{
			if (!_isEnabled)
			{
				return;
			}
			if (_publisher.ContainsKey(key))
			{
				_publisher[key].ForEach(x => x.Invoke(message));
			}
		}

		public void Enable()
		{
			_isEnabled = true;
		}

		public void Disable()
		{
			_isEnabled = false;
		}

		public void Dispose()
		{
			Unsubscribe();
			_publisher = null;
		}

		public ISubscriber Once(string key, Action<object> onPublish)
		{
			if (!_oncePublisher.ContainsKey(key))
			{
				_oncePublisher[key] = new List<Action<object>>();
			}
			_oncePublisher[key].Add(onPublish);
			return this;
		}
		public virtual void ExecuteOnce(string key, object message)
		{
			if (!_isEnabled)
			{
				return;
			}
			if (_oncePublisher.ContainsKey(key))
			{
				_oncePublisher[key].ForEach(x =>
				{
					try
					{
						x.Invoke(message);
					}
					catch (Exception e)
					{
						ExecuteNoSafe("error", e);
					}
				});
				_oncePublisher.Remove(key);
			}
		}
	}
}

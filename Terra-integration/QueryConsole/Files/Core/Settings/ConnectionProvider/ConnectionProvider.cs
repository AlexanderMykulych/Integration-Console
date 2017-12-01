using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terrasoft.TsIntegration.Configuration
{
	/// <summary>
	/// Static class for set connection in current thread
	/// </summary>
	public static class ConnectionProvider
	{
		private static readonly ConcurrentDictionary<int, object> _connections = new ConcurrentDictionary<int, object>();

		private static int _currentId
		{
			get
			{
				return Thread.CurrentThread.ManagedThreadId;
			}
		}

		/// <summary>
		/// Sets the specified connection.
		/// </summary>
		/// <param name="connection">The connection.</param>
		public static void Set(object connection)
		{
			var id = _currentId;
			if (_connections.ContainsKey(id))
			{
				_connections[id] = connection;
			}
			else
			{
				_connections.TryAdd(id, connection);
			}
		}

		/// <summary>
		/// Gets current connection.
		/// </summary>
		/// <returns>connection</returns>
		public static object Get()
		{
			var id = _currentId;
			object value = null;
			if (_connections.TryGetValue(id, out value))
			{
				return value;
			}
			return null;
		}
		/// <summary>
		/// Clear current connection.
		/// </summary>
		/// <returns>is clear success</returns>
		public static bool Clear()
		{
			var id = _currentId;
			if (_connections.ContainsKey(id))
			{
				object removedValue = null;
				return _connections.TryRemove(id, out removedValue);
			}
			return true;
		}

		/// <summary>
		/// Do action with set connection
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <param name="action">The action.</param>
		public static void DoWith(object connection, Action action)
		{
			try
			{
				Set(connection);
				action();
			}
			catch (Exception e)
			{
				IntegrationLogger.Error(e.ToString());
			}
			finally
			{
				Clear();
			}
		}
	}
}

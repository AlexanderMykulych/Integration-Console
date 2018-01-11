using Terrasoft.TsIntegration.Configuration;

namespace Terrasoft.Configuration
{
	using Common;
	using Core;
	using Web.Common;
	using System;

	#region Class : TsiIntegrationManager

	/// <summary>
	/// Точка инициализации интеграции
	/// </summary>
	public class TsiIntegrationManager : AppEventListenerBase
	{

		#region Properties : Protected

		protected UserConnection UserConnection {
			get;
			private set;
		}

		#endregion

		#region Methods : Protected

		/// <summary>
		/// Gets user connection from application event context.
		/// </summary>
		/// <param name="context">Application event context.</param>
		/// <returns>User connection.</returns>
		protected UserConnection GetUserConnection(AppEventContext context)
		{
			var appConnection = context.Application["AppConnection"] as AppConnection;
			if (appConnection == null)
			{
				throw new ArgumentNullOrEmptyException("AppConnection");
			}
			return appConnection.SystemUserConnection;
		}

		#endregion

		#region Methods : Public

		/// <summary>
		/// Handles application start.
		/// </summary>
		/// <param name="context">Application event context.</param>
		public override void OnAppStart(AppEventContext context)
		{
			base.OnAppStart(context);
			UserConnection = GetUserConnection(context);
			try
			{
			}
			catch (Exception)
			{
				//Catch all error
			}
		}

		#endregion

	}

	#endregion

}
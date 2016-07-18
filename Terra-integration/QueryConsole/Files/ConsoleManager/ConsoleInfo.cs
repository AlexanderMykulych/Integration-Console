using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConsole.Files
{
	public class ConsoleInfo : IConsoleTable
	{
		public ConsoleInfo(Action refresh)
		{
			Refresh = refresh;
		}
		public Action Refresh;


		private string _integrationStatus;
		public string IntegrationStatus
		{
			get
			{
				return _integrationStatus;
			}
			set
			{
				_integrationStatus = value;
				Console.Clear();
				Refresh();
			}
		}

		private int _progress;
		public int Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				_progress = value;
				Refresh();
			}
		}
		private string _requestStatus;
		public string RequestStatus
		{
			get
			{
				return _requestStatus;
			}
			set
			{
				_requestStatus = value;
			}
		}

		private string _url;
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_response = "";
				_url = value;
				RequestStatus = "Push Request";
				Refresh();
			}
		}

		private string _response;
		public string Response
		{
			get
			{
				return _response;
			}
			set
			{
				if (_response == value)
				{
					return;
				}
				_response = value;
				RequestStatus = "Process Response";
				Console.Clear();
				Refresh();
			}
		}

		private string _currentEntityName;
		public string CurrentEntityName
		{
			get
			{
				return _currentEntityName;
			}
			set
			{
				_currentEntityName = value;
				Refresh();
			}
		}

		private Dictionary<string, Trio<int, int, int>> _entityProgress = new Dictionary<string, Trio<int, int, int>>();
		public Dictionary<string, Trio<int, int, int>> EntityProgress
		{
			get
			{
				return _entityProgress;
			}
			set
			{
				_entityProgress = value;
				Refresh();
			}
		}

		private int _entityErrorProgress;
		public int EntityErrorProgress
		{
			get
			{
				return _entityErrorProgress;
			}
			set
			{
				_entityErrorProgress = value;
				Refresh();
			}
		}

		private int _summaryEntityCount;
		public int SummaryEntityCount
		{
			get
			{
				return _summaryEntityCount;
			}
			set
			{
				_summaryEntityCount = value;
				Refresh();
			}
		}

		private int _mappingError;
		public int MappingError {
			get {
				return _mappingError;
			}
			set {
				_mappingError = value;
				Refresh();
			}
		}

		public Dictionary<string, Func<object, object>> GetMapper()
		{
			return new Dictionary<string, Func<object, object>>() {
				{ "Integration Status", x => ((ConsoleInfo)x).IntegrationStatus },
				{ "Progress", x => ((ConsoleInfo)x).Progress + "%" },
				{ "Request Status", x => ((ConsoleInfo)x).RequestStatus },
				{ "Url", x => ((ConsoleInfo)x).Url },
				{ "Response", x => ((ConsoleInfo)x).Response },
				{ "Entity Name", x => {
					var value = ((ConsoleInfo)x).CurrentEntityName;
					if(string.IsNullOrEmpty(value))
						return "";
					return string.Format("{0} - {1}%", value, GetPersent(_entityProgress[value].First, _entityProgress[value].Second));
					}
				},
				{ "Entity Progress", x => ((ConsoleInfo)x).EntityProgress.Select(z => string.Format("{0} ({1} - {2} - {3})", z.Key, z.Value.First, z.Value.Second, z.Value.Third)).Aggregate((z,y) =>  z + "\n" + y)},
				{ "Mapping Error", x => ((ConsoleInfo)x).MappingError },
				{ "Save Error Count", x => ((ConsoleInfo)x).EntityErrorProgress },
				{ "Total Count", x => ((ConsoleInfo)x).SummaryEntityCount }
			};
		}

		public static int GetPersent(int all, int part)
		{
			return (part * 100) / all;
		}
	}
}

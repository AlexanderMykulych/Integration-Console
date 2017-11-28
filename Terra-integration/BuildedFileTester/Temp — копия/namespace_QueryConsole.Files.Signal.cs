using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace QueryConsole.Files.Signal {

	#region Class: SignalGenerator
	/*
		Project Path: ..\..\..\QueryConsole\Files\Signal\SignalGenerator.cs
		
	*/
	public class SignalGenerator
	{
		public string GenerateUrl() {
			throw new NotImplementedException();
		}

		public string GenerateAction() {
			throw new NotImplementedException();
		}

		public string GenerateJson() {
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: SignalHandler
	/*
		Project Path: ..\..\..\QueryConsole\Files\Signal\SignalHandler.cs
		
	*/
	public class SignalHandler
	{
		SignalGenerator Generator;
		SignalProcessor Processor;
		public void Generate() {
			throw new NotImplementedException();
		}

		public void Process() {
			throw new NotImplementedException();
		}
	}

	#endregion


	#region Class: SignalProcessor
	/*
		Project Path: ..\..\..\QueryConsole\Files\Signal\SignalProcessor.cs
		
	*/
	public class SignalProcessor
	{
		public void ProcessJson() {
			throw new NotImplementedException();
		}
	}

	#endregion

}
#tool "nuget:?package=NUnit.ConsoleRunner"

Task("Run Test")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll", new NUnit3Settings {
			Results  = new[] { new NUnit3Result { FileName = resultsFile } }б
			StopOnError = false
		});
	});

RunTarget("Run Test");
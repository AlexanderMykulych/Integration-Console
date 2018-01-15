#tool "nuget:?package=NUnit.ConsoleRunner"

Task("Run Test")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll", new NUnit3Settings {
			Timeout = 60000,
			StopOnError = false
		});
	});

RunTarget("Run Test");
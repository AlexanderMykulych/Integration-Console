#tool "nuget:?package=NUnit.ConsoleRunner"

Task("Run Test")
	.Does(() => {
		NUnit3("../bin/Release/IntegrationUnitTest.dll", new NUnit3Settings {
			OutputFile = "result.xml",
			StopOnError = false
		});
	});

RunTarget("Run Test");
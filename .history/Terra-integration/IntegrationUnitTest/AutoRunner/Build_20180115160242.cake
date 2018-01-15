#tool "nuget:?package=NUnit.ConsoleRunner"

Task("Run Test")
	.Does(() => {
		NUnit("../bin/Release/IntegrationUnitTest.dll", new NUnitSettings {
			Timeout = 60000,
			StopOnError = false
		});
	});

RunTarget("Run Test");
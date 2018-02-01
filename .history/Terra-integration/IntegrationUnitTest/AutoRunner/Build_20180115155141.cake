#tool "nuget:?package=NUnit.Runners&version=2.6.4"

Task("Run Test")
	.Does(() => {
		NUnit("./bin/Release/IntegrationUnitTest.dll", new NUnitSettings {
			Timeout = 35000,
			StopOnError = false
		});
	});
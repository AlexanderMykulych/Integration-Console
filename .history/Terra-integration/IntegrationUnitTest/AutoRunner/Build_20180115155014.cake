Task("Run Test")
	.Does(() => {
		NUnit("./bin/Release/IntegrationUnitTest.dll")
	});
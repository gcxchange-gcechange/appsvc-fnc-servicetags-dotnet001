# appsvc-fnc-servicetags-dotnet001

This function get all service tags ip address (IPV4 only) from microsoft with some service exception.
canadaeast or canadacentral need to be send has parameter. 

Service that we remove from the list are: { "AzureBackup", "AzureBotService", "AzureCognitiveSearch", "AzureCosmosDB", "AzureDatabricks", "AzureDigitalTwins", "AzureIoTHub", "AzureSignalR", "AzurePlatformIMDS", "AzureDataLake", "AzureFrontDoor.Frontend", "AzureFrontDoor.Backend", "AzureFrontDoor.FirstParty", "CognitiveServicesManagement", "HDInsight" }

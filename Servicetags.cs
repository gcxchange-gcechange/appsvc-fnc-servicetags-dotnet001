using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace appsvc_fnc_servicetags_dotnet001
{
    public static class Servicetags
    {
        [FunctionName("Servicetags")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string region = req.Query["region"];
            int regionID = 13;//region does not exist

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            region = region ?? data?.region;
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            log.LogInformation("Me region after deserialize "+region);
            switch (region)
            {
                case "canadaeast":
                    regionID = 12;
                    break;

                case "canadacentral":
                    regionID = 11;
                    break;

                default:
                    log.LogInformation($"Region is {region}{regionID}.");
                    break;
            }
            try
            {
                //download servicetags json
                using (WebClient web = new WebClient())
                {
                    string servicetagsLink = web.DownloadString("https://www.microsoft.com/en-us/download/confirmation.aspx?id=56519");
                    // Load
                    var doc = new HtmlDocument();
                    doc.LoadHtml(servicetagsLink);

                    // extracting all links
                    var downloadLink = doc.DocumentNode.SelectSingleNode("//*[@id='c50ef285-c6ea-c240-3cc4-6c9d27067d6c']");
                    HtmlAttribute att = downloadLink.Attributes["href"];

                    using (var client = new WebClient())
                    {
                        client.DownloadFile(att.Value, Path.GetTempPath() + "\\servicetags.json");
                    }
                }
            }
            catch (Exception e)
            {
                log.LogInformation("Download Fail");
                log.LogInformation(e.Message);
            }
            var sourceFile = File.ReadAllText(Path.GetTempPath() + "\\servicetags.json");
            var array = JsonConvert.DeserializeObject<ServiceTagsFile>(sourceFile);
            List<string> allTheSheets = new List<string>();

            //List of service name that ip address are not part of final array
           var ServicesRemove = new[] { "AzureBackup", "AzureBotService", "AzureCognitiveSearch", "AzureCosmosDB", "AzureDatabricks", "AzureDigitalTwins", "AzureIoTHub", "AzureSignalR", "AzurePlatformIMDS", "AzureDataLake", "AzureFrontDoor.Frontend", "AzureFrontDoor.Backend", "AzureFrontDoor.FirstParty", "CognitiveServicesManagement", "HDInsight" };
             
            foreach (var value in array.values)
            {
                if (value.properties.regionId == regionID && ServicesRemove.Contains(value.properties.systemService) == false) //Get ip from specific region and service
                {
                    
                    foreach (var propertie in value.properties.addressPrefixes)
                       
                    {
                        if (propertie.Count(c => c == '.') == 3) // if ipv4
                        {
                           allTheSheets.Add(propertie.ToString());
                        }
                    }
                }
            }
            //remove duplicate value
            string[] q = allTheSheets.Distinct().ToArray();
            log.LogInformation(q.Length.ToString());

            //Create Object wrapper with region
            dynamic ipAddress = regionID == 12 ?
            (object)new
                {
                    canadaeast = q
                }
                :
                (object)new
                {
                    canadacentral = q
                };

             return new OkObjectResult(ipAddress);
        }
    }

    public class Properties
    {
        public int changeNumber { get; set; }
        public string region { get; set; }
        public int regionId { get; set; }
        public string platform { get; set; }
        public string systemService { get; set; }
        public IList<string> addressPrefixes { get; set; }
        public IList<string> networkFeatures { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public string id { get; set; }
        public Properties properties { get; set; }
    }

    public class ServiceTagsFile
    {
        public int changeNumber { get; set; }
        public string cloud { get; set; }
        public IList<Value> values { get; set; }
    }


}


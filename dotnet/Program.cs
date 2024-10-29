using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace AppInsightsTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Get a connection String for App Insights, not an instrumentation key
            // https://learn.microsoft.com/azure/azure-monitor/app/migrate-from-instrumentation-keys-to-connection-strings
	    // Connection string starts with "InstrumentationKey=<GUID>;IngestionEndpoint=<AppInsightsURI> ..."

            var telemetryConfiguration = new TelemetryConfiguration
            {
                ConnectionString = "<PUT YOUR CONNECTION STRING HERE>"
            };
            var telemetryClient = new TelemetryClient(telemetryConfiguration);

	    // create a search client, with the search service endpoint and the name of your search index
            var client = new SearchClient(new Uri("<PUT YOUR SEARCH SERVICE ENDPOINT HERE, example: https://contoso-search.search.windows.net"), "<PUT YOUR INDEX NAME HERE>", new DefaultAzureCredential());
            // Generate a new correlation id for logs
            string searchId = Guid.NewGuid().ToString();
            string searchText = "*";
            SearchResults<SearchDocument> searchResults;
            // Set correlation id for search request
            using (HttpPipeline.CreateClientRequestIdScope(clientRequestId: searchId))
            {
                searchResults = client.Search<SearchDocument>(searchText, options: new SearchOptions { IncludeTotalCount = true } );
            }

            // Create properties for telemetry
            var properties = new Dictionary<string, string>
            {
                ["searchId"] = searchId,
                ["serviceName"] = "<PUT YOUR SEARCH SERVICE NAME HERE, example: contoso-search>",
                ["indexName"] = "<PUT YOUR INDEX NAME HERE>",
                ["searchText"] = searchText,
                ["resultsCount"] = searchResults.TotalCount?.ToString()
            };
            // Find this event in the "customEvents" table in AppInsights
            // https://learn.microsoft.com/azure/azure-monitor/app/api-custom-events-metrics#trackevent
            telemetryClient.TrackEvent("search", properties);
            telemetryClient.Flush();
        }
    }
}

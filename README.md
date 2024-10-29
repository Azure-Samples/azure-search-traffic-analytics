# azure-search-traffic-analytics

Code snippets for adding client-side telemetry from Application Insights in an Azure AI Search application, as described in [Collect telemetry data for search traffic analytics](/azure/search/search-traffic-analytics) in the Azure AI Search documentation.

You can also get service-side telemetry through diagnostic logs added to a metrics database or log store. User queries are captured service-side in these logs, so if your objective is to monitor query operations, see [Monitor Azure AI Search](/azure/search/monitor-azure-cognitive-search) instead. You can also [use log queries](/azure/azure-monitor/logs/queries) for insights into index and query processing.

This repository provides code snippets in C# and JavaScript.

## Prerequisites

- [Azure AI Search](/azure/search/search-create-service-portal), any region and any tier

- [Application Insights](/azure/azure-monitor/app/create-workspace-resource), a feature of Azure Monitor

- A rich client application providing an interactive search experience that includes click events or other user actions that you want to correlate to search result selections

## Add instrumentation to your client code

The code snippets provided in this repo should be added to client front-end code. Instrumentation has the following parts:

- Telemetry client
- Search client, modified to include a correlation Id
- Custom event in Application Insights (for a search query)

### Specify the telemetry client

The telemetry client is Application Insights, a feature of Azure Monitor.

To set up the client, [provide a connection string](https://learn.microsoft.com/azure/azure-monitor/app/migrate-from-instrumentation-keys-to-connection-strings) for Application Insights, not an instrumentation key.

A connection string might look similar to the following example:

```
ConnectionString = "InstrumentationKey=aaaaaaaa-0b0b-1c1c-2d2d-333333333333;IngestionEndpoint=https://contoso.in.applicationinsights.azure.com/;LiveEndpoint=https://contoso.livediagnostics.monitor.azure.com/;ApplicationId=00001111-aaaa-2222-bbbb-3333cccc4444"
```

### Add instrumentation and correlation IDs to a search request

Your application code has query requests. In your query code, add a search ID so that you can correlate a query instance with its search results.

Here's what that code might look like in C#:

```dotnet
var client = new SearchClient(new Uri("https://contoso.search.windows.net"), "hotels-sample-index", new DefaultAzureCredential());

// Generate a new correlation id for logs
string searchId = Guid.NewGuid().ToString();
string searchText = "*";
SearchResults<SearchDocument> searchResults;

// Set correlation id for search request
using (HttpPipeline.CreateClientRequestIdScope(clientRequestId: searchId))
{
    searchResults = client.Search<SearchDocument>(searchText, options: new SearchOptions { IncludeTotalCount = true } );
}
```

### Create properties for telemetry

Create a properties bag that captures the request in terms of the search ID, search service, index name, query, and the results. These properties are used to create custom events for a query request.

Here's an example of what the properties might look like in C#:

```dotnet
// Create properties for telemetry
var properties = new Dictionary<string, string>
{
    ["searchId"] = searchId,
    ["serviceName"] = "contoso-search",
    ["indexName"] = "hotels-sample-index",
    ["searchText"] = searchText,
    ["resultsCount"] = searchResults.TotalCount?.ToString()
};
```

### Send the custom event to Application Insights

Application Insights provides core telemetry API to send custom events and metrics and your own versions of standard telemetry.

Here's an example of sending custom events to Application Insights:

```dotnet
// Find this event in the "customEvents" table in AppInsights
// https://learn.microsoft.com/azure/azure-monitor/app/api-custom-events-metrics#trackevent
telemetryClient.TrackEvent("search", properties);
telemetryClient.Flush();
```

For more information and next steps, we recommend [Application Insights overview](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview#frequently-asked-questions).
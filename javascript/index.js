import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import { SearchClient } from '@azure/search-documents'
import { DefaultAzureCredential } from '@azure/identity'
import { v4 as uuidv4 } from 'uuid';

const searchClient = new SearchClient(
  "<PUT YOUR SEARCH SERVICE ENDPOINT HERE, example: https://contoso-search.search.windows.net>",
  "<PUT YOUR INDEX NAME HERE>",
  new DefaultAzureCredential()
);

// Get a connection String for App Insights, not an instrumentation key
// https://learn.microsoft.com/azure/azure-monitor/app/migrate-from-instrumentation-keys-to-connection-strings
// Connection string starts with "InstrumentationKey=<GUID>;IngestionEndpoint=<AppInsightsURI> ..."

const appInsights = new ApplicationInsights({ config: {
  connectionString: '<PUT YOUR CONNECTION STRING HERE>'
  /* ...Other Configuration Options... */
} });
appInsights.loadAppInsights();

const searchId = uuidv4();
const searchText = "*";
const searchResults = await searchClient.search(searchText, { includeTotalCount: true, customHeaders: { "x-ms-client-request-id": searchId }});
const properties = {
    searchId: searchId,
    serviceName: "<PUT YOUR SEARCH SERVICE NAME HERE, example contoso-search>",
    indexName: "<PUT YOUR INDEX NAME HERE>",
    searchText: searchText,
    resultsCount: searchResults.count
};
appInsights.trackEvent({ name: "search" }, properties);
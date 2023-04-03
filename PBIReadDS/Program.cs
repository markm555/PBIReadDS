using Microsoft.Identity.Client;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Nancy.Json;

// 03/30/2023 - Mark Moore - Developed to pull data out of a Power BI Dataset as JSON.

namespace PBIReadDS
{

    internal class Program
    {
        public static string GetAccessTokenWithLocalCredential(string client_id, string client_secret, string tenant_id)
        {
            string appId = client_id;               //pull client_id from arguments passed to funtion and store them in variable.
            string client_secrets = client_secret;  //pull client_secret from arguments passed to funtion and store them in variable.
            string tenant_secrets = tenant_id;      //pull tenant_id from from arguments passed to funtion and store them in variable.
            string tenantSpeceficAuthority = "https://login.microsoftonline.com/" + tenant_secrets;
            var appConfidential = ConfidentialClientApplicationBuilder.Create(appId)  //
                .WithClientSecret(client_secret)                                      //
                .WithAuthority(tenantSpeceficAuthority)                               //  Set up API info to call API to get a token
                .Build();                                                             //
            string[] scopes = { "https://analysis.windows.net/powerbi/api/.default" };
            var authResult = appConfidential.AcquireTokenForClient(scopes).ExecuteAsync().Result;  // Call API and store response in results
            return authResult.AccessToken;  // Return the AccessToken from the results to the caller
        }

        static void Main(string[] args)
        {
            string clientID = "<Your Client ID>";               //
            string TenantId = "<Your Tenant ID>";               // Create variables that contain Service Principal info and
            string ClientSecret = "<Your Client Secret>";       // Workspace and dataset info
            string datasetId = "<Your Dataset ID>";             // ** Not best practice ** Idealy Service Principal info would be stored in keyvault and pulled from there
            
            //Replace with your query formated as Json
            string query = "{\"queries\": [{\"query\": \"EVALUATE TOPN( 10, VALUES(Sales))\"}],\"serializerSettings\": {\"includeNulls\": true}}";  // DAX Query

            string token = GetAccessTokenWithLocalCredential(clientID, ClientSecret, TenantId);  // Pull Access Token using above function

            HttpClient client = new HttpClient();  // setup to make a REST API Call
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);  // Set up REST API header info
            string url = $"https://api.powerbi.com/v1.0/myorg/datasets/{datasetId}/executeQueries";  // REST API url
            var response = client.PostAsync(url, new StringContent(query, UnicodeEncoding.UTF8, "application/json")).Result;  // Make API call using HttpClient
            var jsonResponse = response.Content.ReadAsStringAsync().Result; // Store response in Result
            JToken jtoken = JToken.Parse(jsonResponse);  // Setup to deserialize JSON response
            JArray result = (JArray)jtoken.SelectToken("results"); // Pull the JSON array our of results
            Console.WriteLine(result);  // Display the results in a Console window.
        }
    }
}

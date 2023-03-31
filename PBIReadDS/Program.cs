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
            string appId = client_id;
            string client_secrets = client_secret;
            string tenant_secrets = tenant_id;
            string tenantSpeceficAuthority = "https://login.microsoftonline.com/" + tenant_secrets;
            var appConfidential = ConfidentialClientApplicationBuilder.Create(appId)
                .WithClientSecret(client_secret)
                .WithAuthority(tenantSpeceficAuthority)
                .Build();
            string[] scopes = { "https://analysis.windows.net/powerbi/api/.default" };
            var authResult = appConfidential.AcquireTokenForClient(scopes).ExecuteAsync().Result;
            return authResult.AccessToken;
        }

        static void Main(string[] args)
        {
            string clientID = "e7c4192b-4eb4-46af-9a0c-ffd54b7bc0f2";
            string TenantId = "7f017d74-7112-456d-bbc8-6558888a90a1";
            string ClientSecret = ".I78Q~fO9JL~ASOxpnA.ReWXKLC2ltnb-gicjbss";
            string datasetId = "e3ba3167-148d-4410-b57e-8db945d1f2bf";
            string query = "{\"queries\": [{\"query\": \"EVALUATE TOPN( 10, VALUES(Sales))\"}],\"serializerSettings\": {\"includeNulls\": true}}";

            string token = GetAccessTokenWithLocalCredential(clientID, ClientSecret, TenantId);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string url = $"https://api.powerbi.com/v1.0/myorg/datasets/{datasetId}/executeQueries";
            var response = client.PostAsync(url, new StringContent(query, UnicodeEncoding.UTF8, "application/json")).Result;
            var jsonResponse = response.Content.ReadAsStringAsync().Result;
            JToken jtoken = JToken.Parse(jsonResponse);
            JArray result = (JArray)jtoken.SelectToken("results");
            Console.WriteLine(result);
        }
    }
}

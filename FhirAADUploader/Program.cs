using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FhirAADUploader
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }
        private static AuthenticationContext authContext = null;

        static void Main(string[] args)
        {
            Task.Run(() => MainAsync(args)).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            Configuration = builder.Build();


            //Parse some command line arguments
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            string fhirResourcePath = Path.GetFullPath(args[0]);
            string fhirServerUrl = args[1];

            Console.WriteLine($"FHIR Resource Path  : {fhirResourcePath}");
            Console.WriteLine($"FHIR Server URL     : {fhirServerUrl}");
            Console.WriteLine($"Azure AD Authority  : {Configuration["AzureAD_Authority"]}");
            Console.WriteLine($"Azure AD Client ID  : {Configuration["AzureAD_ClientId"]}");
            Console.WriteLine($"Azure AD Audience   : {Configuration["AzureAD_Audience"]}");

            DirectoryInfo dir = new DirectoryInfo(fhirResourcePath);
            FileInfo[] files = null;
            try
            {
                files = dir.GetFiles("*.json");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            authContext = new AuthenticationContext(Configuration["AzureAD_Authority"]);

            ClientCredential clientCredential = new ClientCredential(Configuration["AzureAD_ClientId"], Configuration["AzureAD_ClientSecret"]); ;

            AuthenticationResult authResult = null;
            try
            {
                authResult = authContext.AcquireTokenAsync(Configuration["AzureAD_Audience"], clientCredential).Result;
            }
            catch (Exception ee)
            {
                Console.WriteLine(
                    String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\n",
                    DateTime.Now.ToString(),
                    ee.ToString()));
                return;
            }

            foreach (FileInfo f in files)
            {
                Console.WriteLine("Processing file: " + f.FullName);
                using (StreamReader reader = File.OpenText(f.FullName))
                {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                    JArray entries = (JArray)o["entry"];

                    Console.WriteLine("Number of entries: " + entries.Count);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        string entry_json = (((JObject)entries[i])["resource"]).ToString();
                        string resource_type = (string)(((JObject)entries[i])["resource"]["resourceType"]);

                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(fhirServerUrl);
                            
                            //If we already have a token, we should get the cached one, otherwise, refresh
                            authResult = authContext.AcquireTokenAsync(Configuration["AzureAD_Audience"], clientCredential).Result;

                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                            StringContent content = new StringContent(entry_json, Encoding.UTF8, "application/json");
                            var postresult = await client.PostAsync($"/{resource_type}", content);
                            if (!postresult.IsSuccessStatusCode)
                            {
                                string resultContent = await postresult.Content.ReadAsStringAsync();
                                Console.WriteLine(resultContent);
                            }
                        }
                    }
                }
            }


        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("   dotnet run <PATH TO FHIR RESOURCES> <FHIR SERVER URL>");
        }
    }
}

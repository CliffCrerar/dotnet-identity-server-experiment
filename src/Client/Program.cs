using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteHeading();
            WriteLine();
            var discoveryDocument = IdentityClient.Discovery().Result;
            // discovery doc
            PrintDiscoveryDocument(discoveryDocument);
            WriteLine();
            // token request
            var tokenResponse = IdentityClient.RequestToken(discoveryDocument).Result;
            WriteLine();
            PrintTokenResponse(tokenResponse);
            WriteLine();
            // API CALL
            var apiCallResponse = IdentityClient.CallApi(tokenResponse).Result;
            PrintApiCallResponse(apiCallResponse);
            
            // https://jwt.ms/
            
            static void WriteHeading()
            {
                Console.WriteLine("--------------------------------------------------------------------------");
                Console.WriteLine("----------------------IDENTITY SERVER CONSOLE DEMO------------------------");
                Console.WriteLine("--------------------------------------------------------------------------");
            }

            static void WriteLine()
            {
                Console.WriteLine("--------------------------------------------------------------------------");
            }

            static void PrintDiscoveryDocument(DiscoveryDocumentResponse discoDoc)
            {
                Console.WriteLine("--------------------------DISCOVERY DOCUMENT------------------------------");
                Console.WriteLine(JsonConvert.SerializeObject(discoDoc));
                Console.WriteLine("--------------------------------------------------------------------------");
            }

            static void PrintTokenResponse(TokenResponse tokenResponse)
            {
                Console.WriteLine("-----------------------------TOKEN RESPONSE-------------------------------");
                Console.WriteLine(tokenResponse.Json);
                Console.WriteLine("--------------------------------------------------------------------------");
            }
            
            static void PrintApiCallResponse(dynamic response)
            {
                Console.WriteLine("---------------------------API CALL RESPONSE------------------------------");
                Console.WriteLine(response);
                Console.WriteLine("--------------------------------------------------------------------------");
            }
        }
    }
    

    public abstract class IdentityClient
    {
        private static HttpClient HttpClient => new HttpClient();

        // API CALL
        public static async Task<dynamic> CallApi(TokenResponse tokenResponse)
        {
            var apiClient = HttpClient;
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            
            var response = await apiClient.GetAsync("https://localhost:6001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                return response;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                return JArray.Parse(content);
            }
        }
        
        // DISCOVERY DOCUMENT
        public static async Task<DiscoveryDocumentResponse> Discovery()
        {
            var disco = await HttpClient.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                throw new Exception(disco.Error);
            }

            return disco;
        }

        // ACCESS TOKEN REQUEST
        public static async Task<TokenResponse> RequestToken(DiscoveryDocumentResponse disco)
        {
            var tokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                throw new Exception(disco.Error);
            }

            var idToken = tokenResponse.IdentityToken ?? "No Id Token";
            var refreshToken = tokenResponse.RefreshToken ?? "No Refresh Token";
            var accessToken = tokenResponse.AccessToken ?? "No Acceess Token";

            Console.WriteLine("-----------------------ID TOKEN-----------------------");
            Console.WriteLine(idToken);
            Console.WriteLine("-----------------------REFRESH TOKEN-----------------------");
            Console.WriteLine(refreshToken);
            Console.WriteLine("-----------------------ACCESS TOKEN-----------------------");
            Console.WriteLine(accessToken);

            return tokenResponse;
        }
    }
}
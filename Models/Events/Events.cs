using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GraphAPILibraries
{
    public class Events
    {
        public static async Task<string> GetResultAsync(string input_json, string accessToken) 
        {
            string endpoint = "https://graph.microsoft.com/v1.0/me/events";
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Content = new StringContent(input_json, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                            return json.ToString();
                        }
                        string failture = "failture";
                        return failture;
                    }
                }
            }
        } 
    }
}
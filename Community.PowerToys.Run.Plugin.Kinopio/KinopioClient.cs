using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wox.Plugin.Logger;

namespace Kinopio
{
    public class KinopioClient
    {
        private const string HostName = "https://api.kinopio.club";

        private readonly HttpClient _client;

        public string username;

        public KinopioClient(string kinopioAuthToken)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Cache-Control", "must-revalidate, no-store, no-cache, private");
            SetAuthToken(kinopioAuthToken);
        }

        public async void SetAuthToken(string kinopioAuthToken)
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", kinopioAuthToken);
            
            if (!string.IsNullOrEmpty(kinopioAuthToken))
            {
                await GetUsername();
            }
            else 
            {
                username = string.Empty;
            }
        }

        public async Task GetUsername()
        {
            try
            {
                var response = await _client.GetAsync($"{HostName}/user");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    username = user.GetProperty("name").GetString();
                }
                else
                {
                    Log.Error($"🐸 GetUsername error: {await response.Content.ReadAsStringAsync()}", GetType());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"🐸 GetUsername exception: {ex.Message}", GetType());
            }
        }

        public async Task<bool> SaveToInbox(string content)
        {
            var contentObject = new { name = content, status = 200 };
            var serializedContent = new StringContent(JsonSerializer.Serialize(contentObject), Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync($"{HostName}/card/to-inbox", serializedContent);
                if (response.IsSuccessStatusCode)
                {
                    // Log.Info($"🐸 SaveToInbox success: {await response.Content.ReadAsStringAsync()}", GetType());
                    return true;
                }
                else
                {
                    // TODO find way to inform user of error
                    Log.Error($"🐸 SaveToInbox error: {await response.Content.ReadAsStringAsync()}", GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"🐸 SaveToInbox exception: {ex.Message}", GetType());
                return false;
            }
        }
    }
}

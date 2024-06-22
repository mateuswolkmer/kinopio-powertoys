using System.Net;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;
using System.Globalization;
using System.Text.RegularExpressions;

using Wox.Plugin;
using Wox.Plugin.Logger;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Kinopio
{
    public class Main : IPlugin, ISettingProvider, IDisposable, IDelayedExecutionPlugin
    {
        private string? IconPath { get; set; }

        private PluginInitContext? Context { get; set; }

        public string Name => "Kinopio";

        public string Description => "Add Kinopio card to your inbox.";

        public static string PluginID => "5D7039091AB947BBAF1C2E61CC02B5B5";

        private bool Disposed { get; set; }

        private string _kinopioAuthToken;

		public IEnumerable<PluginAdditionalOption> AdditionalOptions =>
		[
			new()
			{
				Key = "KinopioAuthToken",
				DisplayLabel = "Kinopio auth token",
                DisplayDescription = "To get it click in your User icon, then Settings, then API key",
				TextValue = _kinopioAuthToken,
				PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
			},
		];
        

		public void UpdateSettings(PowerLauncherPluginSettings settings)
		{
            var kinopioAuthToken = string.Empty;

            if (settings != null && settings.AdditionalOptions != null)
            {
                var authToken = settings.AdditionalOptions.FirstOrDefault(x => x.Key == "KinopioAuthToken");
                kinopioAuthToken = authToken?.TextValue ?? kinopioAuthToken;
            }

            _kinopioAuthToken = kinopioAuthToken;
		}

        public List<Result> Query(Query query)
        {
            return Query(query, false);
        }

        public List<Result> Query(Query query, bool delayedExecution)
        {
            var content = query.Search;

            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<Result>();
            }

            return new List<Result>
            {
                new() {
                    Title = content,
                    SubTitle = "Save to inbox",
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        Task.Run(async () => await ExecuteAsync(content));
                        return true;
                    }
                },
            };
        }

        private async Task<bool> ExecuteAsync(string content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Cache-Control", "must-revalidate, no-store, no-cache, private");
                if (!string.IsNullOrEmpty(_kinopioAuthToken))
                {
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", _kinopioAuthToken);
                }
                var contentObject = new { name = content, status = 200 };
                var serializedContent = new StringContent(JsonSerializer.Serialize(contentObject), Encoding.UTF8, "application/json");
                Log.Info($"💩 AUTH TOKEN: {_kinopioAuthToken}", GetType());
                Log.Info($"💩 CONTENT: {contentObject}", GetType());
                
                try
                {
                    var response = await client.PostAsync("https://api.kinopio.club/card/to-inbox", serializedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Info($"💩 RESPONSE: {await response.Content.ReadAsStringAsync()}", GetType());
                    }
                    else
                    {
                        Log.Error($"💩 ERROR RESPONSE: {await response.Content.ReadAsStringAsync()}", GetType());
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"💩 EXCEPTION: {ex.Message}", GetType());
                }
                
                return true;
            }
        }
        
        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        private void UpdateIconPath(Theme theme)
        {
            IconPath = "images/icon.png";
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }
        
        System.Windows.Controls.Control ISettingProvider.CreateSettingPanel() => throw new NotImplementedException();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }
    }
}

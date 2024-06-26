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
using Kinopio;

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
        private KinopioClient kinopioClient;

        public IEnumerable<PluginAdditionalOption> AdditionalOptions =>
        new[]
        {
            new PluginAdditionalOption
            {
                Key = "KinopioAuthToken",
                DisplayLabel = "Kinopio auth token",
                DisplayDescription = kinopioClient?.username != null ? $"Connected to {kinopioClient.username}" : "Get it from your account settings on kinopio.club",
                TextValue = _kinopioAuthToken,
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            },
        };

		public void UpdateSettings(PowerLauncherPluginSettings settings)
		{
            var kinopioAuthToken = string.Empty;

            if (settings != null && settings.AdditionalOptions != null)
            {
                var authToken = settings.AdditionalOptions.FirstOrDefault(x => x.Key == "KinopioAuthToken");
                kinopioAuthToken = authToken?.TextValue ?? kinopioAuthToken;
            }

            _kinopioAuthToken = kinopioAuthToken;
            kinopioClient?.SetAuthToken(_kinopioAuthToken);
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

            if (string.IsNullOrEmpty(_kinopioAuthToken))
            {
                return new List<Result>
                {
                    new() {
                        Title = "API key not set",
                        SubTitle = "Please set it before we can start talking to the API",
                        IcoPath = IconPath
                    },
                };
            }

            return new List<Result>
            {
                new() {
                    Title = content,
                    SubTitle = $"Save to inbox{(kinopioClient?.username != null ? $" • {kinopioClient.username}" : string.Empty)}",
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        Task.Run(async () => await kinopioClient.SaveToInbox(content));
                        return true;
                    }
                },
            };
        }
        
        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
            kinopioClient = new KinopioClient(_kinopioAuthToken);
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
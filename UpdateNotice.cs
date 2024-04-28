using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ConsoleSystem;

namespace Oxide.Plugins
{
    [Info("Update Notice", "Psystec", "3.2.2", ResourceId = 2837)]
    [Description("Notifies you when new Rust updates are released.")]
    internal sealed class UpdateNotice : RustPlugin
    {
        //DO NOT EDIT. Please ask for permission or notify me should you require changes.
        //Check out our API host sponser YaMang's plugins at codefling.com: https://codefling.com/yamang-w
        #region Fields

        private Configuration _configuration;
        private UpdateInfo _updateInfo;

        private const string AdminPermission = "updatenotice.admin";
        private const string ApiUrl = "http://api.yamang.xyz:2095/rustapi";

        private bool _initLoad = true;
        private int _checkingInterval = 180;

        private readonly Dictionary<string, string> _properNames = new Dictionary<string, string>
        {
            {"carbon", "Carbon"},
            {"uMod", "Oxide"},
            {"rustClient", "Client"},
            {"rustClientStaging", "ClientStaging"},
            {"rustServer", "Server"},
            {"newsgid", "DevBlog"},
        };

        private readonly Dictionary<string, string> _properCommands = new Dictionary<string, string>
        {
            {"carbon", "carbon"},
            {"uMod", "umod"},
            {"rustClient", "client"},
            {"rustClientStaging", "clientstaging"},
            {"rustServer", "server"},
            {"newsgid", "devblog"},
        };

        #endregion Fields

        #region Classes

        private sealed class UpdateInfo
        {
            public string carbon { get; set; }
            public string uMod { get; set; }
            public string rustClient { get; set; }
            public string rustClientStaging { get; set; }
            public string rustServer { get; set; }
            public string newsgid { get; set; }
            public string newsurl { get; set; }
            public string newsdate { get; set; }
        }

        #region Discord Message
        private sealed class DiscordMessageEmbeds
        {
            /// <summary>
            /// if used, it overrides the default username of the webhook
            /// </summary>
            public string username { get; set; }
            /// <summary>
            /// if used, it overrides the default avatar of the webhook
            /// </summary>
            public string avatar_url { get; set; }
            /// <summary>
            /// simple message, the message contains (up to 2000 characters)
            /// </summary>
            public string content { get; set; }
            /// <summary>
            /// array of embed objects. That means, you can use more than one in the same body
            /// </summary>
            public Embed[] embeds { get; set; }
        }
        private sealed class Embed
        {
            /// <summary>
            /// embed author object
            /// </summary>
            public Author author { get; set; }
            /// <summary>
            /// title of embed
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// url of embed. If title was used, it becomes hyperlink
            /// </summary>
            public string url { get; set; }
            /// <summary>
            /// description text
            /// </summary>
            public string description { get; set; }
            /// <summary>
            /// color code of the embed. You have to use Decimal numeral system, not Hexadecimal. Use color picker and converter: https://htmlcolorcodes.com/color-picker/ and https://www.binaryhexconverter.com/hex-to-decimal-converter
            /// </summary>
            public int color { get; set; }
            /// <summary>
            /// rray of embed field objects
            /// </summary>
            public Field[] fields { get; set; }
            /// <summary>
            /// embed thumbnail object
            /// </summary>
            public Thumbnail thumbnail { get; set; }
            /// <summary>
            /// embed image object
            /// </summary>
            public Image image { get; set; }
            /// <summary>
            /// embed footer object
            /// </summary>
            public Footer footer { get; set; }
        }
        private sealed class Author
        {
            /// <summary>
            /// name of author
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// url of author. If name was used, it becomes a hyperlink
            /// </summary>
            public string url { get; set; }
            /// <summary>
            /// url of author icon
            /// </summary>
            public string icon_url { get; set; }
        }
        private sealed class Thumbnail
        {
            /// <summary>
            /// url of thumbnail
            /// </summary>
            public string url { get; set; }
        }
        private sealed class Image
        {
            /// <summary>
            /// url of image
            /// </summary>
            public string url { get; set; }
        }
        private sealed class Footer
        {
            /// <summary>
            /// footer text, doesn't support Markdown
            /// </summary>
            public string text { get; set; }
            /// <summary>
            /// url of footer icon
            /// </summary>
            public string icon_url { get; set; }
        }
        private sealed class Field
        {
            /// <summary>
            /// name of the field
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// alue of the field
            /// </summary>
            public string value { get; set; }
            /// <summary>
            /// if true, fields will be displayed in same line, but there can only be 3 max in same line or 2 max if you used thumbnail
            /// </summary>
            public bool inline { get; set; }
        }
        #endregion Discord Message

        #endregion Classes

        #region Configuration

        private sealed class Configuration
        {
            [JsonProperty("Only Notify Admin")]
            public bool OnlyNotifyAdmins { get; set; } = false;

            [JsonProperty("Enable Discord Notifications")]
            public bool EnableDiscordNotify { get; set; } = false;

            [JsonProperty("Discord role id to mention (0 = no mention)")]
            public ulong DiscordRoleId { get; set; } = 0;

            [JsonProperty("Discord Webhook URL")]
            public string DiscordWebhookURL { get; set; } = "https://support.discordapp.com/hc/en-us/articles/228383668";

            [JsonProperty("Enable GUI Notifications (Needs GUIAnnouncements)")]
            public bool EnableGuiNotifications { get; set; } = true;

            [JsonProperty("Enable Chat Notifications")]
            public bool EnableChatNotifications { get; set; } = true;

            [JsonProperty("GUI Notifications Tint Color")]
            public string GUINotificationsTintColor { get; set; } = "Purple";

            [JsonProperty("GUI Notifications Text Color")]
            public string GUINotificationsTextColor { get; set; } = "Yellow";

            [JsonProperty("Enable Server Version Notifications")]
            public bool EnableServer { get; set; } = true;

            [JsonProperty("Enable DevBlog Notifications")]
            public bool EnableDevBlog { get; set; } = true;

            [JsonProperty("Enable Client Version Notifications")]
            public bool EnableClient { get; set; } = true;

            [JsonProperty("Enable Staging Version Notifications")]
            public bool EnableStaging { get; set; } = false;

            [JsonProperty("Enable UMod Version Notifications")]
            public bool EnableUMod { get; set; } = false;

            [JsonProperty("Enable Carbon Version Notifications")]
            public bool EnableCarbon { get; set; } = false;
        }

        protected override void SaveConfig() => Config.WriteObject(_configuration);
        private void LoadNewConfig() => _configuration = Config.ReadObject<Configuration>();
        protected override void LoadDefaultConfig() => _configuration = new Configuration();
        protected override void LoadConfig()
        {
            base.LoadConfig();
            _configuration = Config.ReadObject<Configuration>();
        }

        #endregion Configuration

        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ServerUpdated"] = "Server Update Released!",
                ["DevBlogUpdated"] = "DevBlog Update Released!",
                ["ClientUpdated"] = "Client Update Released!",
                ["ClientStagingUpdated"] = "Client Staging Update Released!",
                ["UModUpdated"] = "UMod Update Released!",
                ["CarbonUpdated"] = "Carbon Update Released!",
                ["FailedToCheckUpdates"] = "Failed to check for RUST updates, if this keeps happening please contact the developer.",
                ["PluginNotFoundGuiAnnouncements"] = "GUIAnnouncements plugin was not found. GUI Announcements disabled.",
                ["NoPermission"] = "You do not have permission to use this command.",
                ["DiscordWebhookURLNotConfigured"] = "Discord Webhook URL is not configured.",
                ["IntervalCheck"] = "Checking interval must be 180 seconds or greater! Setting this lower may get your server banned. Auto adjusted to 300.",
                ["Help.Command"] = "COMMAND",
                ["Help.Description"] = "DESCRIPTION",
                ["Help.Gui"] = "Tests GUI notification (Needs GUIAnnouncements Plugin)",
                ["Help.Discord"] = "Tests Discord notification",
                ["Help.Current"] = "Display's all current versions",
                ["Help.Server"] = "Simulate Server update release",
                ["Help.DevBlog"] = "Simulate DevBlog update release",
                ["Help.Client"] = "Simulate Client update release",
                ["Help.Staging"] = "Simulate Staging update release",
                ["Help.Oxide"] = "Simulate Oxide update release",
                ["Help.Carbon"] = "Simulate Carbon update release",
                ["Help.All"] = "Simulate all updates released (depends on config)",
                ["Help.ForceCheck"] = "Forces a version check",
                ["Help.LoadConfig"] = "Reload the config file",
                ["Chat.Prefix"] = "<size=20><color=#ff0000>Update Notice</color></size>",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ServerUpdated"] = "Mise à jour du Serveur disponible !",
                ["DevBlogUpdated"] = "Mise à jour du \"DevBlog\" disponible !",
                ["ClientUpdated"] = "Mise à jour du Client disponible !",
                ["ClientStagingUpdated"] = "Mise à jour de la branche \"Staging\" du Client disponible !",
                ["UModUpdated"] = "Mise à jour de UMod disponible !",
                ["CarbonUpdated"] = "Mise à jour de Carbon disponible !",
                ["FailedToCheckUpdates"] = "Récupération des mises à jour de RUST impossible, si cela se reproduit veuillez contacter le développeur.",
                ["PluginNotFoundGuiAnnouncements"] = "Le plugin GUIAnnouncements n'a pas été trouvé. GUI Announcements désactivé.",
                ["NoPermission"] = "Vous n'avez pas la permission d'utiliser cette commande",
                ["DiscordWebhookURLNotConfigured"] = "L'URL du Discord Webhook n'est pas configuré.",
                ["IntervalCheck"] = "L'interval de vérification doit être de 180 secondes ou plus ! Configurer une valeur inférieure pourrait voir votre serveur banni. Ajustement automatique à 300.",
                ["Help.Command"] = "COMMANDE",
                ["Help.Description"] = "DESCRIPTION",
                ["Help.Gui"] = "Simule la notification par GUI (Requiert le plugin GUIAnnouncements)",
                ["Help.Discord"] = "Simule la notification Discord",
                ["Help.Current"] = "Affiche toutes les versions actuelles",
                ["Help.Server"] = "Simule la notification de la mise à jour du Server",
                ["Help.DevBlog"] = "Simule la notification de la mise à jour \"DevBlog\"",
                ["Help.Client"] = "Simule la notification de la mise à jour du Client",
                ["Help.Staging"] = "Simule la notification de la mise à jour de la branche \"Staging\"",
                ["Help.Oxide"] = "Simule la notification de la mise à jour d'Oxide",
                ["Help.Carbon"] = "Simule la notification de la mise à jour de Carbon",
                ["Help.All"] = "Simule toutes les notifications de mise à jour (dépend de la config)",
                ["Help.ForceCheck"] = "Force l'actualisation des données",
                ["Help.LoadConfig"] = "Recharge la configuration",
                ["Chat.Prefix"] = "<size=20><color=#ff0000>Avis de mise à jour</color></size>",
            }, this, "fr");
        }

        #endregion Localization

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(AdminPermission, this);
        }

        private void Loaded()
        {
            if (GUIAnnouncements == null)
            {
                PrintWarning(Lang("PluginNotFoundGuiAnnouncements"));
                _configuration.EnableGuiNotifications = false;
            }

            if (_checkingInterval < 180)
            {
                PrintWarning(Lang("IntervalCheck"));
                _checkingInterval = 300;
            }

            timer.Every(_checkingInterval, CompareVersions);
        }

        #endregion Hooks

        #region Testing

        [ConsoleCommand("updatenotice")]
        private void AdvertCommands(Arg arg)
        {
            if (arg.IsClientside && !HasPermission(arg.Player(), AdminPermission))
            {
                SendReply(arg, Lang("NoPermission"));
                return;
            }
            if (arg.Args.IsNullOrEmpty())
            {
                SendHelp(arg);
                return;
            }

            if (_updateInfo == null)
            {
                CompareVersions();
            }

            var command = arg.Args[0].ToLower();

            switch (command)
            {
                case "gui":
                    TestNotification("GUI", GUIAnnouncements, "Test message from Update Notice by Psystec", SendtoGui);
                    break;

                case "discord":
                    TestNotification("Discord", !string.IsNullOrEmpty(_configuration.DiscordWebhookURL), "Test message from Update Notice by Psystec", SendToDiscord);
                    break;

                case "current":
                    DisplayCurrentVersions(arg);
                    break;

                case "server":
                case "devblog":
                case "client":
                case "clientstaging":
                case "umod":
                case "carbon":
                    TestUpdate(command, true);
                    break;

                case "all":
                    TestAllUpdates();
                    break;

                case "forcecheck":
                    Puts("Forcing Version Check");
                    CompareVersions();
                    break;

                case "loadconfig":
                    Puts("Loading Configuration File");
                    LoadNewConfig();
                    break;

                default:
                    SendHelp(arg);
                    break;
            }
        }

        private void TestNotification(string type, bool condition, string message, Action<string> notificationMethod)
        {
            if (!condition)
            {
                PrintWarning(Lang($"PluginNotFound{type}Announcements"));
                return;
            }

            Puts($"Testing {type} Messages: {message}");
            notificationMethod.Invoke(message);
        }

        private void TestUpdate(string type, bool isCmd)
        {
            Puts($"Testing {type} Update");

            Interface.CallHook($"On{type}Update", "TestUpdate");

            // Yeah that's reflexion but fuck infernal switch case
            if (isCmd)
            {
                typeof(UpdateInfo).GetProperty(_properCommands.FirstOrDefault(x => x.Value == type).Key)?.SetValue(_updateInfo, $"Simulating {type} Update");
                return;
            }
            typeof(UpdateInfo).GetProperty(_properNames.FirstOrDefault(x => x.Value == type).Key)?.SetValue(_updateInfo, $"Simulating {type} Update");
        }

        private void TestAllUpdates()
        {
            Puts("Testing All Updates");

            foreach (var keyValuePair in _properNames)
            {
                TestUpdate(keyValuePair.Value, false);
            }
        }

        private void DisplayCurrentVersions(Arg arg)
        {
            SendReply(arg, $"Update Notice by Psystec\n" +
                           $"Server: {_updateInfo.rustServer}\n" +
                           $"DevBlog: {_updateInfo.newsgid}\n" +
                           $"Client: {_updateInfo.rustClient}\n" +
                           $"Staging: {_updateInfo.rustClientStaging}\n" +
                           $"UMod: {_updateInfo.uMod}\n" +
                           $"Carbon: {_updateInfo.carbon}");
        }

        private void SendHelp(Arg arg)
        {
            var width = 30;
            SendReply(arg, (Lang("Help.Command")).PadRight(width) + Lang("Help.Description"));
            SendReply(arg, ("updatenotice gui").PadRight(width) + Lang("Help.Gui"));
            SendReply(arg, ("updatenotice discord").PadRight(width) + Lang("Help.Discord"));
            SendReply(arg, ("updatenotice current").PadRight(width) + Lang("Help.Current"));
            SendReply(arg, ("updatenotice server").PadRight(width) + Lang("Help.Server"));
            SendReply(arg, ("updatenotice devblog").PadRight(width) + Lang("Help.DevBlog"));
            SendReply(arg, ("updatenotice client").PadRight(width) + Lang("Help.Client"));
            SendReply(arg, ("updatenotice staging").PadRight(width) + Lang("Help.Staging"));
            SendReply(arg, ("updatenotice oxide").PadRight(width) + Lang("Help.Oxide"));
            SendReply(arg, ("updatenotice carbon").PadRight(width) + Lang("Help.Carbon"));
            SendReply(arg, ("updatenotice all").PadRight(width) + Lang("Help.All"));
            SendReply(arg, ("updatenotice forcecheck").PadRight(width) + Lang("Help.ForceCheck"));
            SendReply(arg, ("updatenotice loadconfig").PadRight(width) + Lang("Help.LoadConfig"));
        }

        #endregion Testing

        #region Version Comparison

        private void CompareVersions()
        {
            webrequest.Enqueue(ApiUrl, null, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    PrintWarning($"API: {Lang("FailedToCheckUpdates")}\nError Code: {code} | Message: {response}");
                    return;
                }

                var nVData = JsonConvert.DeserializeObject<UpdateInfo>(response);

                if (_initLoad)
                {
                    _updateInfo = nVData;
                    _initLoad = false;
                }

                UpdateCheck("Carbon", nVData.carbon, _updateInfo.carbon, _configuration.EnableCarbon);
                UpdateCheck("Client", nVData.rustClient, _updateInfo.rustClient, _configuration.EnableClient);
                UpdateCheck("ClientStaging", nVData.rustClientStaging, _updateInfo.rustClientStaging, _configuration.EnableStaging);
                UpdateCheck("DevBlog", nVData.newsgid, _updateInfo.newsgid, _configuration.EnableDevBlog, _updateInfo.newsurl);
                UpdateCheck("Oxide", nVData.uMod, _updateInfo.uMod, _configuration.EnableUMod);
                UpdateCheck("Server", nVData.rustServer, _updateInfo.rustServer, _configuration.EnableServer);

                _updateInfo = nVData;

            }, this);
        }

        private void UpdateCheck(string type, string newData, string oldData, bool enableFeature, string additionalInfo = null)
        {
            if (newData == oldData) return;

            Interface.CallHook($"On{type}Update", newData);

            if (!enableFeature) return;

            Puts(Lang($"{type}Updated"));

            if (_configuration.EnableChatNotifications)
                SendToChat(Lang($"{type}Updated"));
            if (_configuration.EnableGuiNotifications)
                SendtoGui(Lang($"{type}Updated"));
            if (_configuration.EnableDiscordNotify)
                SendToDiscord(Lang($"{type}Updated") + (additionalInfo != null ? $": {additionalInfo}" : ""));
        }

        #endregion Version Comparison

        #region Helpers

        private string Lang(string key, string id = null, params object[] args)
            => string.Format(lang.GetMessage(key, this, id), args);

        private bool HasPermission(BasePlayer player, string perm)
        {
            if (permission.UserHasPermission(player.userID.ToString(), perm)) return true;

            PrintWarning("UserID: " + player.UserIDString + " | UserName: " + player.displayName + " | " + Lang("NoPermission"));
            return false;
        }

        private void SendtoGui(string message)
        {
            if (GUIAnnouncements == null)
            {
                PrintWarning(Lang("PluginNotFoundGuiAnnouncements"));
                return;
            }

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (_configuration.OnlyNotifyAdmins)
                {
                    if (!_configuration.OnlyNotifyAdmins || !HasPermission(player, AdminPermission)) continue;

                    GUIAnnouncements?.Call("CreateAnnouncement", message, _configuration.GUINotificationsTintColor, _configuration.GUINotificationsTextColor, player);
                    Puts($"Announcement created for: {player.displayName}: {message}");
                }
                else
                {
                    GUIAnnouncements?.Call("CreateAnnouncement", message, _configuration.GUINotificationsTintColor, _configuration.GUINotificationsTextColor, player);
                    Puts($"Announcement created for: {player.displayName}: {message}");
                }
            }
        }

        private void SendToChat(string message)
        {
            rust.BroadcastChat(null, $"{Lang("Chat.Prefix")}\n{message}");
        }

        private void SendToDiscord(string message)
        {
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };

            var dc = new DiscordMessageEmbeds
            {
                content = _configuration.DiscordRoleId == 0
                    ? message
                    : $"<@&{_configuration.DiscordRoleId}> {message}",
                embeds = new[]
                {
                    new Embed
                    {
                        title = "Update Notice",
                        description = $"Alert Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nVisit [Umod](https://umod.org/plugins/update-notice) for more information or [Discord](https://discord.gg/EyRgFdA) for any assistance.",
                        url = "https://discord.gg/EyRgFdA",
                        color = 16711686,
                        thumbnail = new Thumbnail { url = "https://assets.umod.org/images/icons/plugin/5ea987f1379b2.png" },
                        footer = new Footer { icon_url = "https://assets.umod.org/user/7O3gGkDgaP/14G1myUYST6LEi2.png", text = "Created by Psystec" }
                    }
                }
            };

            var payload = JsonConvert.SerializeObject(dc);
            webrequest.Enqueue(_configuration.DiscordWebhookURL, payload, (code, response) =>
            {
                if (code == 200 || code == 204) return;
                if (response == null)
                {
                    PrintWarning($"Discord didn't respond. Error Code: {code}");
                }
                else
                {
                    Puts($"Discord respond with: {response} Payload: {payload}");
                }
            }, this, Core.Libraries.RequestMethod.POST, headers);
        }

        #endregion Helpers

        #region Internal API

        public string GetServerVersion() => _updateInfo?.rustServer;
        public string GetDevBlogVersion() => _updateInfo?.newsgid;
        public string GetClientVersion() => _updateInfo?.rustClient;
        public string GetStagingVersion() => _updateInfo?.rustClientStaging;
        public string GetUModVersion() => _updateInfo?.uMod;
        public string GetCarbonVersion() => _updateInfo?.carbon;
        public string GetDevBlogLink() => _updateInfo?.newsurl;

        #endregion Internal API

        #region External API

        [PluginReference]
        Plugin GUIAnnouncements;

        #endregion External API
    }
}
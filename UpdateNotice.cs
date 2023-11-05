using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Update Notice", "Psystec", "3.0.2", ResourceId = 2837)]
    [Description("Notifies you when new Rust updates are released.")]
    public class UpdateNotice : RustPlugin
    {
        //DO NOT EDIT. Please ask for permission or notify me should you require changes.
        #region Fields

        private const string AdminPermission = "updatenotice.admin";
        private const string ApiUrl = "http://rust.yamang.xyz:2095/rustapi";
        private Configuration _configuration;
        private UpdateInfo _updateInfo;
        private bool _initLoad = true;
        private int CheckingInterval = 180;

        #endregion Fields

        #region Classes

        private class UpdateInfo
        {
            public string Carbon { get; set; }
            public string UMod { get; set; }
            public string RustClient { get; set; }
            public string RustClientStaging { get; set; }
            public string RustServer { get; set; }
            public string Newsgid { get; set; }
            public string Newsurl { get; set; }
            public string Newsdate { get; set; }
        }

        #region Discord Message
        private class DiscordMessageEmbeds
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
        private class Embed
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
        private class Author
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
        private class Thumbnail
        {
            /// <summary>
            /// url of thumbnail
            /// </summary>
            public string url { get; set; }
        }
        private class Image
        {
            /// <summary>
            /// url of image
            /// </summary>
            public string url { get; set; }
        }
        private class Footer
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
        private class Field
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

        private class Configuration
        {
            [JsonProperty("Only Notify Admin")]
            public bool OnlyNotifyAdmins { get; set; } = false;

            [JsonProperty("Enable Discord Notifications")]
            public bool EnableDiscordNotify { get; set; } = false;

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
                ["StagingUpdated"] = "Staging Update Released!",
                ["UModUpdated"] = "UMod Update Released!",
                ["FailedToCheckUpdates"] = "Failed to check for RUST updates, if this keeps happening please contact the developer.",
                ["PluginNotFoundGuiAnnouncements"] = "GUIAnnouncements plugin was not found. GUI Announcements disabled.",
                ["NoPermission"] = "You do not have permission to use this command.",
                ["DiscordWebhookURLNotConfigured"] = "Discord Webhook URL is not configured.",
                ["IntervalCheck"] = "Checking interval must be 180 seconds or greater! Setting this lower may get your server banned. Auto adjusted to 300.",
            }, this);
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

            if (CheckingInterval < 180)
            {
                PrintWarning(Lang("IntervalCheck"));
                CheckingInterval = 300;
            }

            timer.Every(CheckingInterval, CompareVersions);
        }

        #endregion Hooks

        #region Testing

        [ConsoleCommand("updatenotice")]
        private void AdvertCommands(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            if (!HasPermission(player, AdminPermission))
            {
                SendReply(arg, Lang("NoPermission"));
                return;
            }
            var args = arg?.Args ?? null;

            if (args == null)
            {
                SendReply(arg, ("COMMAND").PadRight(30) + "DESCRIPTION");
                SendReply(arg, ("updatenotice gui").PadRight(30) + "Tests GUI notification (Needs GUIAnnouncements Plugin)");
                SendReply(arg, ("updatenotice discord").PadRight(30) + "Tests Discord notification");
                SendReply(arg, ("updatenotice current").PadRight(30) + "Display's current update versions");
                SendReply(arg, ("updatenotice server").PadRight(30) + "Simulate Server update release");
                SendReply(arg, ("updatenotice devblog").PadRight(30) + "Simulate DevBlog update release");
                SendReply(arg, ("updatenotice client").PadRight(30) + "Simulate Client update release");
                SendReply(arg, ("updatenotice staging").PadRight(30) + "Simulate Staging update release");
                SendReply(arg, ("updatenotice oxide").PadRight(30) + "Simulate Oxide update release");
                SendReply(arg, ("updatenotice all").PadRight(30) + "Simulate all updates released");
                SendReply(arg, ("updatenotice check").PadRight(30) + "Forces a version check");
                SendReply(arg, ("updatenotice loadconfig").PadRight(30) + "Reads the config file");
                return;
            }

            if (args[0] == "gui")
            {
                if (GUIAnnouncements == null)
                {
                    PrintWarning(Lang("PluginNotFoundGuiAnnouncements"));
                    return;
                }

                SendReply(arg, "Testing GUI Messages: Test message from Update Notice by Psystec");
                Puts("Testing GUI Messages: Test message from Update Notice by Psystec");
                SendtoGui("Test message from Update Notice by Psystec");

            }

            if (args[0] == "discord")
            {
                if (_configuration.DiscordWebhookURL == "https://support.discordapp.com/hc/en-us/articles/228383668" || string.IsNullOrEmpty(_configuration.DiscordWebhookURL))
                {
                    PrintWarning(Lang("DiscordWebhookURLNotConfigured"));
                    return;
                }

                SendReply(arg, "Testing Discord Messages: Test message from Update Notice by Psystec");
                Puts("Testing Discord Messages: Test message from Update Notice by Psystec");
                SendToDiscord("Test message from Update Notice by Psystec");
            }

            if (args[0] == "current")
            {
                SendReply(arg, "Update Notice by Psystec\nServer: " + _updateInfo.RustServer + "\n" +
                    "DevBlog: " + _updateInfo.Newsgid + "\n" +
                    "Client: " + _updateInfo.RustClient + "\n" +
                    "Staging: " + _updateInfo.RustClientStaging + "\n" +
                    "UMod: " + _updateInfo.UMod);

                Puts("Update Notice by Psystec\nServer: " + _updateInfo.RustServer + "\n" +
                    "DevBlog: " + _updateInfo.Newsgid + "\n" +
                    "Client: " + _updateInfo.RustClient + "\n" +
                    "Staging: " + _updateInfo.RustClientStaging + "\n" +
                    "UMod: " + _updateInfo.UMod);
            }

            if (args[0] == "server")
            {
                SendReply(arg, "Testing Server Update");
                Puts("Testing Server Update");
                _updateInfo.RustServer = "Simulating Server Update";
            }

            if (args[0] == "devblog")
            {
                SendReply(arg, "Testing Devblog Update");
                Puts("Testing Devblog Update");
                _updateInfo.Newsgid = "Simulating Devblog Update";
            }

            if (args[0] == "client")
            {
                SendReply(arg, "Testing Client Update");
                Puts("Testing Client Update");
                _updateInfo.RustClient = "Simulating Client Update";
            }

            if (args[0] == "staging")
            {
                SendReply(arg, "Testing Stagting Update");
                Puts("Testing Stagting Update");
                _updateInfo.RustClientStaging = "Simulating Stagting Update";
            }

            if (args[0] == "umod")
            {
                SendReply(arg, "Testing UMod Update");
                Puts("Testing UMod Update");
                _updateInfo.UMod = "Simulating UMod Update";
            }

            if (args[0] == "all")
            {
                SendReply(arg, "Testing All Updates");
                Puts("Testing All Updates");
                _updateInfo.RustServer = "Simulating Server Update";
                _updateInfo.Newsgid = "Simulating Devblog Update";
                _updateInfo.RustClient = "Simulating Client Update";
                _updateInfo.RustClientStaging = "Simulating Stagting Update";
                _updateInfo.UMod = "Simulating UMod Update";
            }

            if (args[0] == "forcecheck")
            {
                SendReply(arg, "Forcing Version Check");
                Puts("Forcing Version Check");
                CompareVersions();
            }

            if (args[0] == "loadconfig")
            {
                SendReply(arg, "Loading Configuration File");
                Puts("Loading Configuration File");
                LoadNewConfig();
            }
        }

        #endregion Testing

        #region Version Comparison

        private void CompareVersions()
        {
            webrequest.Enqueue(ApiUrl, null, (code, response) =>
            {
                if (code != 200 || response == null || code == 0)
                {
                    PrintWarning("API: " + Lang("FailedToCheckUpdates") + "\nError Code: " + code + " | Message: " + response);
                    return;
                }

                UpdateInfo nVData = JsonConvert.DeserializeObject<UpdateInfo>(response);

                if (_initLoad)
                {
                    _updateInfo = nVData;
                    _initLoad = false;
                }

                if (nVData.RustClientStaging != _updateInfo.RustClientStaging)
                {
                    Puts(Lang("StagingUpdated"));
                    if (_configuration.EnableStaging)
                    {
                        if (_configuration.EnableChatNotifications)
                            SendToChat(Lang("StagingUpdated"));
                        if (_configuration.EnableGuiNotifications)
                            SendtoGui(Lang("StagingUpdated"));
                        if (_configuration.EnableDiscordNotify)
                            SendToDiscord(Lang("StagingUpdated"));
                    }
                }

                if (nVData.RustClient != _updateInfo.RustClient)
                {
                    Puts(Lang("ClientUpdated"));
                    if (_configuration.EnableClient)
                    {
                        if (_configuration.EnableChatNotifications)
                            SendToChat(Lang("ClientUpdated"));
                        if (_configuration.EnableGuiNotifications)
                            SendtoGui(Lang("ClientUpdated"));
                        if (_configuration.EnableDiscordNotify)
                            SendToDiscord(Lang("ClientUpdated"));
                    }
                }

                if (nVData.Newsgid != _updateInfo.Newsgid)
                {
                    Puts(Lang("DevBlogUpdated"));
                    if (_configuration.EnableDevBlog)
                    {
                        if (_configuration.EnableChatNotifications)
                            SendToChat(Lang("DevBlogUpdated"));
                        if (_configuration.EnableGuiNotifications)
                            SendtoGui(Lang("DevBlogUpdated"));
                        if (_configuration.EnableDiscordNotify)
                            SendToDiscord(Lang("DevBlogUpdated") + ": " + _updateInfo.Newsurl);
                    }
                }

                if (nVData.UMod != _updateInfo.UMod)
                {
                    Puts(Lang("UModUpdated"));
                    if (_configuration.EnableUMod)
                    {
                        if (_configuration.EnableChatNotifications)
                            SendToChat(Lang("UModUpdated"));
                        if (_configuration.EnableGuiNotifications)
                            SendtoGui(Lang("UModUpdated"));
                        if (_configuration.EnableDiscordNotify)
                            SendToDiscord(Lang("UModUpdated"));
                    }
                }

                if (nVData.RustServer != _updateInfo.RustServer)
                {
                    Puts(Lang("ServerUpdated"));
                    if (_configuration.EnableServer)
                    {
                        if (_configuration.EnableChatNotifications)
                            SendToChat(Lang("ServerUpdated"));
                        if (_configuration.EnableGuiNotifications)
                            SendtoGui(Lang("ServerUpdated"));
                        if (_configuration.EnableDiscordNotify)
                            SendToDiscord(Lang("ServerUpdated"));
                    }
                }

                _updateInfo = nVData;

            }, this, Core.Libraries.RequestMethod.GET);
        }

        #endregion Version Comparison

        #region Helpers

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        private bool HasPermission(BasePlayer player, string perm)
        {
            if (!permission.UserHasPermission(player.userID.ToString(), perm))
            {
                PrintWarning("UserID: " + player.UserIDString + " | UserName: " + player.displayName + " | " + Lang("NoPermission"));
                return false;
            }
            return true;
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
                    if (_configuration.OnlyNotifyAdmins && HasPermission(player, AdminPermission))
                    {
                        GUIAnnouncements?.Call("CreateAnnouncement", message, _configuration.GUINotificationsTintColor, _configuration.GUINotificationsTextColor, player);
                        Puts($"Announcement created for: {player.displayName}: {message}");
                    }
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
            rust.BroadcastChat(null, $"<size=20><color=#ff0000>Update Notice</color></size>\n{message}");
        }
        private void SendToDiscord(string message)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");

            DiscordMessageEmbeds dc = new DiscordMessageEmbeds();
            dc.content = message;
            dc.embeds = new[]
            {
                new Embed
                {
                    title = "Update Notice",
                    description = $"Alert Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\nVisit [Umod](https://umod.org/plugins/update-notice) for more information or [Discord](https://discord.gg/EyRgFdA) for any assistance.",
                    url = "https://discord.gg/eZcFanf",
                    color = 16711686,
                    thumbnail = new Thumbnail { url = "https://assets.umod.org/images/icons/plugin/5ea987f1379b2.png" },
                    footer = new Footer { icon_url = "https://assets.umod.org/user/7O3gGkDgaP/14G1myUYST6LEi2.png", text = "Created by Psystec" }
                }

            };

            string payload = JsonConvert.SerializeObject(dc);
            webrequest.Enqueue(_configuration.DiscordWebhookURL, payload, (code, response) =>
            {
                if (code != 200 && code != 204)
                {
                    if (response == null)
                    {
                        PrintWarning($"Discord didn't respond. Error Code: {code}");
                    }
                    else
                    {
                        Puts($"Discord respond with: {response} Payload: {payload}");
                    }
                }
            }, this, Core.Libraries.RequestMethod.POST, headers);
        }

        #endregion Helpers

        #region Internal API

        public string GetServerVersion() => _updateInfo.RustServer;
        public string GetDevBlogVersion() => _updateInfo.Newsgid;
        public string GetClientVersion() => _updateInfo.RustClient;
        public string GetStagingVersion() => _updateInfo.RustClientStaging;
        public string GetUModVersion() => _updateInfo.UMod;
        public string GetDevBlogLink() => _updateInfo.Newsurl;

        #endregion Internal API

        #region External API
        [PluginReference]
        Plugin GUIAnnouncements;
        #endregion External API
    }
}

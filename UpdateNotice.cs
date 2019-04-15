using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    /* API hosting provided by Cyrus the Virus @ saltfactoryhosting.com
     * Salt Factory Hosting Discord: discord.gg/nedgasy
     * DO NOT EDIT THE CODE BELOW. Use the config file ..\oxide\config\UpdateNotice.json
     */

     // Added error codes to show if it is steam or the API giving an error.

    [Info("Update Notice", "Psystec", "1.1.1", ResourceId = 2837)]
    [Description("Notifies you when new Rust updates are released.")]
    public class UpdateNotice : RustPlugin
    {
        #region Fields

        private const string AdminPermission = "updatenotice.admin";
        private const string ApiUrl = "https://saltfactoryhosting.com/api/update/rust";
        private Configuration _configuration;
        private int _devBlogId = 0, _port = 0, _serverBuildId = 0, _clientBuildId = 0, _stagingBuildId = 0, _oxideBuildId = 0, _version = 0;

        #endregion Fields

        #region Classes

        private class UpdateInfo
        {
            public int api { get; set; }
            public int client { get; set; }
            public int oxide { get; set; }
            public int server { get; set; }
            public int staging { get; set; }
        }

        private class DevBlog
        {
            public Appnews appnews { get; set; }
        }

        private class Appnews
        {
            public int appid { get; set; }
            public Newsitem[] newsitems { get; set; }
            public int count { get; set; }
        }

        private class Newsitem
        {
            public string gid { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public bool is_external_url { get; set; }
            public string author { get; set; }
            public string contents { get; set; }
            public string feedlabel { get; set; }
            public int date { get; set; }
            public string feedname { get; set; }
            public int feed_type { get; set; }
            public int appid { get; set; }
        }

        private class DiscordMessage
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

        #endregion Classes

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Only Notify Admins")]
            public bool OnlyNotifyAdmins { get; set; } = false;

            [JsonProperty("Enable Discord Notifications")]
            public bool EnableDiscordNotify { get; set; } = false;

            [JsonProperty("Discord Webhook URL")]
            public string DiscordWebhookURL { get; set; } = "https://support.discordapp.com/hc/en-us/articles/228383668";

            [JsonProperty("Enable Gui Notifications")]
            public bool EnableGuiNotifications { get; set; } = true;

            [JsonProperty("GUI Removal Delay (in Seconds)")]
            public int GuiRemovalDelay { get; set; } = 300;

            [JsonProperty("Enable Server Version Notifications")]
            public bool EnableServer { get; set; } = true;

            [JsonProperty("Enable DevBlog Notifications")]
            public bool EnableDevBlog { get; set; } = true;

            [JsonProperty("Enable Client Version Notifications")]
            public bool EnableClient { get; set; } = true;

            [JsonProperty("Enable Staging Version Notifications")]
            public bool EnableStaging { get; set; } = false;

            [JsonProperty("Enable Oxide Version Notifications")]
            public bool EnableOxide { get; set; } = false;

            [JsonProperty("Checking Interval (in Seconds)")]
            public int CheckingInterval { get; set; } = 180;
        }

        protected override void LoadDefaultConfig()
        {
            _configuration = new Configuration();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _configuration = Config.ReadObject<Configuration>();

            if (_configuration.CheckingInterval < 180)
            {
                PrintWarning("Checking interval must be 180 seconds or greater! Setting this lower may get your server banned. Auto adjusted to 300.");
                _configuration.CheckingInterval = 300;
            }
        }

        protected override void SaveConfig() => Config.WriteObject(_configuration);

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
                ["OxideUpdated"] = "Oxide Update Released!",
                ["UpdateNoticeApiUpdated"] = "Update Notice API Updated",
                ["FailedToCheckUpdates"] = "Failed to check for RUST updates, if this keeps happening please contact the developer."
            }, this);
        }

        #endregion Localization

        #region Hooks

        private void Init()
        {
            _port = ConVar.Server.port;
            LoadConfig();
            permission.RegisterPermission(AdminPermission, this);
        }

        private void Loaded()
        {
            CompareBuilds();
            timer.Every(_configuration.CheckingInterval, CompareBuilds);
        }

        #endregion Hooks

        #region Testing

        [ChatCommand("updatenotice")]
        private void UpdateNoticeTest(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            if (args[0] == "current")
            {
                //SendReply(player, "Server: " + _serverBuildId.ToString());
                //SendReply(player, "Client: " + _clientBuildId.ToString());
                //SendReply(player, "Staging: " + _stagingBuildId.ToString());
                //SendReply(player, "Oxide: " + _oxideBuildId.ToString());
                //SendReply(player, "Version: " + _version.ToString());

                SendReply(player, "Server: " + _serverBuildId.ToString() + "\n" +
                    "DevBlog: " + _devBlogId.ToString() + "\n" +
                    "Client: " + _clientBuildId.ToString() + "\n" +
                    "Staging: " + _stagingBuildId.ToString() + "\n" +
                    "Oxide: " + _oxideBuildId.ToString() + "\n" +
                    "API: " + _version.ToString());
            }

            if (args[0] == "discord")
            {
                SendToDiscord("Test Message from Rust server.");
            }

            if (args[0] == "server")
            {
                _serverBuildId = 1;
            }

            if (args[0] == "devblog")
            {
                _devBlogId = 1;
            }

            if (args[0] == "client")
            {
                _clientBuildId = 1;
            }

            if (args[0] == "staging")
            {
                _stagingBuildId = 1;
            }

            if (args[0] == "oxide")
            {
                _oxideBuildId = 1;
            }

            if (args[0] == "api")
            {
                _version = 999;
            }

            if (args[0] == "all")
            {
                _serverBuildId = 1;
                _devBlogId = 1;
                _clientBuildId = 1;
                _stagingBuildId = 1;
                _oxideBuildId = 1;
            }

            if (args[0] == "removegui")
            {
                RemoveGuiForAll();
            }
        }

        #endregion Testing

        #region Build Comparison

        private void CompareBuilds()
        {
            webrequest.Enqueue(ApiUrl + "/" + _port.ToString(), null, (code, response) =>
            {
                if (code != 200)
                {
                    if (code == 0)
                    {
                        return;
                    }

                    PrintWarning("API: " + Lang("FailedToCheckUpdates") + "\nError Code: " + code + " | Message: " + response);
                    return;
                }

                var updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(response);

                if (_serverBuildId == 0 || _clientBuildId == 0 || _stagingBuildId == 0 || _oxideBuildId == 0 || _version == 0)
                {
                    _serverBuildId = updateInfo.server;
                    _clientBuildId = updateInfo.client;
                    _stagingBuildId = updateInfo.staging;
                    _oxideBuildId = updateInfo.oxide;
                    _version = updateInfo.api;
                }
                else
                {
                    bool serverUpdated = _serverBuildId != updateInfo.server;
                    bool clientUpdated = _clientBuildId != updateInfo.client;
                    bool stagingUpdated = _stagingBuildId != updateInfo.staging;
                    bool oxideUpdated = _oxideBuildId != updateInfo.oxide;
                    bool versionUpdated = _version != updateInfo.api;

                    if (!serverUpdated && !clientUpdated && !stagingUpdated && !oxideUpdated && !versionUpdated)
                    {
                        return;
                    }

                    if (versionUpdated)
                    {
                        _version = updateInfo.api;
                        PrintWarning(Lang("UpdateNoticeApiUpdated"));

                        if (_configuration.EnableServer && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("UpdateNoticeApiUpdated"));
                        }

                        if (_configuration.EnableServer && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("UpdateNoticeApiUpdated"));
                        }
                    }
                    if (serverUpdated)
                    {
                        _serverBuildId = updateInfo.server;
                        PrintWarning(Lang("ServerUpdated"));

                        if (_configuration.EnableServer && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("ServerUpdated"));
                        }

                        if (_configuration.EnableServer && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("ServerUpdated"));
                        }
                    }
                    if (clientUpdated)
                    {
                        _clientBuildId = updateInfo.client;
                        PrintWarning(Lang("ClientUpdated"));

                        if (_configuration.EnableClient && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("ClientUpdated"));
                        }

                        if (_configuration.EnableClient && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("ClientUpdated"));
                        }
                    }
                    if (stagingUpdated)
                    {
                        _stagingBuildId = updateInfo.staging;
                        PrintWarning(Lang("StagingUpdated"));

                        if (_configuration.EnableStaging && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("StagingUpdated"));
                        }

                        if (_configuration.EnableStaging && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("StagingUpdated"));
                        }
                    }
                    if (oxideUpdated)
                    {
                        _oxideBuildId = updateInfo.oxide;
                        PrintWarning(Lang("OxideUpdated"));

                        if (_configuration.EnableOxide && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("OxideUpdated"));
                        }

                        if (_configuration.EnableOxide && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("OxideUpdated"));
                        }
                    }
                }
            }, this);

            webrequest.Enqueue("http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=252490&count=1&maxlength=300&format=json", null, (code, response) =>
            {
                if (code != 200)
                {
                    if (code == 0)
                    {
                        return;
                    }

                    PrintWarning("Steam: " + Lang("FailedToCheckUpdates") + "\nError Code: " + code + " | Message: " + response);
                    return;
                }

                var DevBlogInfo = JsonConvert.DeserializeObject<DevBlog>(response);

                if (_devBlogId == 0)
                {
                    _devBlogId = DevBlogInfo.appnews.newsitems[0].date;
                }
                else
                {
                    bool devBlogUpdated = _devBlogId != DevBlogInfo.appnews.newsitems[0].date;

                    if (devBlogUpdated)
                    {
                        _devBlogId = DevBlogInfo.appnews.newsitems[0].date;
                        PrintWarning(Lang("DevBlogUpdated"));

                        if (_configuration.EnableDevBlog && _configuration.EnableGuiNotifications)
                        {
                            DrawGuiForAll(Lang("DevBlogUpdated"));
                        }

                        if (_configuration.EnableDevBlog && _configuration.EnableDiscordNotify)
                        {
                            SendToDiscord(Lang("DevBlogUpdated"));
                        }
                    }
                }

            }, this, Core.Libraries.RequestMethod.GET);
        }

        #endregion Build Comparison

        #region Gui Handling

        private void RemoveGuiAfterDelay(int delay) => timer.Once(delay, RemoveGuiForAll);

        private void RemoveGuiForAll()
        {
            BasePlayer.activePlayerList.ForEach(RemoveGui);
            GuiTracker = 0;
            y = 0.98;
        }

        private void RemoveGui(BasePlayer player)
        {
            for (int i = 0; i <= GuiTracker; i++)
            {
                CuiHelper.DestroyUi(player, "UpdateNotice" + i.ToString());
            }
        }

        private void DrawGuiForAdmins(string message)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.IsAdmin && HasPermission(player, AdminPermission))
                {
                    AddGui(player, message);
                }
            }
            RemoveGuiAfterDelay(_configuration.GuiRemovalDelay);
        }

        private void DrawGuiForAll(string message)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (_configuration.OnlyNotifyAdmins && !HasPermission(player, AdminPermission))
                {
                    continue;
                }

                AddGui(player, message);
            }
            RemoveGuiAfterDelay(_configuration.GuiRemovalDelay);
        }

        private double y = 0.98;
        private int GuiTracker = 0;
        private void AddGui(BasePlayer player, string message)
        {
            y = y - 0.025;
            GuiTracker++;
            var container = new CuiElementContainer();

            var panel = container.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.1 0.1 0.1 0.5"
                },
                RectTransform =
                {
                    AnchorMin = "0.012 " + y.ToString(), // left down
			        AnchorMax = "0.25 " + (y + 0.02).ToString() // right up
		        },
                CursorEnabled = false
            }, "Hud", "UpdateNotice" + GuiTracker.ToString());
            container.Add(new CuiLabel
            {
                Text =
                {
                    Text = message,
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.0 8.0 0.0 1.0"
                },
                RectTransform =
                {
                    AnchorMin = "0.00 0.00",
                    AnchorMax = "1.00 1.00"
                }
            }, panel);
            CuiHelper.AddUi(player, container);
            y = y - 0.005;
        }

        #endregion Procedures

        #region Helpers

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        private bool HasPermission(BasePlayer player, string perm) => permission.UserHasPermission(player.userID.ToString(), perm);

        private void SendToDiscord(string message)
        {
            DiscordMessage dm = new DiscordMessage();
            dm.content = message;
            string payload = JsonConvert.SerializeObject(dm);

            webrequest.Enqueue(_configuration.DiscordWebhookURL, payload, (dcode, dresponse) =>
            {
                if (dcode != 200 && dcode != 204)
                {
                    if (dresponse == null)
                    {
                        PrintWarning($"Discord didn't respond. Error Code: {dcode}");
                    }
                }
            }, this, Core.Libraries.RequestMethod.POST);
        }

        #endregion Helpers

        #region User API

        private int GetServerVersion() => _serverBuildId;  // Returns 0 if version could not be determined
        private int GetDevBlogVersion() => _devBlogId;   // Returns 0 if version could not be determined (date)
        private int GetClientVersion() => _clientBuildId;  // Returns 0 if version could not be determined
        private int GetStagingVersion() => _stagingBuildId;  // Returns 0 if version could not be determined
        private int GetOxideVersion() => _oxideBuildId;  // Returns 0 if version could not be determined

        #endregion User API
    }
}
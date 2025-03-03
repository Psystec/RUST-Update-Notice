Notifies players when a server, devblog, client and oxide update has been released with a GUI notification.

Psystec's Discord: discord. /EyRgFdA

Server sponsored by **YaMang -w-**   :   [Click here to see if the plugin's server is alive](https://status.yamang.xyz/)

Please visit his discord and say thank you if you find this plugin usefull: discord. /9fHkBftMb

## Features
 
Update Notice can be used to notify you when the following updates is released:

* Carbon
* Client
* ClientStaging
* DevBlog
* Oxide
* Server

You can also be notified via:

* Ingame Chat
* [GUI Announcements](https://umod.org/plugins/gui-announcements)
* Discord

## Permissions
 
To use the `Only Notify Admin` option, add the following premission to a group or player: `updatenotice.admin`

## Configuration
 
```json
{
  "Only Notify Admin": false,
  "Enable Discord Notifications": false,
  "Discord role id to mention (0 = no mention)": 0,
  "Discord Webhook URL": "https://support.discordapp.com/hc/en-us/articles/228383668",
  "Enable GUI Notifications (Needs GUIAnnouncements)": true,
  "Enable Chat Notifications": true,
  "GUI Notifications Tint Color": "Purple",
  "GUI Notifications Text Color": "Yellow",
  "Enable Server Version Notifications": true,
  "Enable DevBlog Notifications": true,
  "Enable Client Version Notifications": true,
  "Enable Staging Version Notifications": false,
  "Enable UMod Version Notifications": false,
  "Enable Carbon Version Notifications": false
}
```

## Localization

 ```json
{
  "ServerUpdated": "Server Update Released!",
  "DevBlogUpdated": "DevBlog Update Released!",
  "ClientUpdated": "Client Update Released!",
  "StagingUpdated": "Staging Update Released!",
  "UModUpdated": "UMod Update Released!",
  "CarbonUpdated": "Carbon Update Released!",
  "FailedToCheckUpdates": "Failed to check for RUST updates, if this keeps happening please contact the developer.",
  "PluginNotFoundGuiAnnouncements": "GUIAnnouncements plugin was not found. GUI Announcements disabled.",
  "NoPermission": "You do not have permission to use this command.",
  "DiscordWebhookURLNotConfigured": "Discord Webhook URL is not configured.",
  "IntervalCheck": "Checking interval must be 180 seconds or greater! Setting this lower may get your server banned. Auto adjusted to 300.",
  "Help.Command": "COMMAND",
  "Help.Description": "DESCRIPTION",
  "Help.Gui": "Tests GUI notification (Needs GUIAnnouncements Plugin)",
  "Help.Discord": "Tests Discord notification",
  "Help.Current": "Display's all current versions",
  "Help.Server": "Simulate Server update release",
  "Help.DevBlog": "Simulate DevBlog update release",
  "Help.Client": "Simulate Client update release",
  "Help.Staging": "Simulate Staging update release",
  "Help.Oxide": "Simulate Oxide update release",
  "Help.Carbon": "Simulate Carbon update release",
  "Help.All": "Simulate all updates released (depends on config)",
  "Help.ForceCheck": "Forces a version check",
  "Help.LoadConfig": "Reload the config file",
  "Chat.Prefix": "<size=20><color=#ff0000>Update Notice</color></size>"
}
```

## Discord Notifications

1. Change the setting `Enable Discord Notifications` to `true`
2. Replace the Webhook URL setting `Discord Webhook URL` with your custom URL from Discord.
2.1 Set a role id to mention in `Discord role id to mention (0 = no mention)` or not
3. In the console type `updatenotice discord` to send a test message to Discord.

## For Developers

### Hooks

```csharp
UpdateNotice.Call<string>("GetCarbonVersion");
UpdateNotice.Call<string>("GetClientVersion");
UpdateNotice.Call<string>("GetClientStagingVersion");
UpdateNotice.Call<string>("GetDevBlogLink");
UpdateNotice.Call<string>("GetDevBlogVersion");
UpdateNotice.Call<string>("GetOxideVersion");
UpdateNotice.Call<string>("GetServerVersion");
```

```csharp
[PluginReference] private Plugin UpdateNotice;

private void Init()
{
    string serverVersion = UpdateNotice.Call<string>("GetServerVersion");
}
```

### Events

```csharp
void OnCarbonUpdate(string version);
void OnClientUpdate(string version);
void OnClientStagingUpdate(string version);
void OnDevBlogUpdate(string version);
void OnOxideUpdate(string version);
void OnServerUpdate(string version);
```

```csharp
public class Plugin : RustPlugin
{
    void OnServerUpdate(string version)
    {
        Puts($"Server got updated! - {version}");
    }
}
```

## Testing

When testing messages, the message won't appear immediatly. it will wait till the next API request.

Console Commands:

- `updatenotice gui` -- Test GUI notification
- `updatenotice discord` -- Test Discord notification
- `updatenotice current` -- Display current update versions

- `updatenotice server` -- Simulate Server update release
- `updatenotice devblog` -- Simulate DevBlog update release
- `updatenotice client` -- Simulate Client update release
- `updatenotice clientstaging` -- Simulate Staging update release
- `updatenotice umod` -- Simulate Oxide update release
- `updatenotice carbon` -- Simulate Carbon update release

- `updatenotice all` -- Simulate all updates released
- `updatenotice forcecheck` -- Forces a version check
- `updatenotice loadconfig` -- Reads the config file

## Examples

- GUI Notification:

![](https://i.imgur.com/S53hip4.png)

- Discord Notification:

![](https://i.imgur.com/C3m1Pkc.png)
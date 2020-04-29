## Announcement 
API hosting provided by Cyrus the Virus @ saltfactoryhosting.com.

For any information or immediate help please find me on Discord: 

## About
Notifies players when a client and server update has been released with a GUI notification.

 ## Permissions
- `updatenotice.admin` -- For admin only notifications.

 ## Features
 Update Notice can be used to notify you when the following updates is released:
*  Server
*  DevBlog
*  Client
*  Staging
*  Oxide (Umod)

You can also be notified via:
* Ingame Chat
* GUI Announcements
* Discord

 ## Configuration
```
{
  "Only Notify Admin": false,
  "Enable Discord Notifications": false,
  "Discord Webhook URL": "https://support.discordapp.com/hc/en-us/articles/228383668",
  "Enable Gui Notifications": true,
  "Enable Chat Notifications": true,
  "GUI Notifications Tint Color": "Purple",
  "GUI Notifications Text Color": "Yellow",
  "Enable Server Version Notifications": true,
  "Enable DevBlog Notifications": true,
  "Enable Client Version Notifications": true,
  "Enable Staging Version Notifications": false,
  "Enable Oxide Version Notifications": false,
  "Checking Interval (in Seconds)": 300
}
```

 ## Localization
 ```
{
  "ServerUpdated": "Server Update Released!",
  "DevBlogUpdated": "DevBlog Update Released!",
  "ClientUpdated": "Client Update Released!",
  "StagingUpdated": "Staging Update Released!",
  "OxideUpdated": "Oxide Update Released!",
  "FailedToCheckUpdates": "Failed to check for RUST updates, if this keeps happening please contact the developer.",
  "PluginNotFoundGuiAnnouncements": "GUIAnnouncements plugin was not found. Disabled by defaut.",
  "NoPermission": "You do not have permission to use this command."
}
```
 
 ## Discord Notifications
 Official Discord [Documentation](https://support.discordapp.com/hc/en-us/articles/228383668).

1. Change the setting `Enable Discord Notifications` to `true`
2. Replace the Webhook URL setting `Discord Webhook URL` with your custom URL from discord.
3. In the console type `updatenotice discord` to send a test message to discord.
 
 ## API
- All "." has been removed from the version results.
- Returns 0 if version could not be determined.
```
UpdateNotice.Call<int>("GetServerVersion");
UpdateNotice.Call<int>("GetDevBlogVersion");
UpdateNotice.Call<int>("GetClientVersion");
UpdateNotice.Call<int>("GetStagingVersion");
UpdateNotice.Call<int>("GetOxideVersion");
```
API Usage Example:
```
[PluginReference]
readonly Plugin UpdateNotice;

private void Init()
{
    int serverVersion = UpdateNotice.Call<int>("GetServerVersion");
}
```

## Testing
When testing messages, the message won't appear immediatly. it will wait till the next API request.

Console Commands:

- `updatenotice gui` -- Tests GUI notification
- `updatenotice discord` -- Tests Discord notification
- `updatenotice current` -- Display's current update versions
- `updatenotice server` -- Simulate Server update release
- `updatenotice devblog` -- Simulate DevBlog update release
- `updatenotice client` -- Simulate Client update release
- `updatenotice staging` -- Simulate Staging update release
- `updatenotice oxide` -- Simulate Oxide update release
- `updatenotice all` -- Simulate all updates released
- `updatenotice check` -- Test GUI notification

 ## Notification Example
- GUI Notification:

![](https://i.imgur.com/S53hip4.png)

- Discord Notification:

![](https://i.imgur.com/C3m1Pkc.png)
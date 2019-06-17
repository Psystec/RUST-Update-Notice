## Announcement 
API hosting provided by Cyrus the Virus @ saltfactoryhosting.com.
For any information or immediate help please find me on Discord: https://discord.gg/eZcFanf

## About
Notifies players when a client and server update has been released with a GUI notification.

 ## Permissions
- `updatenotice.admin` -- For admin only notifications.

 ## Features
 - `Only Notify Admins` -- Notifies all players when an update is released else only notifies admins.
 - `Enable Discord Notifications` -- Enables Discord notifications.
 - `Enable Gui Notifications` -- Enables ingame GUI notifications.
 - `GUI Removal Delay (in Seconds)` -- Interval in seconds to keep the update notification shown after the client update has been released.
 - `Enable Server Version Notifications` -- Enables Server update notifications.
 - `Enable DevBlog Version Notifications` -- Enables DevBlog update notifications.
 - `Enable Client Version Notifications` -- Enables Client update notifications.
 - `Enable Staging Version Notifications` -- Enables Stating update notifications.
 - `Enable Oxide Version Notifications` -- Enables Oxide update notifications.
 - `Checking Interval (in Seconds)` -- Frequency in seconds to check for updates.


 ## Configuration
```
{
  "Only Notify Admins": false,
  "Enable Discord Notifications": false,
  "Discord Webhook URL": "https://support.discordapp.com/hc/en-us/articles/228383668",
  "Enable Gui Notifications": true,
  "GUI Removal Delay (in Seconds)": 300,
  "Enable Server Version Notifications": true,
  "Enable DevBlog Notifications": true,
  "Enable Client Version Notifications": true,
  "Enable Staging Version Notifications": false,
  "Enable Oxide Version Notifications": false,
  "Checking Interval (in Seconds)": 180
}
```

 ## Localization
 ```
{
  "ServerUpdated": "Server Update Released!",
  "DevBlogUpdated": "Dev Blog Update Released!",
  "ClientUpdated": "Client Update Released!",
  "StagingUpdated": "Staging Update Released!",
  "OxideUpdated": "Oxide Update Released!",
  "UpdateNoticeApiUpdated": "Update Notice API Updated",
  "FailedToCheckUpdates": "Failed to check for RUST updates, if this keeps happening please contact the developer."
}
```
 
 ## Discord Notifications
 Official Discord [Documentation](https://support.discordapp.com/hc/en-us/articles/228383668).

1. Download the [Discord Messages](https://umod.org/plugins/discord-messages) plugin.
2. Create a Webhook in Discord and copy the Webhook URL.
3. Enable Discord Notifications in the Update Notice config file.
4. Paste the Webhook URL in the Update Notice config file.
5. Save and reload the Update Notice Plugin: `oxide.reload UpdateNotice`
 
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

Chat Commands:

- `/updatenotice discord` -- Sends a test message to Discord.
- `/updatenotice server` -- Forces server updated message.
- `/updatenotice devblog` -- Forces DevBlog updated message.
- `/updatenotice client` -- Forces client updated message.
- `/updatenotice staging` -- Forces staging updated message.
- `/updatenotice oxide` -- Forces oxide updated message.
- `/updatenotice api` -- Forces plugin update console message.
- `/updatenotice all` -- Forces all update messages.
- `/updatenotice removegui` -- Removes GUI messages for all players.

 ## Notification Example
- GUI Notification:

![](https://i.imgur.com/98YO51j.png)

- Discord Notification:

![](https://i.imgur.com/JCJ4iSf.png)
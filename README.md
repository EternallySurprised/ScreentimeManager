# ScreentimeManager
## Description
ScreentimeManager is a hosted application used to monitor the screentime of a Windows computer on the local network and shut the computer down once a predefined screentime is exceeded. This allows e.g. parent to enforce screentime limits. Just setting a playtime limit in Steam doen't cut it.

## Computer Configuration
### Basics
There are some things that need to be in place for remote shutdowns to work on a Windows machine:
- The system running ScreentimeManager needs to be on the same network as the target computer
- You need to have a user on the target system that has administrative privileges and know that user's credentials
- Appropriate firewall settings (see below)

### Configuration Steps
**Enable the "Remote Registry Service" on the target computer as follows:**
This is required to allow Linux tools to authenticate at the system.
- Press _Win + R_, type _services.msc_ and press Enter.
- Find "Remote Registry" in the list.
- Right-click > Properties > Set Startup type to Automatic.
- Start the service if it is not running.

**Grant Shutdown Privileges via local Group Policy**
This is required so that your admin user has the privileges to shutdown the computer remotely
- Press _Win + R_, type _gpedit.msc_ and press Enter. This requires Windows Pro/Enterprise! For Home, use _secpol.msc_ instead.
- Navigate to: _Computer Configuration_ > _Windows Settings_ > _Security Settings_ > _Local Policies_ > _User Rights Assignment_
- Open "Force shutdown from a remote system".
- Click _Add User or Group_ and add your admin user to the list.
- Apply the changes

**Add the required firewall settings**
This is required to be able to actually receive the shutdown command
- Open "Windows Defender Firewall with Advanced Security".
- Add a new rule at _Inbound Rules_ > _New Rule_ > _Port_ > _TCP_ > _Specific local ports: **445**_ > _Next_.
- _Allow the connection_ > Next > _Check: Domain, Private, and/or Public, depending on how your network is classified_ > _Next_
- Name the rule and click _Finish_.

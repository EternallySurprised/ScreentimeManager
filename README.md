# _ScreentimeManager_
## Description
_ScreentimeManager_ is a hosted application used to monitor the screentime of a Windows computer on the local network and shut the computer down once a predefined screentime is exceeded. This allows e.g. parent to enforce screentime limits. Just setting a playtime limit in Steam doen't cut it.

## Features
The _ScreentimeManager_ is a very simple tool with the following core features:
- It tracks the daily screentime of a single target computer.
- A daily screentime limit can be set and the target system is shut down remotely once the screentime limit is exceeded.
- If the target system is restarted after it was shutdown, a new shutdown will be triggered.
- Notifications can be sent to a Discord server using a [Discord Webhook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks).
- WebAPI for integration into a dashboard, for example [homepage](https://github.com/gethomepage/homepage)

## Target Computer Configuration
There are some things that need to be in place for remote shutdowns to work on a Windows machine:
- The system running _ScreentimeManager_ needs to be on the same network as the target computer
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

## Configuration
Configuration of the _ScreentimeManager_ is done using the following environment variables:
| Variable | Description |
| --- | --- |
| `CULTURE` | Defines the *localization culture* used for the notification messages. |
| `SCREENTIME_LIMIT_MINUTES` | The daily screentime limit in **minutes** |
| `SCREENTIME_COUNT_INTERVAL_MS` | The interval with which the screentime is counted/updated in **milliseconds** |
| `PING_INTERVAL_MS` | The interval with which the online status of the target computer is updated in **milliseconds** |
| `PING_TIMEOUT_MS` | The timeout for the target computer to respond to online status checks in **milliseconds** |
| `WEBHOOK_URL` | The Discord webhook URL to send notifications to. |
| `PING_HOST` | Hostname or IP of the target computer. |
| `HOST_USER` | Username for remote shutdown. This user has to have administrative privileges on the target computer. |
| `PASSWD` | The password for the above mentioned user. |

## Discord Webhook
You need to create a Discord webhook for notifications to be sent. Refer to the [Discord Webhook Introduction](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) on how to do this.

## Build & Import image into Docker
After locally building the project, you need to build the Docker image and get is as a file which you can import into Docker on the production system.

To do so, with the .sln opened in Visual Studio, open a Developer PowerShell window and execute:\
`docker build -f ScreentimeManagerApp/Dockerfile -t screentimemanager`\
`docker save -o screentimemanager.tar screentimemanager`

This leaves you with a tarball of the image which can be imported on the production system via\
`docker import screentimemanager.tar screentimemanager`

## Deployment using `docker compose`
The following is an example docker compose file for running _ScreentimeManager_:
```yaml
services:
  screentimemmanager_example:
    image: screentimemanager:latest  # Your local image name and tag. If none was given, it will be "latest"  
    container_name: screentimemanager_example
    environment:
      TZ: Europe/Berlin  # The timezone is required to make sure screentime is reset at midnight local time and logging has correct timestamps
      CULTURE: de-De
      SCREENTIME_LIMIT_MINUTES: 180
      SCREENTIME_COUNT_INTERVAL_MS: 1000
      PING_INTERVAL_MS: 10000
      PING_TIMEOUT_MS: 200
      WEBHOOK_URL: "https://discord.com/api/webhooks/SomeLongStringThatOnlyYouKnow"  # See Discord documentation how to acquire this
      PING_HOST: "ExampleComputer"
      HOST_USER: Admin
      PASSWD: ChangeMe
    ports:
      - "8080:8080/tcp"  # We make the API port available
    network_mode: bridge
    dns: 
      - 192.168.0.1      # Allows us to use our local DNS server to resolve hostnames outside Docker
```

## WebAPI
The WebAPI for _ScreentimeManager_ is very simple. There is an endpoint available for _GET_ requests which provides status information:\
`http://[Server:Port]/api/status`
This endpoint returns a JSON object as follows:
```json
{
  "hostname": "ExampleComputer",
  "isOnline": false,
  "screentimeLeft": "03:00:00",
  "screentimeLimit": "03:00:00",
  "screentimeUsedPercent": 0
}
```
There is also a set of _PUT_ endpoint available which allows to add or subtract screentime for the current day or set the remaining screentime to zero.\
The configured screentime limit is not changed which means that on the following day, the previously configured screentime limit is used.\
The endpoints are:\
`http://[Server:Port]/api/screentime/add/[minutes]`\
`http://[Server:Port]/api/screentime/subtract/[minutes]`\
`http://[Server:Port]/api/screentime/end`\
These endpoints all return the new, remaining screentime on success.
```json
"03:30:00"
```


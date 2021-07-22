[![Finsemble Logo](https://documentation.finsemble.com/styles/img/Finsemble+Cosaic.svg)](https://documentation.chartiq.com/finsemble/)

## Finsemble's Bloomberg Terminal Connect Integration

Welcome to Finsemble's integration with [Bloomberg Terminal Connect](https://www.bloomberg.com/terminal-connect/), which enables Finsemble components and services to interoperate with Bloomberg panels or Launchpad components, allowing you to build powerful workflows for your users that avoid data re-entry and copy/paste errors.

Specifically, the integration enables:

- Connection to a local or remote Bloomberg terminal
- Execution of functions in a Bloomberg Panel
- Bi-directional data sharing between Bloomberg worksheets and Finsemble
- Bi-directional data sharing between Launchpad groups and Finsemble
- Search functions for Bloomberg security definitions (available for local conneciton only, via Bloomberg DAPI)

By using this integration with Finsemble, your custom applications can drive context in the Bloomberg Terminal or react to context changes received from it.

To use the integration you will need access to both a Bloomberg Terminal and license for Terminal Connect. For more information on Terminal Connect, run `TMCT`**\<GO\>** in your Bloomberg terminal. For help designing your integration, contact the Finsemble [Solutions Engineering team](mailto:support@finsemble.com).

## Table of Contents

- [How it works](#how-it-works)
- [Installation](#installation)
  - [Files](#files)
  - [Installation via the watch script](#installation-via-the-watch-script)
  - [Manual installation](#manual-installation)
- [Working with a Remote terminal](#working-with-a-remote-terminal)
  - [Remote connection requirements](#remote-connection-requirements)
  - [Configuring the terminal location](#configuring-the-terminal-location)
  - [Connection status and config UIs](#connection-status-and-config-uis)
- [Building and Deploying the Bloomberg Bridge](#building-and-deploying-the-bloomberg-bridge)
  - [Prerequisites](#prerequisites)
  - [Build the integration](#build-the-integration)
  - [Producing an appAsset for deployment by Finsemble](#produce-an-appasset-for-deployment-by-finsemble)
- [Using the Bloomberg Bridge Client](#using-the-bloomberg-bridge-client)
  - [Using the preloaded BloombergBridgeClient](#using-the-preloaded-bloombergbridgeclient)
  - [Instantiating the BloombergBridgeClient](#instantiating-the-bloombergbridgeclient)
  - [Data sharing with Launchpad groups](#data-sharing-with-launchpad-groups)
  - [Data sharing with worksheets](#data-sharing-with-worksheets)
  - [Searching for Securities](#searching-for-securities)
  - [Send commands to Bloomberg Terminal Panels](#send-commands-to-bloomberg-terminal-panels)

## How it works

The integration is comprised of a native (.Net) bridge application that acts as a Desktop service for communicating with the terminal via Terminal Connect and the Bloomberg Desktop API (DAPI). The Bridge application exposes an API via the Finsemble router for which a [Typescript client class](../src/clients/BloombergBridgeClient) is provided. The client can be imported into Finsemble Javascript components or custom desktop services that you build. The client may also be used as a preload, where it will be added into the FSBL Object as `FSBL.Clients.BloombergBridgeClient`.

Documentation for the Typescript client implementation can be found [here](modules/_bloombergbridgeclient_.md).

**Note:** The native bridge application must be running before calls to Terminal connect can be made, and can be launched either manually or on startup via a Finsemble component configuration.

![Bloomberg Bridge Architecture Diagram](media://BloombergArchDiagram.png)

A number of examples of using the integration are provided:

- **[Bloomberg Bridge](../src/components/Bloomberg%20Bridge)**: Example component configurations for launching the native bridge application.
- **[testBloomberg](../src/components/testBloomberg)**: A test component demonstrating use of all API functions.
- **[Bloomberg Terminal](../src/components/Bloomberg%20Terminal)**: An example configuration for launching the Bloomberg terminal itself
- **[Security finder example](../src/components/SecurityFinder)**: An example that demonstrates the use of the SecurityLookup function of the Bloomberg Bridge to implement a search with typeahead for Bloomberg securities, which may be used to set the context of launchpad groups, perform commands or add to worksheets. It can also receive context via the Finsemble Linker and help you resolve it to Bloomberg Security.
  Please note that this example can only be used with a local Bloomberg terminal as the security lookup function is provided by the Bloomberg Desktop API (DAPI) which is only supported locally. However, similar functionality can be implemented via the Bloomberg Server API (SAPI).

## Installation

This project contains:

- A .Net solution for building the Bloomberg Bridge
- a Typescript client to work with the API exposed by Bloomberg Bridge
- a number of javascript examples, such as a test component and an example service for integrating BBG LaunchPad groups with FDC3 channels.
- an optional Toolbar component that shows the status of the Bloomberg connection and a Preferences panel for the user to manage the settings
- a watch script that can install the project files in a copy of the Finsemble seed project for you.

Please note that the Bloomberg bridge must be deployed to your user's machines for use, see [Producing an appAsset for deployment by Finsemble](#produce-an-appasset-for-deployment-by-finsemble) for instructions on creating an asset and details of how Finsemble can deploy the asset for you.

In order to use the Javascript and Typescript examples, you can either copy the relevant files to your Finsemble seed project manually or use the supplied watch script to do so. Note the watch script will also deploy the example BloombergBridge build provided.

### Files

```
finsemble-bloomberg
|   .gitignore                   - gitignore file configured for both javascript and .Net projects
|   BloombergIntegration.sln     - Visual studio solution for building the .Net Bloomberg Bridge app
|   finsemble.config.json        - Watch script config and Finsemble examples config imports
|   finsemble.manifest.json      - Example Finsemble manifest entry for a deployed copy of the bridge app
|   LICENSE                      - Finsemble developer license
|   package.json                 - Package.json file for the client and examples
|   README.md                    - This file
|
|───BloombergBridge              - Project for building the .Net BloombergBridge
|   |   BloombergBridge.cs
|   |   BloombergBridge.csproj
|   |   SecurityLookup.cs
|   |
|   └───bin                         - build output directories
|       └───Release
|       └───Debug
|
|───docs                         - Generated documentation directory
|       README.md                    - This file
|
|───docs_src                     - Documentation source directory
|
|───fpe-scripts
|       watch.js                     - Watch script for copying project files into a Finsemble project
|
|───hosted
|       BloombergBridgeRelease.zip   - Example BloombergBridge build packaged for use as an appAsset
|
└───src                           - Typescript client and example Javascript component source directory
    |
    └───clients
    |   └───BloombergBridgeClient
    |           BloombergBridgeClient.ts  - Typescript client class for use with BloombergBridge
    |           BloombergBridgePreload.ts - Preload script that adds the client to `FSBL.Clients`
    |
    └───components
    |   └───bbgHelpers               - The Toolbar and User Preferences code
    |   └───Bloomberg Bridge         - Configs for launching the Bloomberg Bridge
    |   └───Bloomberg Terminal       - Example config for launching the Bloomberg terminal
    |   └───SecurityFinder           - Security lookup example, demonstrating a realistic use-case
    |   └───testBloomberg            - Test component demonstrating use of all API functions
    |
    └───services
        └───BloombergFDC3Service     - Coming soon!
```

### Installation via the watch script

When run, the watch script deploys all files to the configured Finsemble seed project directory and then watches for any changes in the _/src_ directory. When folders or files are added or removed this will be automatically reflected in the Finsemble Seed Project. _finsemble.config.json_ and finsemble.manifest.json are also observed for changes and will update the seed project's main _/configs/application/config.json_ and _/configs/application/manifest-local.json_ files if they change.

To use the watch script:

1. Clone the Finsemble [seed-project](https://github.com/ChartIQ/finsemble-seed) (if you don't already have a local version - see our [Getting Started Tutorial](https://www.chartiq.com/tutorials/?slug=finsemble))

2. Clone this repo

   - **our advice:** clone this repo to the same directory as the seed-project e.g _myfolder/finsemble-seed_ & _myfolder/finsemble-bloomberg_

3. If you clone in a different location, open [finsemble.config.json](../finsemble.config.json) and update `seedProjectDirectory` with the path to your local Finsemble Seed Project. If you intend to build an debug the \(.Net\) BloombergBridge, also set the value of the `$bloombergBridgeFolder` variable to point to the [BloombergBridge folder](../BloombergBridge) in this project \(see the [Finsemble config documentation](https://documentation.chartiq.com/finsemble/tutorial-Configuration.html#configuration-variables) for more details on setting variables\).


4. Run `npm install` then run `npm run watch` in the _finsemble-bloomberg_ project's directory \*_this will continue to watch for file changes and will copy across updated files as needed, this can be stopped once all the files have been copied to the seed project approx. 30 seconds_

5. To build and run the SecurityFinder example, you will also need to install dependencies in your seed project by running:

   ```
   npm install react-tabs react-select react-autosuggest
   ```

6. Your seed project directory has now been updated with the source files from the integration, run `npm run dev` in your Finsemble seed project's directory to build and run locally.

### Manual installation

To manually install the integration into your Finsemble project:

1. Ensure that you have access to a build of the 'Bloomberg Bridge' native component, see the [Building and Deploying the Bloomberg Bridge](#building-and-deploying-the-bloomberg-bridge) section for details.

2. Configure Finsemble to launch the Bloomberg Bridge. This can be achieved by either importing a config file by, for example, adding to the `importConfig` in your seed project's _/configs/application/config.json_ file:

   ```JSON
   ...
       ...
       "importConfig": [
           ...
           "$applicationRoot/components/Bloomberg Bridge/config.json"
           ...
       ]
   }
   ```

   or by adding it directly to the _/configs/application/components.json_ file. Example configurations are available at: _[src/components/Bloomberg Bridge/config.json](../src/components/Bloomberg%20Bridge/config.json)_.

   Note that the 'Bloomberg Bridge Debug' configuration makes use of a `$bloombergBridgeFolder` variable that should be set in your manifest. See the [Finsemble config documentation](https://documentation.chartiq.com/finsemble/tutorial-Configuration.html#configuration-variables) for more details on setting variables.

3. The example configuration supplied is for manual launch of the Bloomberg Bridge. You will likely wish to alter it to automatically launch the Bloomberg Bridge on start-up and to hide it from the launcher menu. To do so modify the example to set:

   - `components['Bloomberg Bridge'].component.spawnOnStartup = true`
   - `components['Bloomberg Bridge'].foreign.components['App Launcher'].launchableByUser = false`

4. Copy the _[src/clients/BloombergBridgeClient/BloombergBridgeClient.ts](../src/clients/BloombergBridgeClient/BloombergBridgeClient.ts)_ file into your project at an appropriate location (e.g. _/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts_). This can then be imported into components or services that you build. You may also wish to copy the preload file, _[src/clients/BloombergBridgeClient/BloombergBridgePreload.ts](../src/clients/BloombergBridgeClient/BloombergBridgePreload.ts)_, into your project if you intend to preload, rather than import the client into your applications.

5. If you wish to incorporate any of the supplied examples into your project, copy their folders from [src/components/](../src/components/) or [src/services/](../src/services/) into your project. Each contains:

   - a _config.json_ file that you should import or copy into your project.
   - a _finsemble.webpack.json_ file that works with the Finsemble seed project's build system. If you are using a different build, ensure that the component is built by it.

   **Note:** The examples include either an import statement for the BloombergBridgeClient, which need to be updated to use the path to the source file you copied into your project in step 4:

   ```javascript
   import { BloombergBridgeClient } from "../../clients/BloombergBridgeClient/BloombergBridgeClient";
   ```

   or use the preload script via their configuration:

   ```JSON
   "Bloomberg Security Finder": {
       "window": {
           ...
       },
       "component": {
           "preload": "$applicationRoot/clients/BloombergBridgeClient/BloombergBridgePreload.js",
           ...
   ```

6. To build and run the SecurityFinder example, you will also need to install dependencies in your project by running:
   ```
   npm install react-tabs react-select react-autosuggest
   ```
   
## Working with a Remote terminal

Some traders have multiple computers on their desks. Hence, Bloomberg Terminal Connect supports connecting to a Bloomberg terminal running on a different machine (a remote terminal) so that apps can still integrate with it without having to run on the same device. When working with a remote terminal Bloomberg Bridge should be run on the same machine as your Finsemble desktop and be launched by Finsemble as normal, however it will be configured to register with the remote machine by machine name or IP address.

### Remote connection requirements

For Bloomberg Terminal Connect to allow a connection to a remote terminal, a number of requirements must be satisfied:

- Both the local and remote machines must be connected to an Active Directory and be logged into the same user account,
- TCP Port 445 must be open on the remote machine
- UDP ports 137 & 139 must be on the remote machine.
  For further details on conneciton requirements please refer to the 'Bloomberg Terminal Connect API getting started guide'.

N.B. The `runSecurityLookup` is implemented via the Bloomberg Desktop API (DAPI), which only supports local connections. Hence, this function will return an error when run over a remote connections. Similar functionality maybe be implemented for remote connections via the Bloomberg Server API (SAPI).

### Configuring the terminal location

The Bloomberg Bridge application will monitor a number of configuration settings to determine whether it is making a remote connection and to what address. These are:

- `finsemble.custom.bloomberg.enabled` (default `true`): determines whether the Bloomberg Bridge should be connected to the Terminal. Set to `false` before updating other settings at runtime, then set to `true` to connect to a new host.
- `finsemble.custom.bloomberg.remote` (default `false`): Enables connection to a remote machine. If false the value of `remoteAddress` will be assumed to be `"localhost"`.
- `finsemble.custom.bloomberg.remoteAddress` (default `null`): The machine name or IP address of the remote machine with a terminal running on it.
- `finsemble.custom.bloomberg.showStatus` (default `false`): Determines whether the Bloomberg connection status icon should be displayed in the toolbar or not.

These config values may be set via dynamic configuration, on behalf of the user. To do so, import configuration of the following format via `ConfigClient.processAndSet`:

```JSON
{
    "finsemble": {
        "custom": {
            "bloomberg": {
                "enabled": true,
                "remote": true,
                "remoteAddress": "mymachine.somedomain.com",
                "showStatus": true
            }
        }
    }
}
```

An example configuration is also provided in _[finsemble.manifest.json](../finsemble.manifest.json)_.

Alternatively, a preference panel is provided allowing configuration of these values by the user at runtime with persistence across sessions. This can be used in conjunction with default settings or settings applied via dynamic configuration, which can then be overridden by user preferences. 

### Connection status and config UIs

The Bloomberg status button in the toolbar, when clicked, will open the Finsemble Preferences to the Bloomberg section. The status will show either green (connected successfullly), orange (disconnected, but bridge is running), or red (error, bridge is likely not running).

![Bloomberg Status](./media/bbg_status.png)

Open the Preferences Panel via the Toolbar > File Menu > Preferences, or by clicking on the Bloomberg status button in the toolbar. In there you will see:

![Bloomberg Preferences](./media/bbg_prefs.png)

- The _"Show Status in Toolbar"_ toggle, defaults to true if not set in the manifest, controls whether or not the Bloomberg Status shows in the Toolbar or not.
- The _"Enabled"_ toggle controls whether or not the Bloomberg Bridge is connected to a running Bloomberg Terminal Connect instance.
- The _"Connection Type"_ radio buttons allow switching between:
  - _Local:_ the Bloomberg Terminal Connect is running on the same machine as Finsemble.
  - _Remote:_ the Bloomberg Terminal Connect instance is running on another machine, in which case that machine's IP address is entered in the _Address_ field.

These will reflect changes pushed via the manifest for Finsemble, or if not in the manifest, will just use the defaults.

**NOTE:** when the connection is _Enabled_, the Connection Type will be disabled. If you need to make changes via the UI, you should first untick the _Enabled_ flag so the connection disconnects first.

#### Installing the preference panel

- in your project, in _src/components/userPreferences/UserPreferences.tsx_ modify the following:
  - In the [imports](./media/bbg_prefs_include.png), add:
  ```TypeScript
      import { BloombergPreferences } from "../bbgHelpers/BloombergPreferences";
  ```
  - in the _sections_ const, add [this line](./media/bbg_prefs_sections_line.png) (and a comma on the previous line):
  ```TypeScript
      "Bloomberg Terminal Connect": BloombergPreferences
  ```

#### Installing the connection status icon

- in your project, in _src/components/toolbar/src/Toolbar.tsx_ modify the following:
  - In the imports, add:
  ```TypeScript
      import { BloombergStatus } from "../../bbgHelpers/BloombergStatus";
  ```
  - In the _Toolbar_ [return](./media/bbg_status_toolbar.png), insert:
  ```TypeScript
      <BloombergStatus />
  ```

## Building and Deploying the Bloomberg Bridge

The Bloomberg Bridge application should be built using Terminal Connect and BLP API DLL files distributed by Bloomberg. It can then either be deployed to a known path on your users machines, or delivered via a Finsemble app asset, which will be downloaded and installed automatically by Finsemble.

An example appAsset for the Bridge is provided in _/hosted_ directory.

### Prerequisites

- Visual Studio 2017 (or later) and .Net Framework 4.5.2+
- Finsemble.dll: Installed automatically via NuGet when using Visual Studio to build the integration.
- Bloomberglp.TerminalApiEx.dll
  - To download from Bloomberg Terminal, run `TMCT` **\<GO\>**
  - Click Software Downloads
  - Follow instructions to install, default location: _C:\blp\TerminalAPISDK_
  - Add either the 32bit or 64bit DLL in the _lib_ or _lib.x64_ to your project as a reference.
- Bloomberglp.Blpapi.dll
  - Download the BLP API C# (.NET) Supported Release for Windows from the [Bloomberg API library](https://www.bloomberg.com/professional/support/api-library/) page.
  - Add the _/bin/Bloomberglp.Blpapi.dll_ DLL as a reference to your integration's project.

### Build the integration

- Open the BloombergIntegration.sln in Visual Studio.
- Add the previously downloaded DLLs as references to the Bloomberg Bridge project, specifically:
  - Terminal Connect API (Bloomberglp.TerminalApiEx.dll)
  - BLP API (Bloomberglp.Blpapi.dll)
- Rebuild the project (which will install NuGet dependencies automatically)

### Running the BloombergBridge from a local path

For debugging purposes a configuration referencing a local Bloomberg Bridge build is provided, see the 'Bloomberg Bridge Debug' configuration in _[src/components/Bloomberg%20Bridge/config.json](../src/components/Bloomberg%20Bridge/config.json)_. This configuration makes use of a `$bloombergBridgeFolder` variable in the `path` element that should be set in your manifest file.

### Produce an appAsset for deployment by Finsemble

- Run a Release build
- Create a zip file from the contents of the _BloombergBridge/bin/Release_ directory and name it appropriately
- Host the build at an appropriate URL (or if using the watch script and testing locally, add it to the _hosted_ directory of the project)
- Configure Finsemble's manifest file (e.g. _/configs/application/manifest-local.json_) with an appropriate appAssets configuration of the form:

  ```JSON
  {
      ...
      "appAssets": [
          ...
          {
              "src": "http://localhost:3375/hosted/BloombergBridgeRelease.zip",
              "version": "1.2.0",
              "alias": "bloomberg_bridge",
              "target": "BloombergBridge.exe"
          }
      ],
      ...
  }
  ```

  N.B. Finsemble will only download and deploy a new version of the asset if it does not have a copy of the asset that was downloaded via an app asset with the given version number. Hence, if you need to replace the version installed on the user's machine ensure that you change the value of the `version` field.

## Using the Bloomberg Bridge Client

The native bridge application \(BloombergBridge\) exposes an API over the Finsemble router that provides access to each of its functions. For convenience, a Typescript client is supplied allowing you to call each of these functions. The client may be imported (or required) into your source files, e.g.:

```Javascript
import {BloombergBridgeClient} from "../../clients/BloombergBridgeClient/BloombergBridgeClient";
```

(ensuring that you correct the path to the file as it exists in your project).

Alternatively, a preload script is provided that will import the client and set up an instance at `FSBL.Clients.BloombergBridgeClient`. This can be used by building the Typescript file and then setting the `component.preload` field of your component's configuration to the path to the compiled preload script. If you are using Finsemble seed project's build system, the Typescript file will be be automatically built for you and output to the _/dist/clients/BloombergBridgeClient_ folder. If you use a different build system, be sure to build the Typescript file before trying to preload it.
Please ensure that either the full path to the preload is added to the `trustedPreloads` array in your manifest file or `window.options.securityPolicy = 'trusted'` is set in your component config or Finsemble may prevent the preload if your component is loaded from a different domain to the one used to host Finsemble, see Finsemble's [security policies documentation](https://documentation.chartiq.com/finsemble/tutorial-SecurityPolicies.html) for more details.

Documentation for:

- the BloombergBridgeClient module can be found [here](modules/_bloombergbridgeclient_.md).
- the BloombergBridgeClient class and its functions can be found [here](classes/_bloombergbridgeclient_.bloombergbridgeclient.md)

### Examples

#### Using the preloaded BloombergBridgeClient

If you use the BloombergBridgeClient preload in your component, it will be instantiated for you at:

```Javascript
FSBL.Clients.BloombergBridgeClient
```

For the purposes of the following examples you can create a reference to it as follows:

```Javascript
let bbg = FSBL.Clients.BloombergBridgeClient;
```

As setup of the client occurs when the Finsemble clients are themselves ready (on the FSBLReady event), the preload will dispatch its own event when the BloombergBridgeClient is ready: `BloombergBridgeClientReady`. To avoid race conditions, you should wait on this event, rather than `FSBLReady`, in your components, e.g.:

```Javascript
window.addEventListener("BloombergBridgeClientReady", BBGReady);

function BBGReady() {
    //Do your setup, which can assume both FSBL and the BloombergBridgeClient are ready e.g.:
    ReactDOM.render(
        <SecurityFinder />,
        document.getElementById('root')
    );
}
```

#### Instantiating the BloombergBridgeClient

If you are not using the BloombergBridgeClient as a preload in a component (when it will instantiate itself using the RouterClient and Logger instance already preloaded into the window) you must instantiate by passing a Finsemble RouterClient and Logger, e.g. in a componet:

```Javascript
let bbg = new BloombergBridgeClient(FSBL.Clients.RouterClient, FSBL.Clients.Logger);
```

#### Check if we are connected to the BloombergBridgeClient

You can either check the connection manually:

```Javascript
let checkConnectionHandler = (err, loggedIn) => {
    if (!err && loggedIn) {
         showConnectedIcon();
    } else {
        showDisconnectedIcon();
    }
};
bbg.checkConnection(checkConnectionHandler);
```

or register a handler for connection events:

```Javascript
let connectionEventHandler = (err, resp) => {
    if (!err && resp && resp.loggedIn) {
        showConnectedIcon();
    } else {
        showDisconnectedIcon();
    }
};
bbg.setConnectionEventListener(connectionEventHandler);
```

#### Data sharing with Launchpad groups

Retrieve a list of all current Launchpad groups and their current context:

```Javascript
bbg.runGetAllGroups((err, response) => {
    if (!err) {
        if (response && response.groups && Array.isArray(response.groups)) {
            //do something with the returned data
            response.groups.forEach(group => {
                let groupName = group.name;
                let groupType = group.type;
                let groupCurrentValue = group.value;
                ...
            });
        } else {
            console.error("Invalid response returned from runGetAllGroups", response);
        }
    } else {
        console.error("Error returned from runGetAllGroups", err);
    }
});
```

Get the current state of a Launchpad group:

```Javascript
bbg.runGetGroupContext(groupName, (err, response) => {
    if (!err) {
        if (response && response.group) {
            let groupName = response.group.name;
            let groupType = group.type;
            let groupCurrentValue = group.value;
            ...
        } else {
            console.error("Invalid response returned from runGetGroupContext", response);
        }
    } else {
        console.error("Error returned from runGetGroupContext", err);
    }
});
```

Set the state (value) of a Launchpad group:

```Javascript
bbg.runSetGroupContext(groupName, newValue, null, (err, response) => {
    if (!err) {
        /* You may wish to retrieve the current state of Launchpad group here as Bloomberg
           will resolve any security your set and may therefore its value may differ from
           what you sent. */
        bbg.runGetGroupContext(groupName, (err2, response2) => { ... });
    } else {
        console.error("Error returned from runSetGroupContext", err);
    }
});
```

\*_Note: an optional third parameter allows you to specify a cookie identifying a specific component within the group to update, instead of the whole group._

Register a listener for group events (e.g. creation or context change):

```Javascript
bbg.setGroupEventListener((err, response) => {
    if (!err) {
        if (response.data.group && response.data.group.type == "monitor") {
            console.log("Monitor event:\n" + JSON.stringify(response.data, null, 2));
        } else {
            console.log("Security event:\n" + JSON.stringify(response.data, null, 2));
        }
    } else {
        console.error("Error returned from setGroupEventListener", err);
    }
});
```

#### Data sharing with worksheets

Retrieve all worksheets:

```Javascript
bbg.runGetAllWorksheets((err, response) => {
    if (!err) {
        if (response && response.worksheets && Array.isArray(response.worksheets)) {
            response.worksheets.forEach(worksheet => {
                let worksheetName = worksheet.name;
                let worksheetId = worksheet.id;
                ...
            });
        } else {
            console.error("invalid response from runGetAllWorksheets", response);
        }
    } else{
        console.error("Error returned from runGetAllWorksheets", err);
    }
});
```

Retrieve the content of a particular worksheet by Id:

```Javascript
bbg.runGetWorksheet(worksheetId, (err, response) => {
    if (!err) {
        if (response && response.worksheet && Array.isArray(response.worksheet.securities)) {
            let worksheetName = response.worksheet.name;
            let worksheetId = response.worksheet.id;
            let workSheetSecurities = response.worksheet.securities;
            ...
        } else {
            console.error("invalid response from runGetWorksheet");
        }
    } else {
        console.error("Error returned from runGetWorksheet", err);
    }
});
```

Create a worksheet:

```Javascript
let securities = ["TSLA US Equity", "AMZN US Equity"];
bbg.runCreateWorksheet(worksheetName, securities, (err, response) => {
    if (!err) {
        if (response && response.worksheet) {
            //Id assigned to the worksheet
            let worksheetId = response.worksheet.id;
            //List of securities resolved by Bloomberg from the input list, unresolvable securities will be removed
            let workSheetSecurities = response.worksheet.securities;
            ...
        } else {
            console.error("invalid response from runCreateWorksheet", response);
        }
    } else {
        console.error("Error returned from runCreateWorksheet", err);
    }
});
```

Replace the content of a worksheet:

```Javascript
let securities = ["TSLA US Equity", "AMZN US Equity"];
bbg.runReplaceWorksheet(worksheetId, securities, (err, response) => {
    if (!err) {
        if (response && response.worksheet) {
            //Details of the updated worksheet will be returned
            let worksheetName = response.worksheet.name;
            //List of securities resolved by Bloomberg from the input list, unresolvable securities will be removed
            let workSheetSecurities = response.worksheet.securities;
            ...
        } else {
            console.error("invalid response from runReplaceWorksheet", response);
        }
    } else {
        console.error("Error returned from runReplaceWorksheet", err);
    }
});
```

#### Searching for Securities

You can search for Bloomberg securities via the Bloomberg Bridge and BLP API, which will return results in around ~120-150ms and maybe used, for example, to power an autocomplete or typeahead search:

```Javascript
bbg.runSecurityLookup(security, (err, response) => {
    if (!err) {
        if (response && response.results) {
            //do something with the results
            response.results.forEach(result => {
                console.log(result.name + " " + result.type);
                ...
            });
        } else {
            console.error("invalid response from runSecurityLookup", response);
        }
    } else {
        console.error("Error returned from runSecurityLookup", err);
    }
});
```

#### Send commands to Bloomberg Terminal panels

You can send commands to the 4 Bloomberg panels. Each Bloomberg command is made up of:

- Mnemonic: The specific command that would normally be entered into the Terminal (e.g., DES, YAS, VCON)
- Panel: The Bloomberg panel number to send it to
- Securities: An array of 0 or more Bloomberg security strings
- Tail: Arguments specific to the mnemonic (optional) - See the help page in the Terminal for each mnemonic to discover what options it supports

At a minimum a command must include a Mnemonic and Panel number.

To run a command:

```Javascript
let mnemonic = "DES";
let securities = ["MSFT US Equity"];
let panel = 3;
let tails = null;
bbg.runBBGCommand(mnemonic, securities, panel, tails, (err, response) => {
    if (!err) {
        console.log(`Ran command "${mnemonic}" on panel ${panel}`);
    } else {
        console.error("Error returned from runBBGCommand", err);
    }
});
```

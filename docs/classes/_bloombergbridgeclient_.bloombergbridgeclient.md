[finsemble-bloomberg](../README.md) › [Globals](../globals.md) › ["BloombergBridgeClient"](../modules/_bloombergbridgeclient_.md) › [BloombergBridgeClient](_bloombergbridgeclient_.bloombergbridgeclient.md)

# Class: BloombergBridgeClient

Client class for communicating with the Finsemble Bloomberg Bridge over the the Finsemble Router,
which in turn communicates with the Bloomberg Terminal via the Terminal Connect and BLP APIs.

This Class may either be imported into code and initialized by passing in an instance
of the Finsemble RouterClient and Logger (e.g. in Finsemble Custom Desktop Service) or
used as a preload to be applied to a component, where it will be automatically initialized
via instances of the RouterClient and Logger referenced from `FSBL.Clients`.

## Hierarchy

* **BloombergBridgeClient**

## Index

### Constructors

* [constructor](_bloombergbridgeclient_.bloombergbridgeclient.md#constructor)

### Properties

* [connectionEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#private-connectioneventlistener)
* [groupEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#private-groupeventlistener)
* [logger](_bloombergbridgeclient_.bloombergbridgeclient.md#private-logger)
* [routerClient](_bloombergbridgeclient_.bloombergbridgeclient.md#private-routerclient)

### Methods

* [apiResponseHandler](_bloombergbridgeclient_.bloombergbridgeclient.md#private-apiresponsehandler)
* [checkConnection](_bloombergbridgeclient_.bloombergbridgeclient.md#checkconnection)
* [queryBloombergBridge](_bloombergbridgeclient_.bloombergbridgeclient.md#private-querybloombergbridge)
* [removeConnectionEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#removeconnectioneventlistener)
* [removeGroupEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#removegroupeventlistener)
* [runBBGCommand](_bloombergbridgeclient_.bloombergbridgeclient.md#runbbgcommand)
* [runCreateWorksheet](_bloombergbridgeclient_.bloombergbridgeclient.md#runcreateworksheet)
* [runGetAllGroups](_bloombergbridgeclient_.bloombergbridgeclient.md#rungetallgroups)
* [runGetAllWorksheets](_bloombergbridgeclient_.bloombergbridgeclient.md#rungetallworksheets)
* [runGetGroupContext](_bloombergbridgeclient_.bloombergbridgeclient.md#rungetgroupcontext)
* [runGetWorksheet](_bloombergbridgeclient_.bloombergbridgeclient.md#rungetworksheet)
* [runReplaceWorksheet](_bloombergbridgeclient_.bloombergbridgeclient.md#runreplaceworksheet)
* [runSecurityLookup](_bloombergbridgeclient_.bloombergbridgeclient.md#runsecuritylookup)
* [runSetGroupContext](_bloombergbridgeclient_.bloombergbridgeclient.md#runsetgroupcontext)
* [setConnectionEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#setconnectioneventlistener)
* [setEnabled](_bloombergbridgeclient_.bloombergbridgeclient.md#setenabled)
* [setGroupEventListener](_bloombergbridgeclient_.bloombergbridgeclient.md#setgroupeventlistener)

## Constructors

###  constructor

\+ **new BloombergBridgeClient**(`routerClient?`: IRouterClient, `logger?`: ILogger): *[BloombergBridgeClient](_bloombergbridgeclient_.bloombergbridgeclient.md)*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:88](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L88)*

BloombergBridgeClient constructor.

**`example`** Instantiating the client in a Finsemble component:
```Javascript
let bbg = new BloombergBridgeClient(FSBL.Clients.RouterClient, FSBL.Clients.Logger);
```

Instantiating the client in a Finsemble service:
```Javascript
const Finsemble = require("@chartiq/finsemble");
Finsemble.Clients.Logger.start();
Finsemble.Clients.Logger.log("test Service starting up");
let bbg = new BloombergBridgeClient(Finsemble.Clients.RouterClient, Finsemble.Clients.Logger);
```

**Parameters:**

Name | Type | Description |
------ | ------ | ------ |
`routerClient?` | IRouterClient | An instance of the Finsemble router client to be used for all = communication. If not passed it will be retrieved from FSBL.Clients.RouterClient or an exception. |
`logger?` | ILogger | An instance of the Finsemble Logger to be used log messages. If not passed it will be retrieved from FSBL.Clients.Logger or an exception. |

**Returns:** *[BloombergBridgeClient](_bloombergbridgeclient_.bloombergbridgeclient.md)*

## Properties

### `Private` connectionEventListener

• **connectionEventListener**: *[BBGConnectionEventListener](../interfaces/_bloombergbridgeclient_.bbgconnectioneventlistener.md) | null* = null

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:85](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L85)*

___

### `Private` groupEventListener

• **groupEventListener**: *[BBGGroupEventListener](../interfaces/_bloombergbridgeclient_.bbggroupeventlistener.md) | null* = null

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:86](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L86)*

___

### `Private` logger

• **logger**: *ILogger | null* = null

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:88](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L88)*

___

### `Private` routerClient

• **routerClient**: *IRouterClient | null* = null

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:87](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L87)*

## Methods

### `Private` apiResponseHandler

▸ **apiResponseHandler**(`cb`: StandardCallback): *(Anonymous function)*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:342](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L342)*

Internal function used to return a call back that will wrap the supplied callback and log all
responses
from the Bloomberg Bridge to aid debugging.

**Parameters:**

Name | Type | Description |
------ | ------ | ------ |
`cb` | StandardCallback | Callback |

**Returns:** *(Anonymous function)*

___

###  checkConnection

▸ **checkConnection**(`cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:290](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L290)*

Check that Bloomberg bridge is connected to the Bloomberg Terminal.

**`example`** 
```Javascript
let checkConnectionHandler = (err, registered) => {
    if (!err && registered) {
        showConnectedIcon();
    } else {
        showDisconnectedIcon();
    }
};
bbg.checkConnection(checkConnectionHandler);
```

**Parameters:**

▪ **cb**: *function*

Callback for connection response that will return response as true if we are
connected.

▸ (`err`: string | CallbackError | Error | null, `response`: boolean | null): *void*

**Parameters:**

Name | Type |
------ | ------ |
`err` | string &#124; CallbackError &#124; Error &#124; null |
`response` | boolean &#124; null |

**Returns:** *void*

___

### `Private` queryBloombergBridge

▸ **queryBloombergBridge**(`message`: object, `cb`: StandardCallback): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:325](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L325)*

Internal function used to send a Query to the BBG_run_terminal_function responder of
BloombergBridge,
which implements the majority functions for the BloombergBridgeClient.

**Parameters:**

▪ **message**: *object*

The query data to pass.

Name | Type | Description |
------ | ------ | ------ |
`function` | string | Required field that determines which function to run. |

▪ **cb**: *StandardCallback*

Callback

**Returns:** *void*

___

###  removeConnectionEventListener

▸ **removeConnectionEventListener**(): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:170](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L170)*

Remove the current connection event handler.

**`example`** 
```Javascript
bbg.removeConnectionEventListener();
```

**Returns:** *void*

___

###  removeGroupEventListener

▸ **removeGroupEventListener**(): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:225](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L225)*

Remove the current group context changed event handler.

**`example`** 
```Javascript
bbg.removeGroupEventListener();
```

**Returns:** *void*

___

###  runBBGCommand

▸ **runBBGCommand**(`mnemonic`: string, `securities`: string[], `panel`: string, `tails`: string, `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:388](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L388)*

Run a function in one of the 4 Bloomberg panel windows.

**`example`** 
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

**Parameters:**

▪ **mnemonic**: *string*

The mnemonic of the Bloomberg command to run on a panel

▪ **securities**: *string[]*

(optional) An array of strings representing one or more securities
to pass to the function.

▪ **panel**: *string*

Panel number to run the command on (accepts values "1", "2", "3" or
"4")

▪ **tails**: *string*

(optional) parameters passed to the function

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |

**Returns:** *void*

___

###  runCreateWorksheet

▸ **runCreateWorksheet**(`worksheetName`: string, `securities`: string[], `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:430](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L430)*

Create a new worksheet with the specified securities and name.

**`example`** 
```Javascript
let securities = ["TSLA US Equity", "AMZN US Equity"];
bbg.runCreateWorksheet(worksheetName, securities, (err, response) => {
    if (!err) {
        if (response && response.worksheet) {
            //Id assigned to the worksheet
            let worksheetId = response.worksheet.id;
            //List of securities resolved by Bloomberg from the input list, unresolvable securities will be removed
            let workSheetSecurities = response.worksheet.securities;
        } else {
            console.error("invalid response from runCreateWorksheet", response);
        }
    } else {
        console.error("Error returned from runCreateWorksheet", err);
    }
});
```

**Parameters:**

▪ **worksheetName**: *string*

Name for the worksheet.

▪ **securities**: *string[]*

An array of strings representing one or more securities.

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |
`worksheet` | [BBGWorksheet](../interfaces/_bloombergbridgeclient_.bbgworksheet.md) |

**Returns:** *void*

___

###  runGetAllGroups

▸ **runGetAllGroups**(`cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:576](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L576)*

Gets a list of all available Launchpad component groups.

**`example`** 
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

**Parameters:**

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`groups` | [BBGGroup](../interfaces/_bloombergbridgeclient_.bbggroup.md)[] |
`status` | boolean |

**Returns:** *void*

___

###  runGetAllWorksheets

▸ **runGetAllWorksheets**(`cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:467](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L467)*

Retrieve all worksheets for the user.

**`example`** 
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
    } else {
        console.error("Error returned from runGetAllWorksheets", err);
    }
});
```

**Parameters:**

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |
`worksheets` | [BBGWorksheet](../interfaces/_bloombergbridgeclient_.bbgworksheet.md)[] |

**Returns:** *void*

___

###  runGetGroupContext

▸ **runGetGroupContext**(`groupName`: string, `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:608](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L608)*

Returns details of a Launchpad component group by name.

**`example`** 
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

**Parameters:**

▪ **groupName**: *string*

The name of the component group to retrieve.

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`group` | [BBGGroup](../interfaces/_bloombergbridgeclient_.bbggroup.md) |
`status` | boolean |

**Returns:** *void*

___

###  runGetWorksheet

▸ **runGetWorksheet**(`worksheetId`: string, `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:502](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L502)*

Retrieve a specific worksheet by id.

**`example`** 
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

**Parameters:**

▪ **worksheetId**: *string*

Worksheet ID to retrieve.

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |
`worksheet` | [BBGWorksheet](../interfaces/_bloombergbridgeclient_.bbgworksheet.md) |

**Returns:** *void*

___

###  runReplaceWorksheet

▸ **runReplaceWorksheet**(`worksheetId`: string, `securities`: string[], `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:538](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L538)*

Replaces a specific worksheet by ID with a new list of securities.

**Parameters:**

▪ **worksheetId**: *string*

Worksheet ID to replace.

▪ **securities**: *string[]*

An array of strings representing one or more securities.

▪ **cb**: *function*

Callback
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

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |
`worksheet` | [BBGWorksheet](../interfaces/_bloombergbridgeclient_.bbgworksheet.md) |

**Returns:** *void*

___

###  runSecurityLookup

▸ **runSecurityLookup**(`security`: string, `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:689](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L689)*

Search for Bloomberg securities via the Bloomberg Bridge and DAPI, which will return
results in around ~120-150ms and maybe used, for example, to power an autocomplete or
typeahead search.
Note that because this functionality is provided by the Bloomberg DAPI, it is only
supported when connecting to a Bloomberg Terminal running on the same machine as the
Bloomberg Bridge and Finsemble.

**`example`** 
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

**Parameters:**

▪ **security**: *string*

The string to lookup a security for

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`results` | [] |
`status` | boolean |

**Returns:** *void*

___

###  runSetGroupContext

▸ **runSetGroupContext**(`groupName`: string, `value`: string, `cookie`: string | null, `cb`: function): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:642](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L642)*

Set the context value of a Launchpad group by name.

**`example`** 
```Javascript
bbg.runSetGroupContext(groupName, newValue, null, (err, response) => {
    if (!err) {
	       // You may wish to retrieve the current state of Launchpad group here as Bloomberg
        // will resolve any security your set and may therefore its value may differ from
        // what you sent.
        bbg.runGetGroupContext(groupName, (err2, response2) => { ... });
    } else {
        console.error("Error returned from runSetGroupContext", err);
    }
});
```

**Parameters:**

▪ **groupName**: *string*

The name of the component group to set the value of.

▪ **value**: *string*

The value to set for hte group, this will usually be a string
representing a security.

▪ **cookie**: *string | null*

(optional) Cookie value identifying a particular component within
a group to set the context of. Pass null if not required.

▪ **cb**: *function*

Callback

▸ (`err`: StandardError, `response`: object): *void*

**Parameters:**

▪ **err**: *StandardError*

▪ **response**: *object*

Name | Type |
------ | ------ |
`status` | boolean |

**Returns:** *void*

___

###  setConnectionEventListener

▸ **setConnectionEventListener**(`cb`: [BBGConnectionEventListener](../interfaces/_bloombergbridgeclient_.bbgconnectioneventlistener.md)): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:146](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L146)*

Set a handler function for connection events.

Note that only one handler function is permitted, hence calling
this multiple times will simply replace the existing handler.

**`example`** 
```Javascript
let connectionEventHandler = (err, resp) => {
    if (!err && resp && resp.registered) {
        showConnectedIcon();
    } else {
        showDisconnectedIcon();
    }
};
bbg.setConnectionEventListener(connectionEventHandler);
```

**Parameters:**

Name | Type | Description |
------ | ------ | ------ |
`cb` | [BBGConnectionEventListener](../interfaces/_bloombergbridgeclient_.bbgconnectioneventlistener.md) | Callback |

**Returns:** *void*

___

###  setEnabled

▸ **setEnabled**(`enabled`: boolean, `cb?`: StandardCallback): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:248](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L248)*

Set the connection state for Bloomberg bridge, i.e. whether it is enabled or not.
Note that the remote connection config should only be changed while the connection is
disabled or disconnected, as it is read when attempting to connect.

**`example`** 
```Javascript
bbg.setConnectState(true);
bbg.setConnectState(false, (err, resp) => { ... });
```

**Parameters:**

Name | Type | Description |
------ | ------ | ------ |
`enabled` | boolean | Boolean flag indicating whether the bridge should connect to Bloomberg. If set false while connected, the bridge will automatically disconnect. |
`cb?` | StandardCallback | Optional callback that will return response as true if we have successfully set the connect states. |

**Returns:** *void*

___

###  setGroupEventListener

▸ **setGroupEventListener**(`cb`: [BBGGroupEventListener](../interfaces/_bloombergbridgeclient_.bbggroupeventlistener.md)): *void*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:202](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L202)*

Set a handler function for Launchpad group context changed events, which
are fired when a group's context changes or a new group is created.

Note that only one handler function is permitted, hence calling
this multiple times will simply replace the existing handler.

**`example`** 
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

**Parameters:**

Name | Type | Description |
------ | ------ | ------ |
`cb` | [BBGGroupEventListener](../interfaces/_bloombergbridgeclient_.bbggroupeventlistener.md) | Handler function to call on group context change events |

**Returns:** *void*

[finsemble-bloomberg](../README.md) › [Globals](../globals.md) › ["BloombergBridgeClient"](_bloombergbridgeclient_.md)

# Module: "BloombergBridgeClient"

## Index

### Classes

* [BloombergBridgeClient](../classes/_bloombergbridgeclient_.bloombergbridgeclient.md)

### Interfaces

* [BBGConnectionEventListener](../interfaces/_bloombergbridgeclient_.bbgconnectioneventlistener.md)
* [BBGGroup](../interfaces/_bloombergbridgeclient_.bbggroup.md)
* [BBGGroupEventListener](../interfaces/_bloombergbridgeclient_.bbggroupeventlistener.md)
* [BBGWorksheet](../interfaces/_bloombergbridgeclient_.bbgworksheet.md)

### Variables

* [CONNECTION_CHECK_TIMEOUT](_bloombergbridgeclient_.md#const-connection_check_timeout)
* [SET_CONNECT_STATE_TIMEOUT](_bloombergbridgeclient_.md#const-set_connect_state_timeout)

## Variables

### `Const` CONNECTION_CHECK_TIMEOUT

• **CONNECTION_CHECK_TIMEOUT**: *1500* = 1500

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:11](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L11)*

Timeout in milliseconds for terminal connection checks that will cause a checkConnection
call to fail.

___

### `Const` SET_CONNECT_STATE_TIMEOUT

• **SET_CONNECT_STATE_TIMEOUT**: *2500* = 2500

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:13](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L13)*

Timeout in milliseconds for setting the connect state.

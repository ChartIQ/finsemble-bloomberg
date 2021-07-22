[finsemble-bloomberg](../README.md) › [Globals](../globals.md) › ["BloombergBridgeClient"](../modules/_bloombergbridgeclient_.md) › [BBGWorksheet](_bloombergbridgeclient_.bbgworksheet.md)

# Interface: BBGWorksheet

Interface representing a Bloomberg worksheet.

## Hierarchy

* **BBGWorksheet**

## Index

### Properties

* [id](_bloombergbridgeclient_.bbgworksheet.md#id)
* [isActive](_bloombergbridgeclient_.bbgworksheet.md#isactive)
* [name](_bloombergbridgeclient_.bbgworksheet.md#name)
* [securities](_bloombergbridgeclient_.bbgworksheet.md#securities)

## Properties

###  id

• **id**: *string*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:54](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L54)*

The name of the worksheet (non-unique).

___

###  isActive

• **isActive**: *boolean*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:58](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L58)*

A flag indicating the Worksheet's IsActive status.

___

###  name

• **name**: *string*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:56](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L56)*

The name of the worksheet assigned by the Bloomberg terminal and globally unique.

___

###  securities

• **securities**: *string[]*

*Defined in [src/clients/BloombergBridgeClient/BloombergBridgeClient.ts:60](https://github.com/ChartIQ/finsemble-bloomberg/blob/16c82f3/src/clients/BloombergBridgeClient/BloombergBridgeClient.ts#L60)*

The list of securities appearing in the worksheet.

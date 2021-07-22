import React, { useEffect, useState } from "react";
import {BloombergBridgeClient} from "../../clients/BloombergBridgeClient/BloombergBridgeClient";

// the BloombergBridgeClient that will be used for all messaging to/from Bloomberg
let bbg = new BloombergBridgeClient(FSBL.Clients.RouterClient, FSBL.Clients.Logger);

// TODO: could refactor this (and the toolbar status code) to be JSX Elements instead of createElement

export const BloombergPreferences = () => {
    const [bbgRemoteAddress, setBbgRemoteAddress] = useState("");
    const [isRemote, setIsRemote] = useState(false);
    const [isEnabled, setIsEnabled] = useState(false);
    const [isConnected, setIsConnected] = useState(false);
    const [connectionStatus, setConnectionStatus] = useState("Disconnected");
    const [indicatorColor, setIndicatorColor] = useState("red");
    const [showBloomberg, setShowBloomberg] = useState(false);

    useEffect(() => {
        function checkConnection() {
            bbg.checkConnection((err, resp) => {
                if (!err && resp === true) {
                    setIsConnected(true);
                    setConnectionStatus("Connected");
                    setIndicatorColor("green");
                } else if (err) {
                    FSBL.Clients.Logger.error("Error received when checking connection", err);
                    setIsConnected(false);
                    setConnectionStatus("Confirm Bloomberg and Bridge are both running.");
                    setIndicatorColor("red");
                } else {
                    FSBL.Clients.Logger.debug("Negative response when checking connection: ", resp);
                    setIsConnected(false);
                    setConnectionStatus("Disconnected");
                    setIndicatorColor("orange");
                }
            });
        };

        try {
            //do the initial check
            checkConnection();
            //listen for connection events (listen/transmit)
            bbg.setConnectionEventListener(checkConnection);
            // its also possible to poll for connection status,
            //  worth doing in case the bridge process is killed off and doesn't get a chance to send an update
            setInterval(checkConnection, 30000);
        } catch (e) {
            FSBL.Clients.Logger.error(`error in bbg prefs: ${e}`);
        }

        let statusHandler = (err, status) => {
            if (err) {
                FSBL.Clients.Logger.error("Error received when checking bloomberg bridge config", err);
            } else {
                let bbgStatus = (typeof status.value == "undefined" || status.value) ? true : false;
                setShowBloomberg(bbgStatus);
            }
        };
        FSBL.Clients.ConfigClient.getValue({ field: "finsemble.custom.bloomberg.showStatus" }, statusHandler);
        FSBL.Clients.ConfigClient.addListener({ field: "finsemble.custom.bloomberg.showStatus" }, statusHandler);

        let remoteAddressHandler = (err, address) => {
            if (err) {
                FSBL.Clients.Logger.error("Error received when checking bloomberg bridge config", err);
            } else {
                let remoteAddress = typeof address.value == "undefined" ? address : address.value;
                setBbgRemoteAddress(remoteAddress);
            }
        };
        FSBL.Clients.ConfigClient.getValue({ field: "finsemble.custom.bloomberg.remoteAddress" }, remoteAddressHandler);
        FSBL.Clients.ConfigClient.addListener({ field: "finsemble.custom.bloomberg.remoteAddress" }, remoteAddressHandler);

        let remoteHandler = (err, remote) => {
            if (err) {
                FSBL.Clients.Logger.error("Error received when checking bloomberg bridge config", err);
            } else {
                let bbgRemote = typeof remote.value == "undefined" ? remote : remote.value;
                setIsRemote(bbgRemote);
            }
        };
        FSBL.Clients.ConfigClient.getValue({ field: "finsemble.custom.bloomberg.remote" }, remoteHandler);
        FSBL.Clients.ConfigClient.addListener({ field: "finsemble.custom.bloomberg.remote" }, remoteHandler);

        let enabledHandler = (err, enabled) => {
            checkConnection();
            if (err) {
                FSBL.Clients.Logger.error("Error received when checking bloomberg bridge config", err);
            } else {
                let bbgEnabled = typeof enabled.value == "undefined" ? enabled : enabled.value;
                setIsEnabled(bbgEnabled);
            }
        };
        FSBL.Clients.ConfigClient.getValue({ field: "finsemble.custom.bloomberg.enabled" }, enabledHandler);
        FSBL.Clients.ConfigClient.addListener({ field: "finsemble.custom.bloomberg.enabled" }, enabledHandler);

    }, []);

    function toggleBloombergConnection() {
        const _enabled = !isEnabled;
        setIsEnabled(_enabled);
        FSBL.Clients.ConfigClient.setPreference({
            field: "finsemble.custom.bloomberg.enabled",
            value: _enabled
        }, (err, response) => {
            //preference has been set
        });
        bbg.setEnabled(_enabled, (err, resp) => {
            if (err) {
                FSBL.Clients.Logger.error("Error - There was an error setting the Bloomberg connection enabled flag:", err);
            }
            if (resp) {
                //connection has been enabled
            }
        });
    }

    function toggleShowBloomberg() {
        const _enabled = !showBloomberg;
        setShowBloomberg(_enabled);
        FSBL.Clients.ConfigClient.setPreference({
            field: "finsemble.custom.bloomberg.showStatus",
            value: _enabled
        }, (err, response) => {
            //preference has been set
        });
    }

    function updateAddress() {
        const remoteAddress = document.getElementById("address").value;
            FSBL.Clients.ConfigClient.setPreference({
                field: "finsemble.custom.bloomberg.remoteAddress",
                value: remoteAddress
            }, (err, response) => {
                //preference has been set
            });
        setBbgRemoteAddress(remoteAddress);
    }

    const addressInput = React.createElement("input", {
        id: "address",
        name: "address",
        type: "text",
        style: {
            backgroundColor: "var(--core-primary)",
            marginLeft: "70px",
            width: "300px",
            height: "34px",
            color: "#f5f6f7"
        },
        defaultValue: bbgRemoteAddress,
        disabled: isConnected,
        onChange: () => {
            updateAddress();
        },
        onBlur: () => {
            updateAddress();
        }
    }, null);
    const addressField = React.createElement("div", {
        style: {
            maxHeight: isRemote ? "90px" : "0",
            transition: isRemote ? "max-height 0.25s ease-in" : "max-height 0.15s ease-out",
            overflow: isRemote ? "visible" : "hidden",
            marginLeft: "55px",
            marginTop: "10px",
            opacity: "0.75"
        }
    }, ["Address", addressInput]);
    const connectionRadioLocal = React.createElement("input", {
        type: "radio",
        value: "local",
        style: {
            marginLeft: "25px",
            marginRight: "5px",
            backgroundColor: "var(--button-affirmative-background-color)",
            verticalAlign: "bottom"
        },
        name: "location",
        checked: !isRemote,
        disabled: isConnected,
        onClick: () => {
            setIsRemote(false);
            FSBL.Clients.ConfigClient.setPreference({
                field: "finsemble.custom.bloomberg.remote",
                value: false
            }, (err, response) => {
                //preference has been set
            });
        }
    }, null);
    const connectionRadioRemote = React.createElement("input", {
        type: "radio",
        value: "remote",
        style: {
            marginLeft: "20px",
            marginRight: "5px",
            backgroundColor: "var(--button-affirmative-background-color)",
            verticalAlign: "bottom"
        },
        name: "location",
        checked: isRemote,
        disabled: isConnected,
        onClick: () => {
            setIsRemote(true);
            FSBL.Clients.ConfigClient.setPreference({
                field: "finsemble.custom.bloomberg.remote",
                value: true
            }, (err, response) => {
                //preference has been set
            });
        }
    }, null);
    const connectionType = React.createElement("div", {
        style: {
            marginLeft: "55px",
            marginTop: "25px",
            opacity: "0.75"

        }
    }, [
        "Connection Type",
        connectionRadioLocal,
        "Local",
        connectionRadioRemote,
        "Remote"
    ]);

    const bbgStatusMarker = React.createElement("span", {
        style: {
            background: indicatorColor,
            width: "15",
            height: "15px",
            borderRadius: "50%",
            margin: "5px",
            marginLeft: "25px",
            marginRight: "25px",
            paddingLeft: "7px",
            paddingRight: "7px"
        }
    }, " ");

    const connectionToggle = React.createElement("input", {
        name: "connection",
        type: "checkbox",
        style: {
            marginLeft: "10px",
            verticalAlign: "bottom"
        },
        checked: (isEnabled ? "checked" : null),
        onClick: () => { toggleBloombergConnection(); }
    });

    const connection = React.createElement("div", {
        style: {
            opacity: "0.75",
            marginLeft: "55px",
            paddingTop: "10px"
        }
    }, ["Enabled ", connectionToggle, bbgStatusMarker, connectionStatus]);

    const customLine = React.createElement("div", {
        style: {
            width: "400px",
            height: "1px",
            margin: "14.5px 59px 14.5px 64px",
            border: "solid 1px #979797",
            opacity: "0.75"
        }
    }, "");

    const showStatusToggle = React.createElement("input", {
        name: "showStatus",
        type: "checkbox",
        style: {
            marginLeft: "10px",
            verticalAlign: "bottom"
        },
        checked: (showBloomberg ? "checked" : null),
        onClick: () => { toggleShowBloomberg(); }
    });

    const showStatus = React.createElement("div", {
        style: {
            opacity: "0.75",
            marginLeft: "55px",
            paddingTop: "10px"
        }
    }, ["Show Status in Toolbar ", showStatusToggle]);

    return <>
        <div>
            {showStatus}
            {connection}
            {customLine}
        </div>
        <div>
            {connectionType}
            {addressField}
        </div>
    </>;
};


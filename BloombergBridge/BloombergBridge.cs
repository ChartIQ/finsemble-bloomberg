using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloomberglp.Blpapi;
using Bloomberglp.TerminalApiEx;
using ChartIQ.Finsemble;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BloombergBridge
{
	/// <summary>
	/// Class that represents the Bloomberg and Finsemble integration
	/// </summary>
	public class BloombergBridge
	{
		private static readonly AutoResetEvent autoEvent = new AutoResetEvent(false);
		private static readonly object lockObj = new object();
		private static Finsemble FSBL = null;

		private static bool shutdown = false;
		private static bool connect = true;
		private static bool isRegistered = false;
		private static bool remote = false;
		private static string remoteAddress = null;

		private static SecurityLookup secFinder = null;

		/// <summary>
		/// Main runner for Finsemble and Bloomberg integration
		/// </summary>
		/// <param name="args">Arguments used to initialize Finsemble connection - generated by Finsemble when it spawns the component.</param>
		public static void Main(string[] args)
		{
#if DEBUG
			System.Diagnostics.Debugger.Launch();
#endif
			lock (lockObj)
			{
					AppDomain.CurrentDomain.ProcessExit += new System.EventHandler(CurrentDomain_ProcessExit);
			}

			// Initialize Finsemble
			try
			{
					FSBL = new Finsemble(args, null);
					FSBL.Connected += OnConnected;
					FSBL.Disconnected += OnShutdown;
					FSBL.Connect();
			}
			catch (Exception err)
			{
				Console.WriteLine("Exception thrown while connecting to Finsemble, error: " + err.Message);
			}
			// Block main thread until worker is finished.
			autoEvent.WaitOne();
		}
		
		/// <summary>
		/// Function that attempts to connect to the bloomberg terminal and then monitor the connection
		/// until told to shutdown. Cycles once per second.
		/// </summary>
		private static void connectionMonitorThread()
		{
			bool _remote = remote;
			string _remoteAddress = remoteAddress;
			while (!shutdown)
			{
				bool _isRegistered = false;

				if (!connect)
				{ //we've been told to not even try and connect

					_isRegistered = false;

					checkForConnectionStatusChange(_isRegistered, _remote, _remoteAddress);
				}
				else
				{ //go ahead and try to connect
					try
					{
						_isRegistered = BlpApi.IsRegistered;
						checkForConnectionStatusChange(_isRegistered, _remote, _remoteAddress);
					}
					catch (Exception err)
					{
						FSBL.Logger.Error("Bloomberg API registration check failed");
					}

					//try to register
					if (!_isRegistered)
					{
						try
						{
							if (remote)
							{
								FSBL.Logger.Debug(new JToken[] { "Attempting remote registration with address: ", remoteAddress });
								BlpApi.RegisterRemote(remoteAddress);
							}
							else
							{
								FSBL.Logger.Debug(new JToken[] { "Attempting local registration..." });
								BlpApi.Register();
							}
							BlpApi.Disconnected += new System.EventHandler(BlpApi_Disconnected);
							_isRegistered = BlpApi.IsRegistered;
						}
						catch (Exception err)
						{
							_isRegistered = false;
							FSBL.Logger.Debug(new JToken[] { "Bloomberg API registration failed: ", err.ToString() });
						}
					}
					else
					{
						FSBL.Logger.Debug(new JToken[] { "Registered and logged in to terminal." });
					}

					checkForConnectionStatusChange(_isRegistered, _remote, _remoteAddress);
				}
				_remote = remote;
				_remoteAddress = remoteAddress;

				Thread.Sleep(2000);
			}
			FSBL.Logger.Log(new JToken[] { "Exited connection monitoring loop." });
		}

        /// <summary>
        /// Utility function called by connectionMonitorThread to detect a change in the connection status.
        /// </summary>
        /// <param name="_isRegistered"></param>
        /// <param name="_remote"></param>
        /// <param name="_remoteAddress"></param>
        private static void checkForConnectionStatusChange(bool _isRegistered, bool _remote, string _remoteAddress)
		{
			bool statusChange = false;
			if (_isRegistered != isRegistered || _remote != remote || _remoteAddress != remoteAddress)
			{
				//status change
				isRegistered = _isRegistered;

				//don't set remote or remoteAddress as the config listener and connection monitor thread will handle that

				statusChange = true;
			}
			if (statusChange)
			{
				JObject connectionStatus = new JObject();
				connectionStatus.Add("enabled", connect);
				connectionStatus.Add("registered", isRegistered);
				if (isRegistered)
                {
					if (!remote)
					{
						connectionStatus.Add("connectedTo", "localhost");
					}
					else
					{
						connectionStatus.Add("connectedTo", remoteAddress);
					}
				}
				
				FSBL.RouterClient.Transmit("BBG_connection_status", connectionStatus);
				FSBL.Logger.Log("Bloomberg connection status changed: ", connectionStatus);

				if (isRegistered) //we've just logged in
				{
					try
					{
						//setup a handler for group events
						BlpTerminal.GroupEvent += BlpTerminal_ComponentGroupEvent;

						//setup security finder
						if (remote)
                        {
							//the Bloomberg DAPI only supports local connections, hence SecurityLookup can't be initialized
                        } else
                        {
							secFinder = new SecurityLookup();
							secFinder.Init();
						}
					}
					catch (Exception err)
					{
						isRegistered = false;
						FSBL.Logger.Error("Error occurred during post-login setup: ", err.Message);
					}
				}
				else
				{
					try
					{
						//dispose of security finder
						if (secFinder != null)
						{
							secFinder.Dispose();
							secFinder = null;
						}
					}
					catch (Exception err)
					{
						FSBL.Logger.Debug("Exception occurred during disposal of security finder:", err.Message);
					}
				}
			}
		}

		/// <summary>
		/// Function that runs when the Bloomberg Bridge successfully connects to Finsemble.
		/// </summary>
		/// <remarks>
		/// Sets up query responders and thread to manage the connection to teh BBG terminal.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnConnected(object sender, EventArgs e)
		{
			FSBL.Logger.Log("Bloomberg bridge connected to Finsemble.");

			//setup Router endpoints
			addResponders();

			//Handler function for received configuration values
			EventHandler<FinsembleEventArgs> ConfigHandler = (routerClient, response) =>
			{
				if (response.error != null)
				{
					FSBL.Logger.Warn(new JToken[] { "Error occurred while receiving Bloomberg remote config...", response.error });
				}
				else
				{
					FSBL.Logger.Debug(new JToken[] { "Received bloomberg bridge configuration: ", response.response?["data"] });
				}
				JToken data = response.response?["data"];
				//handle different return data from ConfigClient.GetValue and COnfigClient.AddListner
				if (data?["value"] != null)
                {
					data = data?["value"];
				}
				if (data != null && data?["remote"] != null)
				{ //possible remote connection
					if (bool.Parse(data?["remote"].ToString()) &&
						data?["remoteAddress"] != null)
					{
						remote = true;
						remoteAddress = data?["remoteAddress"].ToString();
					} else
                    {
						remote = false;
					}
				}
				if (data != null && data?["enabled"] != null)
				{
					connect = bool.Parse(data?["enabled"].ToString());
				}
			};

			//retrieve the initial config
			FSBL.ConfigClient.GetValue(new JObject { ["field"] = "finsemble.custom.bloomberg" }, ConfigHandler);

			//subscribe to changes in the config
			FSBL.ConfigClient.AddListener(new JObject { ["field"] = "finsemble.custom.bloomberg" }, ConfigHandler, (routerClient, response) => {
				//start up connection monitor thread
				Thread thread = new Thread(new ThreadStart(connectionMonitorThread));
				thread.Start();
			});
		}

		/// <summary>
		/// Handler for when the Bloomberg Bridge process is terminated.
		/// </summary>
		/// <param name="sender">Object</param>
		/// <param name="e">EventArgs</param>
		// ! Should be client agnostic
		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			shutdown = true;
			removeResponders();
		}

		/// <summary>
		/// Handles Finsemble shutdown event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnShutdown(object sender, EventArgs e)
		{
			shutdown = true;
			if (FSBL != null)
			{
				lock (lockObj)
				{
					if (FSBL != null)
					{
						try
						{
							removeResponders();

							// Dispose of Finsemble.
							FSBL.Dispose();
						}
						catch { }
						finally
						{
							FSBL = null;
							//Environment.Exit(0);
						}
					}

				}
			}
			// Release main thread so application can exit.
			autoEvent.Set();
		}

		/// <summary>
		/// Handler function for group (context changed) events sent by the terminal.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void BlpTerminal_ComponentGroupEvent(object sender, BlpGroupEventArgs e)
		{
			Type type = e.GetType();
			FSBL.Logger.Debug("Received Bloomberg group event type: ", type.FullName);

			BlpGroupContextChangedEventArgs context = e as BlpGroupContextChangedEventArgs;
			if (context != null)
			{
				JObject output = new JObject();
				if (context.Group != null)
				{
					output.Add("group", renderGroup(context.Group));
				}
				if (context.Groups != null)
				{
					JArray groups = new JArray();
					foreach (var group in context.Groups)
					{
						groups.Add(renderGroup(group));
					}

					output.Add("groups", groups);
				}
				if (string.IsNullOrWhiteSpace(context.Cookie))
				{
					output.Add("cookie", context.Cookie);
				}
				output.Add("externalSource", context.ExternalSource);

				FSBL.Logger.Log("Group Context changed event: ", output);

				FSBL.RouterClient.Transmit("BBG_group_context_events", output);
			}
		}
		
		/// <summary>
		/// Sets up query responders to expose the functions of the bridge as an API.
		/// </summary>
		private static void addResponders()
		{
			FSBL.Logger.Log("Setting up query responders");
			try
			{
				FSBL.RouterClient.AddResponder("BBG_connect", (fsbl_sender, queryMessage) =>
				{
					BBG_connect(queryMessage);
				});

				FSBL.RouterClient.AddResponder("BBG_connection_status", (fsbl_sender, queryMessage) =>
				{
					BBG_connection_status(queryMessage);
				});

				FSBL.RouterClient.AddResponder("BBG_run_terminal_function", (fsbl_sender, queryMessage) =>
				{
					BBG_run_terminal_function(queryMessage);
				});
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				FSBL.Logger.Error("Error occurred while setting up query responders: ", err.Message );
			}
		}

		/// <summary>
		/// Remove query responders, used on shutdown.
		/// </summary>
		private static void removeResponders()
		{
			try
			{
				FSBL.Logger.Log("Removing query responders");
				FSBL.RouterClient.RemoveResponder("BBG_connect", true);
				FSBL.RouterClient.RemoveResponder("BBG_connection_status", true);
				FSBL.RouterClient.RemoveResponder("BBG_run_terminal_function", true);
			}
			catch (Exception err) { }

			//dispose of security finder
			if (secFinder != null)
			{
				secFinder.Dispose();
				secFinder = null;
			}
		}

		/// <summary>
		/// Query responder to instruct the bridge to attempt connection or disconnect from the terminal
		/// </summary>
		/// <param name="queryMessage"></param>
		private static void BBG_connect(FinsembleQueryArgs queryMessage)
		{
			if (queryMessage.error != null)
			{
				//failed to register the query responder properly
				FSBL.Logger.Error("Error received by BBG_connect query responder: ", queryMessage.error);
			}
			else
			{
				JObject queryResponse = new JObject();
				JToken queryData = queryMessage.response?["data"];
				if (queryData != null)
				{
					FSBL.Logger.Debug("Received query: ", queryData);
                    try
                    {
						bool _connect = queryData.Value<bool>("connect");
						connect = _connect;
						queryResponse.Add("status", true);
						queryResponse.Add("message", "BloombergBridge connect state = " + connect);
					} catch (Exception err)
                    {
						queryResponse.Add("status", false);
						queryResponse.Add("message", "Exception occurred while processing bbg_connect query, message: " + err.Message);
					}
				}
				else
				{
					queryResponse.Add("status", false);
					queryResponse.Add("message", "Invalid request: no query data");
				}

				queryMessage.sendQueryMessage(new FinsembleEventResponse(queryResponse, null));
			}

		}

		/// <summary>
		/// Query responder to check if we are connected to the terminal and whether the user is logged in
		/// </summary>
		/// <param name="queryMessage"></param>
		private static void BBG_connection_status(FinsembleQueryArgs queryMessage)
		{
			if (queryMessage.error != null)
			{
				//failed to register the query responder properly
				FSBL.Logger.Error("Error received by BBG_connection_status query responder: ", queryMessage.error );
			} else {
				JObject connectionStatus = new JObject();
				connectionStatus.Add("enabled", connect);
				connectionStatus.Add("registered", isRegistered);
				if (isRegistered)
				{
					if (!remote)
					{
						connectionStatus.Add("connectedTo", "localhost");
					}
					else
					{
						connectionStatus.Add("connectedTo", remoteAddress);
					}
				}
				queryMessage.sendQueryMessage(new FinsembleEventResponse(connectionStatus,null));

				Console.WriteLine("Responded to BBG_connection_status query: " + connectionStatus.ToString());
				FSBL.Logger.Log("Responded to BBG_connection_status query: ", connectionStatus);
			}
			
		}

		/// <summary>
		/// Function that fires when the Terminal Connect API disconnects. Used to issue connection events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void BlpApi_Disconnected(object sender, EventArgs e)
		{
			//status change
			isRegistered = false;
			JObject connectionStatus = new JObject();
			connectionStatus.Add("enabled", connect);
			connectionStatus.Add("registered", isRegistered);
			FSBL.RouterClient.Transmit("BBG_connection_status", connectionStatus);
			Console.WriteLine("Transmitted connection status after disconnect: " + connectionStatus.ToString());
			FSBL.Logger.Log("Transmitted connection status after disconnect: ", connectionStatus);
		}

		/// <summary>
		/// Query handler function that runs a specified terminal connect command and responds.
		/// </summary>
		/// <param name="queryMessage"></param>
		private static void BBG_run_terminal_function(FinsembleQueryArgs queryMessage)
		{
			if (queryMessage.error != null)
			{
				//failed to register the query responder properly
				FSBL.Logger.Error("Error received by BBG_run_terminal_function query responder: ", queryMessage.error);
			}
			else
			{
				JObject queryResponse = new JObject();
				JToken queryData = null;
				if (isRegistered)
				{
					queryData = queryMessage.response?["data"];
					if (queryData != null)
					{
						FSBL.Logger.Debug("Received query: ", queryData);
						BBG_execute_terminal_function(queryResponse, queryData);
					}
					else
					{
						queryResponse.Add("status", false);
						queryResponse.Add("message", "Invalid request: no query data");
					}
				}
				else if (!isRegistered)
				{
					queryResponse.Add("status", false);
					queryResponse.Add("message", "Not registered with the Bloomberg Terminal");
				}

				//return the response
				queryMessage.sendQueryMessage(new FinsembleEventResponse(queryResponse, null));
				Console.WriteLine("Responded to BBG_run_terminal_function query: " + queryResponse.ToString());
				FSBL.Logger.Debug("Responded to BBG_run_terminal_function query: ", queryData, "Response: ", queryResponse );
			}
		}
		/// <summary>
		/// Utility function called by BBG_run_terminal_function to actually execute each supported terminal function.
		/// </summary>
		/// <param name="queryResponse">JObject to add response data to</param>
		/// <param name="queryData">Optional query data</param>
		private static void BBG_execute_terminal_function(JObject queryResponse, JToken queryData)
		{
			string requestedFunction = queryData.Value<string>("function");
			if (requestedFunction != null)
			{
				try
				{
					switch (requestedFunction)
					{
						case "RunFunction":
							RunFunction(queryResponse, queryData);
							break;
						case "CreateWorksheet":
							CreateWorksheet(queryResponse, queryData);
							break;
						case "GetWorksheet":
							GetWorksheet(queryResponse, queryData);
							break;
						case "ReplaceWorksheet":
							ReplaceWorksheet(queryResponse, queryData);
							break;
						case "AppendToWorksheet":
							AppendToWorksheet(queryResponse, queryData);
							break;
						case "GetAllWorksheets":
							GetAllWorksheets(queryResponse);
							break;
						case "GetAllGroups":
							GetAllGroups(queryResponse);
							break;
						case "GetGroupContext":
							GetGroupContext(queryResponse, queryData);
							break;
						case "SetGroupContext":
							SetGroupContext(queryResponse, queryData);
							break;
						case "SecurityLookup":
							SecurityLookup(queryResponse, queryData);
							break;
						case "CreateComponent":
							queryResponse.Add("status", false);
							queryResponse.Add("message", "function '" + requestedFunction + "' not implemented yet");
							break;
						case "DestroyAllComponents":
							queryResponse.Add("status", false);
							queryResponse.Add("message", "function '" + requestedFunction + "' not implemented yet");
							break;
						case "GetAvailableComponents":
							queryResponse.Add("status", false);
							queryResponse.Add("message", "function '" + requestedFunction + "' not implemented yet");
							break;
						case "":
						case null:
							queryResponse.Add("status", false);
							queryResponse.Add("message", "No function specified to run");
							break;
						default:
							queryResponse.Add("status", false);
							queryResponse.Add("message", "Unknown function '" + requestedFunction + "' specified");
							break;
					}

				}
				catch (Exception err)
				{
					queryResponse.Add("status", false);
					queryResponse.Add("message", "Exception occurred while running '" + requestedFunction + "', message: " + err.Message);
				}
			}
			else
			{
				queryResponse.Add("status", false);
				queryResponse.Add("message", "No requested function found in query data:" + queryData.ToString());
			}
		}

		private static void RunFunction(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("RunFunction", queryData, new string[] { "mnemonic", "panel" }, null, queryResponse))
			{
				string BBG_mnemonic = queryData.Value<string>("mnemonic");
				string panel = queryData.Value<string>("panel");
				string tails = null;
				List<string> securitiesList = new List<string>();
				if (queryData["tails"] != null)
				{
					tails = queryData.Value<string>("tails");
				}

				if (queryData["securities"] != null)
				{

					foreach (string a in queryData["securities"])
					{
						securitiesList.Add(a);
					}
				}
				if (securitiesList.Count > 0)
				{
					BlpTerminal.RunFunction(BBG_mnemonic, panel, securitiesList, tails);
					queryResponse.Add("status", true);
				}
				else
				{
					BlpTerminal.RunFunction(BBG_mnemonic, panel, tails);
					queryResponse.Add("status", true);
				}
			}
		}

		private static void CreateWorksheet(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("CreateWorksheet", queryData, new string[] { "securities", "name" }, null, queryResponse))
			{
				var _securities = new List<string>();
				foreach (string a in queryData["securities"])
				{
					_securities.Add(a);
				}
				BlpWorksheet worksheet = BlpTerminal.CreateWorksheet(queryData["name"].ToString(), _securities);
				queryResponse.Add("status", true);
				queryResponse.Add("worksheet", renderWorksheet(worksheet, true));
			}
		}

		private static void GetWorksheet(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("GetWorksheet", queryData, null, new string[] { "name", "id" }, queryResponse))
			{
				string worksheetId = resolveWorksheetId(queryData, queryResponse);
				if (worksheetId != null)
				{
					BlpWorksheet worksheet = BlpTerminal.GetWorksheet(worksheetId);
					if (worksheet != null)
					{
						queryResponse.Add("status", true);
						queryResponse.Add("worksheet", renderWorksheet(worksheet, true));
					}
					else
					{
						queryResponse.Add("status", false);
						queryResponse.Add("message", "Worksheet with id '" + worksheetId + "' not found");
					}
				}
			}
		}

		private static void ReplaceWorksheet(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("ReplaceWorksheet", queryData, new string[] { "securities" }, new string[] { "name", "id" }, queryResponse))
			{
				List<string> securities = new List<string>();
				foreach (string a in queryData["securities"])
				{
					securities.Add(a);
				}

				string worksheetId = resolveWorksheetId(queryData, queryResponse);
				if (worksheetId != null)
				{
					BlpWorksheet worksheet = BlpTerminal.GetWorksheet(worksheetId);
					if (worksheet != null)
					{
						worksheet.ReplaceSecurities(securities);
						queryResponse.Add("status", true);
						queryResponse.Add("worksheet", renderWorksheet(worksheet, true));
					}
					else
					{
						queryResponse.Add("status", false);
						queryResponse.Add("message", "Worksheet with id '" + worksheetId + "' not found");
					}
				}
			}
		}

		private static void AppendToWorksheet(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("AppendToWorksheet", queryData, new string[] { "securities" }, new string[] { "name", "id" }, queryResponse))
			{
				List<string> securities = new List<string>();
				foreach (string a in queryData["securities"])
				{
					securities.Add(a);
				}

				string worksheetId = resolveWorksheetId(queryData, queryResponse);
				if (worksheetId != null)
				{
					BlpWorksheet worksheet = BlpTerminal.GetWorksheet(worksheetId);
					if (worksheet != null)
					{
						worksheet.AppendSecurities(securities);
						queryResponse.Add("status", true);
						queryResponse.Add("worksheet", renderWorksheet(worksheet, true));
					}
					else
					{
						queryResponse.Add("status", false);
						queryResponse.Add("message", "Worksheet with id '" + worksheetId + "' not found");
					}
				}
			}
		}

		private static void GetAllWorksheets(JObject queryResponse)
		{
			var allWorksheets = BlpTerminal.GetAllWorksheets();
			JArray worksheets = new JArray();
			foreach (BlpWorksheet sheet in allWorksheets)
			{
				worksheets.Add(renderWorksheet(sheet, false));
			}
			queryResponse.Add("status", true);
			queryResponse.Add("worksheets", worksheets);
		}

		private static void GetAllGroups(JObject queryResponse)
		{
			var allGroups = BlpTerminal.GetAllGroups();
			JArray groups = new JArray();
			foreach (BlpGroup group in allGroups)
			{
				groups.Add(renderGroup(group));
			}
			queryResponse.Add("status", true);
			queryResponse.Add("groups", groups);
		}

		private static void GetGroupContext(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("GetGroupContext", queryData, new string[] { "name" }, null, queryResponse))
			{
				BlpGroup group = BlpTerminal.GetGroupContext(queryData["name"].ToString());
				queryResponse.Add("status", true);
				queryResponse.Add("group", renderGroup(group));
			}
		}

		private static System.Timers.Timer debounceTimer = null;
		private static DateTimeOffset lastQueryTime = DateTimeOffset.UtcNow;
		private const int SET_GROUP_CONTEXT_THROTTLE = 1200;
		private static void SetGroupContext(JObject queryResponse, JToken queryData)
		{
			if (validateQueryData("SetGroupContext", queryData, new string[] { "name", "value" }, null, queryResponse))
			{
				DateTimeOffset now = DateTimeOffset.UtcNow;
				TimeSpan ts = now.Subtract(lastQueryTime);
				if (ts.TotalMilliseconds < SET_GROUP_CONTEXT_THROTTLE)
				{
					if (debounceTimer != null)
					{
						debounceTimer.Stop();
					}
					debounceTimer = new System.Timers.Timer();
					debounceTimer.Interval = SET_GROUP_CONTEXT_THROTTLE - ts.TotalMilliseconds;
					debounceTimer.Elapsed += async (sender2, args2) =>
					{
						debounceTimer.Stop();
						DoSetGroupContext(queryResponse, queryData);
					};
					debounceTimer.Start();
				}
				else
				{
					DoSetGroupContext(queryResponse, queryData);
				}
			}
		}

		private static void DoSetGroupContext(JObject queryResponse, JToken queryData)
		{
			//save time of last set to allow debouncing   
			lastQueryTime = DateTimeOffset.UtcNow;

			if (queryData["cookie"] != null && queryData["cookie"].ToString() != "")
			{
				BlpTerminal.SetGroupContext(queryData["name"].ToString(), queryData["value"].ToString(), queryData["cookie"].ToString());
			}
			else
			{
				BlpTerminal.SetGroupContext(queryData["name"].ToString(), queryData["value"].ToString());
			}
			queryResponse.Add("status", true);
		}

		private static void SecurityLookup(JObject queryResponse, JToken queryData)
		{
			if (remote)
            {
				queryResponse.Add("status", false);
				queryResponse.Add("message", "Security lookup is a function of the Bloomberg Desktop API (DAPI) which is only supported for local connections. You are connected to a remote terminal");
			} 
			else
            {
				if (validateQueryData("SecurityLookup", queryData, new string[] { "security" }, null, queryResponse))
				{

					JArray resultsArr = new JArray();
					//needs to be surrounded by a lock or mutex as the SecurityLookup code only supports single threaded ops
					//  the alternative is to setup a securityFinder object for each query then dispose of it after:
					//    var secFinder = new SecurityLookup();
					//    secFinder.Init();
					//      do query
					//    secFinder.Dispose();
					//    secFinder = null;
					lock (secFinder)
					{
						secFinder.Query(queryData["security"].ToString(), 10);
						IList<string> results = secFinder.GetResults();

						//convert the results into the security name and Type
						//  results typically look like this: AAPL US<equity>
						//  when we need to output: { name: "AAPL US", type: "Equity" }
						for (int i = 0; i < results.Count; i++)
						{
							string result = results[i];
							JObject resultObj = new JObject();
							int typeStartIndex = result.LastIndexOf('<');
							if (typeStartIndex > -1)
							{
								resultObj.Add("name", result.Substring(0, typeStartIndex).Trim());
								resultObj.Add("type", char.ToUpper(result[typeStartIndex + 1]) + result.Substring(typeStartIndex + 2, result.Length - (typeStartIndex + 2) - 1));
							}
							resultsArr.Add(resultObj);
						}
					}

					queryResponse.Add("status", true);
					queryResponse.Add("results", resultsArr);
				}
			}
		}


		//-----------------------------------------------------------------------
		//Private Utility functions
		private static bool validateQueryData(string function, JToken queryData, string[] allRequiredArgs, string[] anyRequiredArgs, JObject queryResponse)
		{
			if (allRequiredArgs != null)
			{
				foreach (string s in allRequiredArgs)
				{
					if (queryData[s] == null)
					{
						queryResponse.Add("status", false);
						queryResponse.Add("message", "function '" + function + "' requires argument '" + s + "' which was not set");
						return false;
					}
				}
			}
			if (anyRequiredArgs != null)
			{
				foreach (string s in anyRequiredArgs)
				{
					if (queryData[s] != null)
					{
						return true;
					}
				}
				queryResponse.Add("status", false);
				queryResponse.Add("message", "function '" + function + "' requires at least one  of (" + string.Join(", ", anyRequiredArgs) + ") none of which were set");
				return false;
			}
			return true;
		}

		private static string resolveWorksheetId(JToken queryData, JObject queryResponse)
		{
			string worksheetId = null;
			if (queryData["id"] == null)
			{
				var worksheetName = queryData.Value<string>("name");
				var allWorksheets = BlpTerminal.GetAllWorksheets();
				foreach (BlpWorksheet sheet in allWorksheets)
				{
					if (sheet.Name.Equals(worksheetName))
					{
						worksheetId = sheet.Id;
						break;
					}
				}
				if (worksheetId == null)
				{
					queryResponse.Add("status", false);
					queryResponse.Add("message", "Worksheet '" + worksheetName + "' not found");
				}
			}
			else
			{
				worksheetId = queryData.Value<string>("id");
			}
			return worksheetId;
		}

		private static JObject renderWorksheet(BlpWorksheet worksheet, bool includeSecurities = false)
		{
			JObject worksheetObj = new JObject {
				{ "name", worksheet.Name },
				{ "id", worksheet.Id },
				{ "isActive", worksheet.IsActive }
			};

			if (includeSecurities)
			{
				var securities = worksheet.GetSecurities();
				JArray securitiesArr = new JArray();
				JObject obj = new JObject();
				foreach (string a in securities)
				{
					securitiesArr.Add(a);
				}
				worksheetObj.Add("securities", securitiesArr);
			}
			return worksheetObj;
		}

		private static JObject renderGroup(BlpGroup group)
		{
			JObject groupObj = new JObject
			{
				{ "name", group.Name },
				{ "type", group.Type },
				{ "value", group.Value }
			};
	
			return groupObj;
		}
	}
}

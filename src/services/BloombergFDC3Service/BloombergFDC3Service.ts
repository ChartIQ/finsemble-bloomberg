// import { fdc3, Context } from "../../../fdc3-types";
// eslint-disable-next-line
const Finsemble = require("@chartiq/finsemble");
const FDC3Client = require("../FDC3/FDC3Client").default;

Finsemble.Clients.Logger.start();
Finsemble.Clients.Logger.log("BloombergFDC3Service starting up");

type ProviderChannel = {
  channelName: string;
  inbound?: string | null;
  outbound?: string | null;
};

class BloombergFDC3Service extends Finsemble.baseService {
  channel: string;
  provider: string;
  channels: any[];

  constructor() {
    super({
      // Declare any service or client dependencies that must be available before your service starts up.
      startupDependencies: {
        // If the service is using another service directly via an event listener or a responder, that service
        // should be listed as a service start up dependency.
        services: ["FDC3", "routerService"],
        // When ever you use a client API with in the service, it should be listed as a client startup
        // dependency. Any clients listed as a dependency must be initialized at the top of this file for your
        // service to startup.
        clients: ["distributedStoreClient"],
      },
    });

    this.readyHandler = this.readyHandler.bind(this);
    this.onBaseServiceReady(this.readyHandler);

    this.fdc3Ready.bind(this);

    this.provider = "Bloomberg";
    this.channels = [];
  }

  /**
   * Fired when the service is ready for initialization
   * @param {function} callback
   */
  async readyHandler(callback: Function) {
    this.createRouterEndpoints();
    this.fdc3Ready();
    callback();
  }

  // ? add any functionality that requires FDC3 in here
  fdc3Ready() {
    this.FDC3Client = new FDC3Client(Finsemble);
    window.FSBL = {};
    FSBL.Clients = Finsemble.Clients;
    window.addEventListener("fdc3Ready", () => this.BloombergFDC3());
  }

  async BloombergFDC3() {
    const channelName = "channel name here"; // ! change me
    const inbound = "inbound channel"; // ! change me
    const outbound = "outbound channel"; // ! change me

    const BBG = ""; // ! change me
    const fdc3Message = this.translateMessagesToFDC3(BBG);

    const providerChannel: ProviderChannel = {
      channelName,
      inbound,
      outbound,
    };

    // set the store with the channel - do this on every new channel added e.g "GroupA", "GroupB" ...
    this.setChannelMatcherStore(providerChannel, this.provider);

    const channel = await fdc3.getOrCreateChannel(channelName);

    // ? use this if you want to send data
    channel.broadcast(fdc3Message);

    // ? use this if you want to listen to incoming data
    channel.addContextListener((context: Context) => {});
  }

  // TODO: add your translation here. This will be replaced with a better translator soon!
  translateMessagesToFDC3(context: any) {
    const ticker = "FB";
    const FDC3Context = {
      type: "fdc3.instrument",
      id: {
        ticker,
      },
    };

    return FDC3Context;
  }

  setChannelMatcherStore(channel: ProviderChannel, provider: string) {
    const { channelName, inbound, outbound } = channel;

    Finsemble.Clients.DistributedStoreClient.getStore(
      {
        store: "FDC3ToExternalChannelPatches",
      },
      (err: any, store: any) => {
        if (err) throw new Error(err);
        store.setValue({
          field: `${provider}.${channelName}`,
          value: { inbound, outbound },
        });
      }
    );
  }

  /**
   * Creates a router endpoint for you service.
   * Add query responders, listeners or pub/sub topic as appropriate.
   */
  createRouterEndpoints() {
    Finsemble.Clients.RouterClient.addResponder(
      "query-channel",
      (err: any, res: any) => {}
    );

    Finsemble.Clients.RouterClient.addPubSubResponder("publish-channel", {
      State: "start",
    });
    Finsemble.Clients.RouterClient.subscribe(
      "publish-channel",
      (err: any, res: any) => {}
    );

    Finsemble.Clients.RouterClient.addListener(
      "transmit-channel",
      (err: any, res: any) => {}
    );
  }
}

const serviceInstance = new BloombergFDC3Service();

serviceInstance.start();
module.exports = serviceInstance;

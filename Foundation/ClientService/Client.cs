using Foundation.AssetBundles;
using Foundation.ConfigurationResolver;
using Foundation.DebugUtils.ErrorAlertService;
using Foundation.DeviceInfoService;
using Foundation.Facebook;
using Foundation.FileLogger;
using Foundation.Logger;
using Foundation.MonoUtils;
using Foundation.Popups;
using Foundation.ServicesResolver;
using Foundation.TimerUtils;
using Foundation.Network;
using Foundation.SceneService;
using Foundation.AppsFlyer;
using Foundation.Sound;

namespace Foundation.ClientService
{
    public abstract class Client : BaseMonoBehaviour
    {
        private static Client _baseInstance;
        public static Client BaseInstance => _baseInstance;

        public ServiceResolver ServiceResolver { get; protected set; }
        public ConfigResolver ConfigResolver { get; protected set; }

        #region Life Cycle

        /// <summary>
        /// Single entry point to Client's code
        /// </summary>
        protected virtual void Awake()
        {
            // Setup singleton (we can't use mono base singleton due to the inheritance of Games Clients).
            if (_baseInstance != null)
            {
                this.LogError("Client singleton already exists - destroying duplicate!");
                Destroy(gameObject);
                return;
            }
            _baseInstance = this;

            // Initialize all foundation services...
            InitializeFoundationServices();
        }

        protected virtual void Start()
        {
            // Initialize actual game at least 1 frame after initializing foundation.
            // That ensures that any foundation service or 3rd party plugin that uses Awake for initialization
            // will be available for the "game initialization"
            InitializeGame();
        }

        /// <summary>
        /// If client is destroyed - dispose & clean-up
        /// </summary>
        protected virtual void OnDestroy()
        {
            ServiceResolver?.DisposeAllServices();
            ServiceResolver = null;
            _baseInstance = null;
        }

        #endregion

        #region Initialization

        private void InitializeFoundationServices()
        {
            ConfigResolver = new ConfigResolver();
            ServiceResolver = new ServiceResolver(ConfigResolver);

            InitializeCoreServices();
            InitializeDebugServices();
        }

        private void InitializeCoreServices()
        {
            // Start initializing services 1 by 1 according to order & dependencies
            ServiceResolver.Inject(new LoggerInitializer());
            ServiceResolver.Inject(new MonoService());
            ServiceResolver.Inject<ITimerService>(new TimerService());
            ServiceResolver.Inject(new DeviceService());

            ServiceResolver.Inject(new FacebookService());
            ServiceResolver.Inject<ISceneTransitionService>(new SceneTransitionService());
            ServiceResolver.Inject<IHttpService>(new HTTPService());
            ServiceResolver.Inject<IServerHTTPService>(new ServerHTTPService());
            ServiceResolver.Inject<ISocketService>(new SocketService());
            ServiceResolver.Inject(new InternetConnectionService());
            ServiceResolver.Inject<IAssetBundlesService>(new AssetBundlesService());
            ServiceResolver.Inject<IPopupService>(new PopupService());
            ServiceResolver.Inject(new AppsFlyerService());
            ServiceResolver.Inject(new SoundService());
        }

        private void InitializeDebugServices()
        {
            #if ENABLE_FILE_LOGGER
            ServiceResolver.Inject(new FileLoggerService());
            #endif
            #if ENABLE_ERROR_ALERTS
            ServiceResolver.Inject(new ErrorAlertService());
            #endif
        }

        /// <summary>
        /// Called internally after initializing foundation core services
        /// Should be used by game project to initialize the game
        /// </summary>
        protected abstract void InitializeGame();

        #endregion
    }
}

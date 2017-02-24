using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Zyborg.Vault.Helper.Logging;

namespace Zyborg.Vault
{
	public static partial class Constants
	{
		//~ const (
		// coreLockPath is the path used to acquire a coordinating lock
		// for a highly-available deploy.
		//~ coreLockPath = "core/lock"
		public const string coreLockPath = "core/lock";

		// coreLeaderPrefix is the prefix used for the UUID that contains
		// the currently elected leader.
		//~ coreLeaderPrefix = "core/leader/"
		public const string coreLeaderPrefix = "core/leader/";

		// lockRetryInterval is the interval we re-attempt to acquire the
		// HA lock if an error is encountered
		//~ lockRetryInterval = 10 * time.Second
		public static readonly TimeSpan lockRetryInterval = TimeSpan.FromSeconds(10);

		// keyRotateCheckInterval is how often a standby checks for a key
		// rotation taking place.
		//~ keyRotateCheckInterval = 30 * time.Second
		public static readonly TimeSpan keyRotateCheckInterval = TimeSpan.FromSeconds(30);

		// keyRotateGracePeriod is how long we allow an upgrade path
		// for standby instances before we delete the upgrade keys
		//~ keyRotateGracePeriod = 2 * time.Minute
		public static readonly TimeSpan keyRotateGracePeriod = TimeSpan.FromMinutes(2);

		// leaderPrefixCleanDelay is how long to wait between deletions
		// of orphaned leader keys, to prevent slamming the backend.
		//~ leaderPrefixCleanDelay = 200 * time.Millisecond
		public static readonly TimeSpan leaderPrefixCleanDelay = TimeSpan.FromMilliseconds(200);

		// coreKeyringCanaryPath is used as a canary to indicate to replicated
		// clusters that they need to perform a rekey operation synchronously; this
		// isn't keyring-canary to avoid ignoring it when ignoring core/keyring
		//~ coreKeyringCanaryPath = "core/canary-keyring"
		public const string coreKeyringCanaryPath = "core/canary-keyring";
	}

	public static partial class Globals
	{
		//~var (
		// ErrAlreadyInit is returned if the core is already
		// initialized. This prevents a re-initialization.
		//~ ErrAlreadyInit = errors.New("Vault is already initialized")
		public class ErrAlreadyInit : Exception
		{
			public ErrAlreadyInit() : base("Vault is already initialized") { }
		}

		// ErrNotInit is returned if a non-initialized barrier
		// is attempted to be unsealed.
		//~ ErrNotInit = errors.New("Vault is not initialized")
		public class ErrNotInit : Exception
		{
			public ErrNotInit() : base("Vault is not initialized") { }
		}

		// ErrInternalError is returned when we don't want to leak
		// any information about an internal error
		//~ ErrInternalError = errors.New("internal error")
		public class ErrInternalError : Exception
		{
			public ErrInternalError() : base("internal error") { }
		}

		// ErrHANotEnabled is returned if the operation only makes sense
		// in an HA setting
		//~ ErrHANotEnabled = errors.New("Vault is not configured for highly-available mode")
		public class ErrHANotEnabled : Exception
		{
			public ErrHANotEnabled() : base("Vault is not configured for highly-available mode") { }
		}

		// manualStepDownSleepPeriod is how long to sleep after a user-initiated
		// step down of the active node, to prevent instantly regrabbing the lock.
		// It's var not const so that tests can manipulate it.
		//~ manualStepDownSleepPeriod = 10 * time.Second
		public static TimeSpan manualStepDownSleepPeriod
		{ get; set; } = TimeSpan.FromSeconds(10);

		// Functions only in the Enterprise version
		//~ enterprisePostUnseal = enterprisePostUnsealImpl
		//~ enterprisePreSeal    = enterprisePreSealImpl
		//~ startReplication     = startReplicationImpl
		//~ stopReplication      = stopReplicationImpl
		// TODO: Maybe in the far off future

	}

	// ReloadFunc are functions that are called when a reload is requested.
	//~ type ReloadFunc func(map[string]string) error
	public delegate void ReloadFunc(IDictionary<string, string> map);

	// NonFatalError is an error that can be returned during NewCore that should be
	// displayed but not cause a program exit
	//~ type NonFatalError struct {
	public class NonFatalError
	{
		//~ Err error
		public Exception Err
		{ get; set; }

		//~ func (e *NonFatalError) WrappedErrors() []error {
		public IEnumerable<Exception> WrappedErrors()
		{
			//~ return []error{e.Err}
			return new[] { Err };
		}

		//~ func (e *NonFatalError) Error() string {
		public string Error()
		{
			//~ return e.Err.Error()
			return Err.Message;
		}
	}

	// ErrInvalidKey is returned if there is a user-based error with a provided
	// unseal key. This will be shown to the user, so should not contain
	// information that is sensitive.
	//~ type ErrInvalidKey struct {
	public class ErrInvalidKey
	{
		//~ Reason string
		public string Reason
		{ get; set; }
	
		//~ func (e *ErrInvalidKey) Error() string {
		public string Error()
		{
			//~ return fmt.Sprintf("invalid key: %v", e.Reason)
			return $"invalid key: {Reason}";
		}
	}

	//~ type activeAdvertisement struct {
	class ActiveAdvertisement
	{
		//~ RedirectAddr     string            `json:"redirect_addr"`
		//~ ClusterAddr      string            `json:"cluster_addr,omitempty"`
		//~ ClusterCert      []byte            `json:"cluster_cert,omitempty"`
		//~ ClusterKeyParams *clusterKeyParams `json:"cluster_key_params,omitempty"`
		[JsonProperty("redirect_addr")]
		public string RedirectAddr
		{ get; set; }
		[JsonProperty("cluster_addr", NullValueHandling = NullValueHandling.Ignore)]
		public string ClusterAddr
		{ get; set; }
		[JsonProperty("cluster_cert", NullValueHandling = NullValueHandling.Ignore)]
		public byte[] ClusterCert
		{ get; set; }
		[JsonProperty("cluster_key_params", NullValueHandling = NullValueHandling.Ignore)]
		public ClusterKeyParams RedirectAddr
		{ get; set; }
	}

	//~ type unlockInformation struct {
	class UnlockInformation
	{
		//~ Parts [][]byte
		//~ Nonce string
		public byte[][] Parts
		{ get; set; }
		public string Nonce
		{ get; set; }
	}

	// Core is used as the central manager of Vault activity. It is the primary point of
	// interface for API handlers and is responsible for managing the logical and physical
	// backends, router, security barrier, and audit trails.
	//~ type Core struct {
	public class Core
	{
		// N.B.: This is used to populate a dev token down replication, as
		// otherwise, after replication is started, a dev would have to go through
		// the generate-root process simply to talk to the new follower cluster.
		//~ devToken string
		internal string devToken;

		// HABackend may be available depending on the physical backend
		//~ ha physical.HABackend
		internal Physical.IHABackend ha;

		// redirectAddr is the address we advertise as leader if held
		//~ redirectAddr string
		internal string redirectAddr;

		// clusterAddr is the address we use for clustering
		//~ clusterAddr string
		internal string clusterAddr;

		// physical backend is the un-trusted backend with durable data
		//~ physical physical.Backend
		internal Physical.IBackend physical;

		// Our Seal, for seal configuration information
		//~ seal Seal
		internal ISeal seal;

		// barrier is the security barrier wrapping the physical backend
		//~ barrier SecurityBarrier
		internal ISecurityBarrier barrier;

		// router is responsible for managing the mount points for logical backends.
		//~ router *Router
		internal Router router;

		// logicalBackends is the mapping of backends to use for this core
		//~ logicalBackends map[string]logical.Factory
		internal IDictionary<string, Logical.Factory> logicalBackends;

		// credentialBackends is the mapping of backends to use for this core
		//~ credentialBackends map[string]logical.Factory
		internal IDictionary<string, Logical.Factory> credentialBackends;

		// auditBackends is the mapping of backends to use for this core
		//~ auditBackends map[string]audit.Factory
		internal IDictionary<string, Audit.Factory> auditBackends;

		// stateLock protects mutable state
		//~ stateLock sync.RWMutex
		//~ sealed    bool
		internal ReaderWriterLockSlim stateLock;
		internal bool sealed_;

		//~ standby          bool
		//~ standbyDoneCh    chan struct{}
		//~ standbyStopCh    chan struct{}
		//~ manualStepDownCh chan struct{}
		internal bool standby;
		internal IPropagatorBlock<object, object> standbyDoneCh;
		internal IPropagatorBlock<object, object> standbyStopCh;
		internal IPropagatorBlock<object, object> manualh;


		// unlockInfo has the keys provided to Unseal until the threshold number of parts is available, as well as the operation nonce
		//~ unlockInfo* unlockInformation
		internal UnlockInformation unlockInfo;

		// generateRootProgress holds the shares until we reach enough
		// to verify the master key
		//~ generateRootConfig   *GenerateRootConfig
		//~ generateRootProgress [][]byte
		//~ generateRootLock     sync.Mutex
		internal GenerateRootConfig generateRootConfig;
		internal byte[][] generateRootProgress;
		internal Mutex generateRootLock;

		// These variables holds the config and shares we have until we reach
		// enough to verify the appropriate master key. Note that the same lock is
		// used; this isn't time-critical so this shouldn't be a problem.
		//~ barrierRekeyConfig    *SealConfig
		//~ barrierRekeyProgress  [][]byte
		//~ recoveryRekeyConfig   *SealConfig
		//~ recoveryRekeyProgress [][]byte
		//~ rekeyLock             sync.RWMutex
		internal SealConfig barrierRekeyConfig;
		internal byte[][] barrierRekeyProgress;
		internal SealConfig recoveryRekeyConfig;

		// mounts is loaded after unseal since it is a protected
		// configuration
		//~ mounts *MountTable
		internal MountTable mounts;

		// mountsLock is used to ensure that the mounts table does not
		// change underneath a calling function
		//~ mountsLock sync.RWMutex
		internal ReaderWriterLockSlim mountsLock;

		// auth is loaded after unseal since it is a protected
		// configuration
		//~ auth *MountTable
		internal MountTable auth;

		// authLock is used to ensure that the auth table does not
		// change underneath a calling function
		//~ authLock sync.RWMutex
		internal ReaderWriterLockSlim authLock;

		// audit is loaded after unseal since it is a protected
		// configuration
		//~ audit *MountTable
		internal MountTable audit;

		// auditLock is used to ensure that the audit table does not
		// change underneath a calling function
		//~ auditLock sync.RWMutex
		internal ReaderWriterLockSlim auditLock;

		// auditBroker is used to ingest the audit events and fan
		// out into the configured audit backends
		//~ auditBroker *AuditBroker
		internal AuditBroker auditBroker;

		// auditedHeaders is used to configure which http headers
		// can be output in the audit logs
		//~ auditedHeaders *AuditedHeadersConfig
		internal AuditedHeadersConfig auditedHeaders;

		// systemBarrierView is the barrier view for the system backend
		//~ systemBarrierView *BarrierView
		internal BarrierView systemBarrierView;

		// expiration manager is used for managing LeaseIDs,
		// renewal, expiration and revocation
		//~ expiration *ExpirationManager
		internal ExpirationManager expiration;

		// rollback manager is used to run rollbacks periodically
		//~ rollback *RollbackManager
		internal RollbackManager rollback;

		// policy store is used to manage named ACL policies
		//~ policyStore *PolicyStore
		internal PolicyStore policyStore;

		// token store is used to manage authentication tokens
		//~ tokenStore *TokenStore
		internal TokenStore tokenStore;

		// metricsCh is used to stop the metrics streaming
		//~ metricsCh chan struct{}
		internal IPropagatorBlock<object, object> metricsCh;

		// metricsMutex is used to prevent a race condition between
		// metrics emission and sealing leading to a nil pointer
		//~ metricsMutex sync.Mutex
		internal Mutex metricsMutex;

		//~ defaultLeaseTTL time.Duration
		//~ maxLeaseTTL     time.Duration
		internal TimeSpan defaultLeaseTTL;
		internal TimeSpan maxLeaseTTL;

		//~ logger log.Logger
		internal ILogger logger;

		// cachingDisabled indicates whether caches are disabled
		//~ cachingDisabled bool
		internal bool cachingDisabled;

		// reloadFuncs is a map containing reload functions
		//~ reloadFuncs map[string][]ReloadFunc
		internal IDictionary<string, ReloadFunc[]> reloadFuncs;

		// reloadFuncsLock controlls access to the funcs
		//~ reloadFuncsLock sync.RWMutex
		internal ReaderWriterLockSlim reloadFuncsLock;

		// wrappingJWTKey is the key used for generating JWTs containing response
		// wrapping information
		//~ wrappingJWTKey *ecdsa.PrivateKey
		internal CngKey wrappingJWTKey;

		//
		// Cluster information
		//
		// Name
		//~ clusterName string
		internal string clusterName;
		// Used to modify cluster parameters
		//~ clusterParamsLock sync.RWMutex
		internal ReaderWriterLockSlim clusterParamsLock;
		// The private key stored in the barrier used for establishing
		// mutually-authenticated connections between Vault cluster members
		//~ localClusterPrivateKey crypto.Signer
		internal object localClusterPrivateKey;
		// The local cluster cert
		//~ localClusterCert []byte
		internal byte[] localClusterCert;
		// The cert pool containing trusted cluster CAs
		//~ clusterCertPool *x509.CertPool
		internal X509Certificate2Collection clusterCertPool;
		// The TCP addresses we should use for clustering
		//~ clusterListenerAddrs []*net.TCPAddr
		internal IPAddress[] clusterListenerAddrs;
		// The setup function that gives us the handler to use
		//~ clusterHandlerSetupFunc func() (http.Handler, http.Handler)
		internal Func<object, object> clusterHandlerSetupFunc;
		// Tracks whether cluster listeners are running, e.g. it's safe to send a
		// shutdown down the channel
		//~ clusterListenersRunning bool
		internal bool clusterListenerRunning;
		// Shutdown channel for the cluster listeners
		//~ clusterListenerShutdownCh chan struct{}

		// Shutdown success channel. We need this to be done serially to ensure
		// that binds are removed before they might be reinstated.
		//~ clusterListenerShutdownSuccessCh chan struct{}
		internal IPropagatorBlock<object, object> clusterListenerShutdownSuccessCh;
		// Connection info containing a client and a current active address
		//~ requestForwardingConnection *activeConnection
		internal activeConnection requestForwardingConnection;
		// Write lock used to ensure that we don't have multiple connections adjust
		// this value at the same time
		//~ requestForwardingConnectionLock sync.RWMutex
		internal ReaderWriterLockSlim requestForwardingConnectionLock;
		// Most recent leader UUID. Used to avoid repeatedly JSON parsing the same
		// values.
		//~ clusterLeaderUUID string
		internal string clusterLeaderUUID;
		// Most recent leader redirect addr
		//~ clusterLeaderRedirectAddr string
		internal string clusterLeaderRedirectAddr;
		// The grpc Server that handles server RPC calls
		//~ rpcServer *grpc.Server
		internal Server rpcServer;
		// The function for canceling the client connection
		//~ rpcClientConnCancelFunc context.CancelFunc
		internal CancellationTokenSource rpcClientConnCancelFunc;
		// The grpc ClientConn for RPC calls
		//~ rpcClientConn *grpc.ClientConn
		internal object rpcClientConn;
		// The grpc forwarding client
		//~ rpcForwardingClient RequestForwardingClient
		internal RequestForwardingClient rpcForwardingClient;

		// replicationState keeps the current replication state cached for quick
		// lookup
		//~ replicationState consts.ReplicationState
		// IMPL: based on helper.consts: type ReplicationState uint32
		internal uint replicationState;

		// NewCore is used to construct a new core
		//~ func NewCore(conf *CoreConfig) (*Core, error) {
		public static Core NewCore(CoreConfig conf)
		{
			//~ if conf.HAPhysical != nil && conf.HAPhysical.HAEnabled() {
			//~ 	if conf.RedirectAddr == "" {
			//~ 		return nil, fmt.Errorf("missing redirect address")
			//~ 	}
			//~ }
			if (conf.HAPhysical != null && conf.HAPhysical.HAEnabled())
			{
				if (string.IsNullOrEmpty(conf.RedirectAddr))
					throw new Exception("missing redirect address");

				//~ if conf.DefaultLeaseTTL == 0 {
				//~ 	conf.DefaultLeaseTTL = defaultLeaseTTL
				//~ }
				//~ if conf.MaxLeaseTTL == 0 {
				//~ 	conf.MaxLeaseTTL = maxLeaseTTL
				//~ }
				//~ if conf.DefaultLeaseTTL > conf.MaxLeaseTTL {
				//~ 	return nil, fmt.Errorf("cannot have DefaultLeaseTTL larger than MaxLeaseTTL")
				//~ }
				if (conf.DefaultLeaseTTL == TimeSpan.Zero)
					conf.DefaultLeaseTTL = Constants.defaultLeaseTTL;
				if (conf.MaxLeaseTTL == TimeSpan.Zero)
					conf.MaxLeaseTTL = Constants.maxLeaseTTL;
				if (conf.DefaultLeaseTTL > conf.MaxLeaseTTL)
					throw new Exception("cannot have DefaultLeaseTTL larger than MaxLeaseTTL");

				// Validate the advertise addr if its given to us
				//~ if conf.RedirectAddr != "" {
				if (!string.IsNullOrEmpty(conf.RedirectAddr))
				{
					//~ u, err := url.Parse(conf.RedirectAddr)
					//~ if err != nil {
					//~ 	return nil, fmt.Errorf("redirect address is not valid url: %s", err)
					//~ }
					//~ 
					//~ if u.Scheme == "" {
					//~ 	return nil, fmt.Errorf("redirect address must include scheme (ex. 'http')")
					//~ }
					var u = new Uri(conf.RedirectAddr);
					if (string.IsNullOrEmpty(u.Scheme))
						throw new Exception("redirect address must include scheme (ex. 'http')");
				}

				// Make a default logger if not provided
				//~ if conf.Logger == nil {
				//~ 	conf.Logger = logformat.NewVaultLogger(log.LevelTrace)
				//~ }
				if (conf.Logger == null)
					conf.Logger = VaultLogger.CreateLoggerFactory(LogLevel.Trace).CreateLogger<Core>();

			// Setup the core
			c := &Core{
				redirectAddr:                     conf.RedirectAddr,
				clusterAddr:                      conf.ClusterAddr,
				physical:                         conf.Physical,
				seal:                             conf.Seal,
				router:                           NewRouter(),
				sealed:                           true,
				standby:                          true,
				logger:                           conf.Logger,
				defaultLeaseTTL:                  conf.DefaultLeaseTTL,
				maxLeaseTTL:                      conf.MaxLeaseTTL,
				cachingDisabled:                  conf.DisableCache,
				clusterName:                      conf.ClusterName,
				clusterCertPool:                  x509.NewCertPool(),
				clusterListenerShutdownCh:        make(chan struct{}),
				clusterListenerShutdownSuccessCh: make(chan struct{}),
			}

			// Wrap the physical backend in a cache layer if enabled and not already wrapped
			if _, isCache := conf.Physical.(*physical.Cache); !conf.DisableCache && !isCache {
				c.physical = physical.NewCache(conf.Physical, conf.CacheSize, conf.Logger)
			}

			if !conf.DisableMlock {
				// Ensure our memory usage is locked into physical RAM
				if err := mlock.LockMemory(); err != nil {
					return nil, fmt.Errorf(
						"Failed to lock memory: %v\n\n"+
							"This usually means that the mlock syscall is not available.\n"+
							"Vault uses mlock to prevent memory from being swapped to\n"+
							"disk. This requires root privileges as well as a machine\n"+
							"that supports mlock. Please enable mlock on your system or\n"+
							"disable Vault from using it. To disable Vault from using it,\n"+
							"set the `disable_mlock` configuration option in your configuration\n"+
							"file.",
						err)
				}
			}

			// Construct a new AES-GCM barrier
			var err error
			c.barrier, err = NewAESGCMBarrier(c.physical)
			if err != nil {
				return nil, fmt.Errorf("barrier setup failed: %v", err)
			}

			if conf.HAPhysical != nil && conf.HAPhysical.HAEnabled() {
				c.ha = conf.HAPhysical
			}

			// We create the funcs here, then populate the given config with it so that
			// the caller can share state
			conf.ReloadFuncsLock = &c.reloadFuncsLock
			c.reloadFuncsLock.Lock()
			c.reloadFuncs = make(map[string][]ReloadFunc)
			c.reloadFuncsLock.Unlock()
			conf.ReloadFuncs = &c.reloadFuncs

			// Setup the backends
			logicalBackends := make(map[string]logical.Factory)
			for k, f := range conf.LogicalBackends {
				logicalBackends[k] = f
			}
			_, ok := logicalBackends["generic"]
			if !ok {
				logicalBackends["generic"] = PassthroughBackendFactory
			}
			logicalBackends["cubbyhole"] = CubbyholeBackendFactory
			logicalBackends["system"] = func(config *logical.BackendConfig) (logical.Backend, error) {
				return NewSystemBackend(c, config)
			}
			c.logicalBackends = logicalBackends

			credentialBackends := make(map[string]logical.Factory)
			for k, f := range conf.CredentialBackends {
				credentialBackends[k] = f
			}
			credentialBackends["token"] = func(config *logical.BackendConfig) (logical.Backend, error) {
				return NewTokenStore(c, config)
			}
			c.credentialBackends = credentialBackends

			auditBackends := make(map[string]audit.Factory)
			for k, f := range conf.AuditBackends {
				auditBackends[k] = f
			}
			c.auditBackends = auditBackends

			if c.seal == nil {
				c.seal = &DefaultSeal{}
			}
			c.seal.SetCore(c)

			// Attempt unsealing with stored keys; if there are no stored keys this
			// returns nil, otherwise returns nil or an error
			storedKeyErr := c.UnsealWithStoredKeys()

			return c, storedKeyErr
		}

		// Shutdown is invoked when the Vault instance is about to be terminated. It
		// should not be accessible as part of an API call as it will cause an availability
		// problem. It is only used to gracefully quit in the case of HA so that failover
		// happens as quickly as possible.
		func (c *Core) Shutdown() error {
			c.stateLock.Lock()
			defer c.stateLock.Unlock()
			if c.sealed {
				return nil
			}

			// Seal the Vault, causes a leader stepdown
			return c.sealInternal()
		}
	}

	// CoreConfig is used to parameterize a core
	//~ type CoreConfig struct {
	public class CoreConfig
	{
		//~ DevToken string `json:"dev_token" structs:"dev_token" mapstructure:"dev_token"`
		[JsonProperty("dev_token")]
		public string DevToken
		{ get; set; }

		//~ LogicalBackends map[string]logical.Factory `json:"logical_backends" structs:"logical_backends" mapstructure:"logical_backends"`
		//~
		//~ CredentialBackends map[string]logical.Factory `json:"credential_backends" structs:"credential_backends" mapstructure:"credential_backends"`
		//~ 
		//~ AuditBackends map[string]audit.Factory `json:"audit_backends" structs:"audit_backends" mapstructure:"audit_backends"`
		//~ 
		//~ Physical physical.Backend `json:"physical" structs:"physical" mapstructure:"physical"`
		//~ 
		//~ // May be nil, which disables HA operations
		//~ HAPhysical physical.HABackend `json:"ha_physical" structs:"ha_physical" mapstructure:"ha_physical"`
		//~ 
		//~ Seal Seal `json:"seal" structs:"seal" mapstructure:"seal"`
		//~ 
		//~ Logger log.Logger `json:"logger" structs:"logger" mapstructure:"logger"`
		//~ 
		//~ // Disables the LRU cache on the physical backend
		//~ DisableCache bool `json:"disable_cache" structs:"disable_cache" mapstructure:"disable_cache"`
		//~ 
		//~ // Disables mlock syscall
		//~ DisableMlock bool `json:"disable_mlock" structs:"disable_mlock" mapstructure:"disable_mlock"`
		//~ 
		//~ // Custom cache size for the LRU cache on the physical backend, or zero for default
		//~ CacheSize int `json:"cache_size" structs:"cache_size" mapstructure:"cache_size"`
		//~ 
		//~ // Set as the leader address for HA
		//~ RedirectAddr string `json:"redirect_addr" structs:"redirect_addr" mapstructure:"redirect_addr"`
		//~ 
		//~ // Set as the cluster address for HA
		//~ ClusterAddr string `json:"cluster_addr" structs:"cluster_addr" mapstructure:"cluster_addr"`
		//~ 
		//~ DefaultLeaseTTL time.Duration `json:"default_lease_ttl" structs:"default_lease_ttl" mapstructure:"default_lease_ttl"`
		//~ 
		//~ MaxLeaseTTL time.Duration `json:"max_lease_ttl" structs:"max_lease_ttl" mapstructure:"max_lease_ttl"`
		//~ 
		//~ ClusterName string `json:"cluster_name" structs:"cluster_name" mapstructure:"cluster_name"`
		//~ 
		//~ ReloadFuncs     *map[string][]ReloadFunc
		//~ ReloadFuncsLock *sync.RWMutex
		[JsonProperty("logical_backends")]
		public IDictionary<string, Logical.Factory> LogicalBackends
		{ get; set; }
		[JsonProperty("credentials_backends")]
		public IDictionary<string, Logical.Factory> CredentialBackends
		{ get; set; }
		[JsonProperty("audit_backends")]
		public IDictionary<string, Audit.Factory  > AuditBackends
		{ get; set; }
		[JsonProperty("physical")]
		public Physical.IBackend Physical
		{ get; set; }
		[JsonProperty("ha_physical")]
		public Physical.IHABackend HAPhysical
		{ get; set; }
		[JsonProperty("seal")]
		public ISeal Seal
		{ get; set; }
		[JsonProperty("logger")]
		public ILogger Logger
		{ get; set; }
		[JsonProperty("disable_cache")]
		public bool DisableCache
		{ get; set; }
		[JsonProperty("disable_mlock")]
		public bool DisableMlock
		{ get; set; }
		[JsonProperty("cache_size")]
		public int CacheSize
		{ get; set; }
		[JsonProperty("redirect_addr")]
		public string RedirectAddr
		{ get; set; }
		[JsonProperty("cluster_addr")]
		public string ClusterAddr
		{ get; set; }
		[JsonProperty("default_lease_ttl")]
		public TimeSpan DefaultLeaseTTL
		{ get; set; }
		[JsonProperty("max_lease_ttl")]
		public TimeSpan MaxLeaseTTL
		{ get; set; }
		[JsonProperty("cluster_name")]
		public string ClusterName
		{ get; set; }

		public IDictionary<string, ReloadFunc[]> ReloadFuncs
		{ get; set; }

		public ReaderWriterLockSlim ReloadFuncsLock
		{ get; set; }


	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Zyborg.Vault.Physical
{
    // // ShutdownSignal
    //~ type ShutdownChannel chan struct{}
    //
    // Replaced with BufferBlock<object> down below

    // Backend is the interface required for a physical
    // backend. A physical backend is used to durably store
    // data outside of Vault. As such, it is completely untrusted,
    // and is only accessed via a security barrier. The backends
    // must represent keys in a hierarchical manner. All methods
    // are expected to be thread safe.
    public interface IBackend
    {
        // Put is used to insert or update an entry
        void Put(Entry entry);

        // Get is used to fetch an entry
        Entry Get(string key);

        // Delete is used to permanently delete an entry
        void Delete(string key);

        // List is used ot list all the keys under a given
        // prefix, up to the next prefix.
        IEnumerable<string> List(string prefix);
    }
    
    // HABackend is an extensions to the standard physical
    // backend to support high-availability. Vault only expects to
    // use mutual exclusion to allow multiple instances to act as a
    // hot standby for a leader that services all requests.
    public interface IHABackend : IBackend
    {
        // LockWith is used for mutual exclusion based on the given key.
        Lock LockWith(string key, string value);

        // Whether or not HA functionality is enabled
        bool IsHAEnabled();
    }

    // Purgable is an optional interface for backends that support
    // purging of their caches.
    public interface IPurgable
    {
        void Purge();
    }

    // RedirectDetect is an optional interface that an HABackend
    // can implement. If they do, a redirect address can be automatically
    // detected.
    public interface IRedirectDetect
    {
        // DetectHostAddr is used to detect the host address
        string DetectHostAddr();
    }

    // Callback signatures for RunServiceDiscovery
    public delegate bool ActiveFunc();
    public delegate bool SealedFunc();

    // ServiceDiscovery is an optional interface that an HABackend can implement.
    // If they do, the state of a backend is advertised to the service discovery
    // network.
    public interface IServiceDiscovery
    {
        // NotifyActiveStateChange is used by Core to notify a backend
        // capable of ServiceDiscovery that this Vault instance has changed
        // its status to active or standby.
        void NotifyActiveStateChange();

        // NotifySealedStateChange is used by Core to notify a backend
        // capable of ServiceDiscovery that Vault has changed its Sealed
        // status to sealed or unsealed.
        void NotifySealedStateChange();

        // Run executes any background service discovery tasks until the
        // shutdown channel is closed.
        void RunServiceDiscovery(/*sync.WaitGroup*/ Barrier waitGroup,
                BufferBlock<object> shutdownChan, string redirectAddr,
                ActiveFunc activeFunc, SealedFunc sealedFunc);
    }

    public interface Lock
    {
        // // Lock is used to acquire the given lock
        // // The stopCh is optional and if closed should interrupt the lock
        // // acquisition attempt. The return struct should be closed when
        // // leadership is lost.
        // // stopCh: receive-only channel
        // // returns: receive-only channel
        // Lock(stopCh <-chan struct{}) (<-chan struct{}, error)

        ISourceBlock<object> Lock(ISourceBlock<object> stopCh);

        // Unlock is used to release the lock
        void Unlock();

        // Returns the value of the lock and if it is held
        Tuple<bool, string> Value();
    }

    // Entry is used to represent data stored by the physical backend
    public class Entry
    {
        [JsonProperty("key")]
        public string Key
        { get; set; }

        [JsonProperty("value")]
        public byte[] Value
        { get; set; }
    }

    // Factory is the factory function to create a physical backend.
    //type Factory func(config map[string]string, logger log.Logger) (Backend, error)
    public interface IBackendFactory
    {
        IBackend CreateBackend(IReadOnlyDictionary<string, string> config, ILogger logger);
    }

    // PermitPool is used to limit maximum outstanding requests
    // type PermitPool struct {
    //     sem chan int
    // }
    public class PermitPool
    {
        private BufferBlock<int> _sem;

        // NewPermitPool returns a new permit pool with the provided
        // number of permits
        // func NewPermitPool(permits int) *PermitPool {
        //     if permits < 1 {
        //         permits = DefaultParallelOperations
        //     }
        //     return &PermitPool{
        //         sem: make(chan int, permits),
        //     }
        // }
        public PermitPool(int permits)
        {
            if (permits < 1)
                permits = Constants.DefaultParallelOperations;

            _sem = new BufferBlock<int>(new DataflowBlockOptions
            {
                BoundedCapacity = permits,
            });
        }

        // Acquire returns when a permit has been acquired
        // func (c *PermitPool) Acquire() {
        //     c.sem <- 1
        // }
        public async Task Acquire()
        {
            await _sem.SendAsync(1);
        }

        // Release returns a permit to the pool
        // func (c *PermitPool) Release() {
        //     <-c.sem
        // }
        public async Task Release()
        {
            await _sem.ReceiveAsync();
        }
    }
}
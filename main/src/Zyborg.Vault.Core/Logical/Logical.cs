using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Logical
{
	// Backend interface must be implemented to be "mountable" at
	// a given path. Requests flow through a router which has various mount
	// points that flow to a logical backend. The logic of each backend is flexible,
	// and this is what allows materialized keys to function. There can be specialized
	// logical backends for various upstreams (Consul, PostgreSQL, MySQL, etc) that can
	// interact with remote APIs to generate keys dynamically. This interface also
	// allows for a "procfs" like interaction, as internal state can be exposed by
	// acting like a logical backend and being mounted.
	//~ type Backend interface {
	public interface IBackend
	{
		// HandleRequest is used to handle a request and generate a response.
		// The backends must check the operation type and handle appropriately.
		//~ HandleRequest(*Request) (*Response, error)
		Response HandleRequest(Request requ);

		// SpecialPaths is a list of paths that are special in some way.
		// See PathType for the types of special paths. The key is the type
		// of the special path, and the value is a list of paths for this type.
		// This is not a regular expression but is an exact match. If the path
		// ends in '*' then it is a prefix-based match. The '*' can only appear
		// at the end.
		//~ SpecialPaths() *Paths
		Paths SpecialPaths();

		// System provides an interface to access certain system configuration
		// information, such as globally configured default and max lease TTLs.
		//~ System() SystemView
		ISystemView System();

		// HandleExistenceCheck is used to handle a request and generate a response
		// indicating whether the given path exists or not; this is used to
		// understand whether the request must have a Create or Update capability
		// ACL applied. The first bool indicates whether an existence check
		// function was found for the backend; the second indicates whether, if an
		// existence check function was found, the item exists or not.
		//~ HandleExistenceCheck(*Request) (bool, bool, error)
		(bool fnFound, bool itemExists) HandleExistenceCheck(Request requ);

		// Cleanup is invoked during an unmount of a backend to allow it to
		// handle any cleanup like connection closing or releasing of file handles.
		//~ Cleanup()
		void Cleanup();

		// Initialize is invoked after a backend is created. It is the place to run
		// any operations requiring storage; these should not be in the factory.
		//~ Initialize() error
		void Initialize();

		// InvalidateKey may be invoked when an object is modified that belongs
		// to the backend. The backend can use this to clear any caches or reset
		// internal state as needed.
		//~ InvalidateKey(key string)
		void InvalidateKey(string key);
	}

	// BackendConfig is provided to the factory to initialize the backend
	//~ type BackendConfig struct {
	public class BackendConfig
	{
		// View should not be stored, and should only be used for initialization
		//~ StorageView Storage
		public IStorage StorageView;

		// The backend should use this logger. The log should not contain any secrets.
		//~ Logger log.Logger
		public ILogger Logger;

		// System provides a view into a subset of safe system information that
		// is useful for backends, such as the default/max lease TTLs
		//~ System SystemView
		public ISystemView System;

		// Config is the opaque user configuration provided when mounting
		//~ Config map[string]string
		public ConfigMap<string> Config;
	}

	// Factory is the factory function to create a logical backend.
	//~ type Factory func(*BackendConfig) (Backend, error)
	public delegate IBackend Factory(BackendConfig config);

	// Paths is the structure of special paths that is used for SpecialPaths.
	//~ type Paths struct {
	public class Paths
	{
		// Root are the paths that require a root token to access
		//~ Root []string
		string[] _root;

		// Unauthenticated are the paths that can be accessed without any auth.
		//~ Unauthenticated []string
		string[] _unauthenticated;

		// LocalStorage are paths (prefixes) that are local to this instance; this
		// indicates that these paths should not be replicated
		//~ LocalStorage []string
		string[] _localStorage;
	}

	//~ type ReplicationState uint32

	//~ const (
	//~ 	ReplicationDisabled ReplicationState = iota
	//~ 	ReplicationPrimary
	//~ 	ReplicationSecondary
	//~ )
	//~ 
	//~ func (r ReplicationState) String() string {
	//~ 	switch r {
	//~ 	case ReplicationSecondary:
	//~ 		return "secondary"
	//~ 	case ReplicationPrimary:
	//~ 		return "primary"
	//~ 	}
	//~ 
	//~ 	return "disabled"
	//~ }

	public enum ReplicationState
	{
		ReplicationDisabled,
		ReplicationPrimary,
		ReplicationSecondary
	}
}

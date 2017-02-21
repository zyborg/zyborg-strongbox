using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Logical
{
	// RequestWrapInfo is a struct that stores information about desired response
	// wrapping behavior
	//~ type RequestWrapInfo struct {
	public class RequestWrapInfo
	{
		// Setting to non-zero specifies that the response should be wrapped.
		// Specifies the desired TTL of the wrapping token.
		//~ TTL time.Duration `json:"ttl" structs:"ttl" mapstructure:"ttl"`
		[JsonProperty("ttl")]
		public TimeSpan TTL
		{ get; set; }

		// The format to use for the wrapped response; if not specified it's a bare
		// token
		//~ Format string `json:"format" structs:"format" mapstructure:"format"`
		[JsonProperty("format")]
		public string Format
		{ get; set; }

		public RequestWrapInfo DeepCopy()
		{
			return new RequestWrapInfo
			{
				TTL = this.TTL,
				Format = this.Format,
			};
		}
	}

	// Request is a struct that stores the parameters and context
	// of a request being made to Vault. It is used to abstract
	// the details of the higher level request protocol from the handlers.
	//~ type Request struct {
	public class Request
	{
		// Id is the uuid associated with each request
		//~ ID string `json:"id" structs:"id" mapstructure:"id"`
		[JsonProperty("id")]
		public string ID
		{ get; set; }

		// If set, the name given to the replication secondary where this request
		// originated
		//~ ReplicationCluster string `json:"replication_cluster" structs:"replication_cluster", mapstructure:"replication_cluster"`
		[JsonProperty("replication_cluster")]
		public string ReplicationCluster
		{ get; set; }

		// Operation is the requested operation type
		//~ Operation Operation `json:"operation" structs:"operation" mapstructure:"operation"`
		[JsonProperty("Operation")]
		public Operation Operation
		{ get; set; }

		// Path is the part of the request path not consumed by the
		// routing. As an example, if the original request path is "prod/aws/foo"
		// and the AWS logical backend is mounted at "prod/aws/", then the
		// final path is "foo" since the mount prefix is trimmed.
		//~ Path string `json:"path" structs:"path" mapstructure:"path"`
		[JsonProperty("Path")]
		public string Path
		{ get; set; }

		// Request data is an opaque map that must have string keys.
		//~ Data map[string]interface{} `json:"map" structs:"data" mapstructure:"data"`
		[JsonProperty("map")]
		public IDictionary<string, object> Data
		{ get; set; }

		// Storage can be used to durably store and retrieve state.
		//~ Storage Storage `json:"storage" structs:"storage" mapstructure:"storage"`
		[JsonProperty("storage")]
		public IStorage Storage
		{ get; set; }

		// Secret will be non-nil only for Revoke and Renew operations
		// to represent the secret that was returned prior.
		//~ Secret* Secret `json:"secret" structs:"secret" mapstructure:"secret"`
		[JsonProperty("secret")]
		public Secret Secret
		{ get; set; }

		// Auth will be non-nil only for Renew operations
		// to represent the auth that was returned prior.
		//~ Auth *Auth `json:"auth" structs:"auth" mapstructure:"auth"`
		[JsonProperty("auth")]
		public Auth Auth
		{ get; set; }


		// Headers will contain the http headers from the request. This value will
		// be used in the audit broker to ensure we are auditing only the allowed
		// headers.
		//~ Headers map[string][]string `json:"headers" structs:"headers" mapstructure:"headers"`
		[JsonProperty("headers")]
		public IDictionary<string, string[]> Headers
		{ get; set; }


		// Connection will be non-nil only for credential providers to
		// inspect the connection information and potentially use it for
		// authentication/protection.
		//~ Connection* Connection `json:"connection" structs:"connection" mapstructure:"connection"`
		[JsonProperty("connection")]
		public Connection Connection
		{ get; set; }

		// ClientToken is provided to the core so that the identity
		// can be verified and ACLs applied. This value is passed
		// through to the logical backends but after being salted and
		// hashed.
		//~ ClientToken string `json:"client_token" structs:"client_token" mapstructure:"client_token"`
		[JsonProperty("client_token")]
		public string ClientToken
		{ get; set; }

		// ClientTokenAccessor is provided to the core so that the it can get
		// logged as part of request audit logging.
		//~ ClientTokenAccessor string `json:"client_token_accessor" structs:"client_token_accessor" mapstructure:"client_token_accessor"`
		[JsonProperty("client_token_accessor")]
		public string ClientTokenAccessor
		{ get; set; }

		// DisplayName is provided to the logical backend to help associate
		// dynamic secrets with the source entity. This is not a sensitive
		// name, but is useful for operators.
		//~ DisplayName string `json:"display_name" structs:"display_name" mapstructure:"display_name"`
		[JsonProperty("display_name")]
		public string DisplayName
		{ get; set; }

		// MountPoint is provided so that a logical backend can generate
		// paths relative to itself. The `Path` is effectively the client
		// request path with the MountPoint trimmed off.
		//~ MountPoint string `json:"mount_point" structs:"mount_point" mapstructure:"mount_point"`
		[JsonProperty("mount_point")]
		public string MountPoint
		{ get; set; }

		// WrapInfo contains requested response wrapping parameters
		//~ WrapInfo* RequestWrapInfo `json:"wrap_info" structs:"wrap_info" mapstructure:"wrap_info"`
		[JsonProperty("wrap_info")]
		public RequestWrapInfo WrapInfo
		{ get; set; }

		// Get returns a data field and guards for nil Data
		//~ func (r *Request) Get(key string) interface{} {
		public object Get(string key)
		{
			//~ if r.Data == nil {
			//~ 	return nil
			//~ }
			//~ return r.Data[key]
			return Data?[key];
		}

		// GetString returns a data field as a string
		//~ func (r *Request) GetString(key string) string {
		public string GetString(string key)
		{
			//~ raw := r.Get(key)
			//~ s, _ := raw.(string)
			//~ return s
			return Get(key) as string; // TODO: should we do a ToString() if it's not a string???
		}

		//~ func (r *Request) GoString() string {
		public string GoString()
		{
			//~ return fmt.Sprintf("*%#v", *r)
			return $"*{this}"; // TODO:  Not really the same thing:  https://gobyexample.com/string-formatting
		}

		// RenewRequest creates the structure of the renew request.
		//~ func RenewRequest(
		//~         path string, secret* Secret, data map[string] interface{ }) *Request {
		public static Request RenewRequest(string path, Secret secret, IDictionary<string, object> data)
		{
			//~ return &Request{
			//~ 	Operation: RenewOperation,
			//~ 	Path:      path,
			//~ 	Data:      data,
			//~ 	Secret:    secret,
			//~ }
			return new Request
			{
				Operation = Operation.RenewOperation,
				Path = path,
				Data = data,
				Secret = secret,
			};
		}

		// RenewAuthRequest creates the structure of the renew request for an auth.
		//~ func RenewAuthRequest(
		//~ 	path string, auth *Auth, data map[string]interface{}) *Request {
		public static Request RenewAuthRequest(string path, Auth auth, IDictionary<string, object> data)
		{
			//~ return &Request{
			//~ 	Operation: RenewOperation,
			//~ 	Path:      path,
			//~ 	Data:      data,
			//~ 	Auth:      auth,
			//~ }
			return new Request
			{
				Operation = Operation.RenewOperation,
				Path = path,
				Data = data,
				Auth = auth,
			};
		}

		// RevokeRequest creates the structure of the revoke request.
		//~ func RevokeRequest(
		//~ 	path string, secret *Secret, data map[string]interface{}) *Request {
		public static Request RevokeRequest(string path, Secret secret, IDictionary<string, object> data)
		{
			//~ return &Request{
			//~ 	Operation: RevokeOperation,
			//~ 	Path:      path,
			//~ 	Data:      data,
			//~ 	Secret:    secret,
			//~ }
			return new Request
			{
				Operation = Operation.RevokeOperation,
				Path = path,
				Data = data,
				Secret = secret,
			};
		}

		// RollbackRequest creates the structure of the revoke request.
		//~ func RollbackRequest(path string) *Request {
		public static Request RollbackRequest(string path)
		{
			//return &Request{
			//	Operation: RollbackOperation,
			//	Path:      path,
			//	Data:      make(map[string]interface{}),
			//}
			return new Request
			{
				Operation = Operation.RollbackOperation,
				Path = path,
				Data = new Dictionary<string, object>(),
			};
		}

		public Request DeepCopy()
		{
			return new Request
			{
				Auth = this.Auth?.DeepCopy(),
				ClientToken = this.ClientToken,
				ClientTokenAccessor = this.ClientTokenAccessor,
				Connection = this.Connection?.DeepCopy(),
				Data = this.Data?.DeepCopy(),
				DisplayName = this.DisplayName,
				Headers = this.Headers?.DeepCopy(),
				ID = this.ID,
				MountPoint = this.MountPoint,
				Operation = this.Operation,
				Path = this.Path,
				Secret = this.Secret?.DeepCopy(),
				Storage = this.Storage?.DeepCopy(),
				WrapInfo = this.WrapInfo?.DeepCopy(),
			};
		}
	}

	// Operation is an enum that is used to specify the type
	// of request being made
	//~ type Operation string
	//~ 
	//~ const (
	//~ 	// The operations below are called per path
	//~ 	CreateOperation Operation = "create"
	//~ 	ReadOperation             = "read"
	//~ 	UpdateOperation           = "update"
	//~ 	DeleteOperation           = "delete"
	//~ 	ListOperation             = "list"
	//~ 	HelpOperation             = "help"
	//~ 
	//~ 	// The operations below are called globally, the path is less relevant.
	//~ 	RevokeOperation   Operation = "revoke"
	//~ 	RenewOperation              = "renew"
	//~ 	RollbackOperation           = "rollback"
	//~ )
	public sealed class Operation
	{
		public static readonly Operation CreateOperation = new Operation("create");
		public static readonly Operation ReadOperation = new Operation("read");
		public static readonly Operation DeleteOperation = new Operation("delete");
		public static readonly Operation UpdateOperation = new Operation("update");
		public static readonly Operation ListOperation = new Operation("list");
		public static readonly Operation HelpOperation = new Operation("help");

		public static readonly Operation RevokeOperation = new Operation("revoke");
		public static readonly Operation RenewOperation = new Operation("renew");
		public static readonly Operation RollbackOperation = new Operation("rollback");

		private Operation(string name) => Name = name;

		public string Name
		{ get; }
	}

	//~ var (
	//~ 	// ErrUnsupportedOperation is returned if the operation is not supported
	//~ 	// by the logical backend.
	//~ 	ErrUnsupportedOperation = errors.New("unsupported operation")
	//~ 
	//~ 	// ErrUnsupportedPath is returned if the path is not supported
	//~ 	// by the logical backend.
	//~ 	ErrUnsupportedPath = errors.New("unsupported path")
	//~ 
	//~ 	// ErrInvalidRequest is returned if the request is invalid
	//~ 	ErrInvalidRequest = errors.New("invalid request")
	//~ 
	//~ 	// ErrPermissionDenied is returned if the client is not authorized
	//~ 	ErrPermissionDenied = errors.New("permission denied")
	//~ )
	public class ErrUnsupportedOperation : Exception
	{
		public ErrUnsupportedOperation()
			: base("unsupported operation")
		{ }
	}
	public class ErrUnsupportedPath : Exception
	{
		public ErrUnsupportedPath()
			: base("unsupported path")
		{ }
	}
	public class ErrInvalidRequest : Exception
	{
		public ErrInvalidRequest()
			: base("invalid request")
		{ }
	}
	public class ErrPermissionDenied : Exception
	{
		public ErrPermissionDenied()
			: base("permission denied")
		{ }
	}
}

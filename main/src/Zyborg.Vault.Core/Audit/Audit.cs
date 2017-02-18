using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Vault.Helper.Salt;

namespace Zyborg.Vault.Audit
{
	// Backend interface must be implemented for an audit
	// mechanism to be made available. Audit backends can be enabled to
	// sink information to different backends such as logs, file, databases,
	// or other external services.
	//~ type Backend interface {
	public interface IBackend
	{
		// LogRequest is used to synchronously log a request. This is done after the
		// request is authorized but before the request is executed. The arguments
		// MUST not be modified in anyway. They should be deep copied if this is
		// a possibility.
		//~ LogRequest(*logical.Auth, *logical.Request, error) error
		void LogRequest(Logical.Auth auth, Logical.Request requ, Exception error);

		// LogResponse is used to synchronously log a response. This is done after
		// the request is processed but before the response is sent. The arguments
		// MUST not be modified in anyway. They should be deep copied if this is
		// a possibility.
		//~ LogResponse(*logical.Auth, *logical.Request, *logical.Response, error) error
		void LogResponse(Logical.Auth auth, Logical.Request requ, Logical.Response resp, Exception error);

		// GetHash is used to return the given data with the backend's hash,
		// so that a caller can determine if a value in the audit log matches
		// an expected plaintext value
		//~ GetHash(string) string
		string GetHash(string data);

		// Reload is called on SIGHUP for supporting backends.
		//~ Reload() error
		void Reload();
	}

	//~ type BackendConfig struct {
	public class BackendConfig
	{
		// The salt that should be used for any secret obfuscation
		//~ Salt *salt.Salt
		public Salt Salt
		{ get; set; }

		// Config is the opaque user configuration provided when mounting
		//~ Config map[string]string
		public IDictionary<string, string> Config
		{ get; set; }
	}

	// Factory is the factory function to create an audit backend.
	//~ type Factory func(*BackendConfig) (Backend, error)
	public delegate IBackend Factory(BackendConfig config);
}

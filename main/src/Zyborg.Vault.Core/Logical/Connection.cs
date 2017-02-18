using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	// Connection represents the connection information for a request. This
	// is present on the Request structure for credential backends.
	//~ type Connection struct {
	public class Connection
	{
		// RemoteAddr is the network address that sent the request.
		//~ RemoteAddr string
		public string RemoteAddr
		{ get; set; }

		// ConnState is the TLS connection state if applicable.
		// ConnState *tls.ConnectionState
		// TODO: we may have to piece something together
		// from System.Net.Security and System.Security.*
		// based on how this is used
		public object ConnectionState
		{ get; set; }

		public Connection DeepCopy()
		{
			return new Connection
			{
				RemoteAddr = this.RemoteAddr,
				ConnectionState = this.ConnectionState, // TODO: this may need to be adjusted
			};
		}
	}
}

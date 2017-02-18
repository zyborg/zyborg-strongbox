using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyborg.Vault.Helper.Salt;

namespace Zyborg.Vault.Audit
{
	// Formatter is an interface that is responsible for formating a
	// request/response into some format. Formatters write their output
	// to an io.Writer.
	//
	// It is recommended that you pass data through Hash prior to formatting it.
	//~ type Formatter interface {
	public interface IFormatter
	{
		//~ FormatRequest(io.Writer, FormatterConfig, *logical.Auth, *logical.Request, error) error
		//~ FormatResponse(io.Writer, FormatterConfig, *logical.Auth, *logical.Request, *logical.Response, error) error
		void FormatRequest(Stream s, FormatterConfig config, Logical.Auth auth,
				Logical.Request req, Exception error);
		void FormatResponse(Stream w, FormatterConfig config, Logical.Auth auth,
				Logical.Request req, Logical.Response resp, Exception error);
	}

	//~ type FormatterConfig struct {
	public class FormatterConfig
	{
		//~ Raw          bool
		//~ Salt         *salt.Salt
		//~ HMACAccessor bool
		public bool Raw
		{ get; set; }
		public Salt Salt
		{ get; set; }
		public bool HMACAccessor
		{ get; set; }

		// This should only ever be used in a testing context
		//~ OmitTime bool
		public bool OmitTime
		{ get; set; }
	}
}

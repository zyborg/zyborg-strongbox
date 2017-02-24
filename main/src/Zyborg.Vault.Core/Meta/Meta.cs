using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util.CLI;
using Zyborg.Vault.API;
using Zyborg.Vault.Command.Token;

namespace Zyborg.Vault.Meta
{
	//~ type TokenHelperFunc func() (token.TokenHelper, error)
	public delegate ITokenHelper TokenHelperFunc();

	public class Meta
    {
		// FlagSetFlags is an enum to define what flags are present in the
		// default FlagSet returned by Meta.FlagSet.
		//~ type FlagSetFlags uint
		//~ 
		//~ type TokenHelperFunc func() (token.TokenHelper, error)
		//~ 
		//~ const (
		//~ 	FlagSetNone    FlagSetFlags = 0
		//~ 	FlagSetServer  FlagSetFlags = 1 << iota
		//~ 	FlagSetDefault              = FlagSetServer
		//~ )
		public enum FlagSetFlags
		{
			FlagSetNone,
			FlagSetServer,
			FlagSetDefault = FlagSetServer,
		}

		//~var (
		//~	additionalOptionsUsage = func() string {
		//~		return `
		//~  -wrap-ttl=""            Indicates that the response should be wrapped in a
		//~						  cubbyhole token with the requested TTL. The response
		//~						  can be fetched by calling the "sys/wrapping/unwrap"
		//~						  endpoint, passing in the wrappping token's ID. This
		//~						  is a numeric string with an optional suffix
		//~						  "s", "m", or "h"; if no suffix is specified it will
		//~						  be parsed as seconds. May also be specified via
		//~						  VAULT_WRAP_TTL.
		//~`
		//~	}
		//~)
		private static readonly Func<string> additionalOptionsUsage = () => @"
  -wrap-ttl=""""            Indicates that the response should be wrapped in a
						  cubbyhole token with the requested TTL. The response
						  can be fetched by calling the ""sys/wrapping/unwrap""
						  endpoint, passing in the wrappping token's ID. This
						  is a numeric string with an optional suffix
						  ""s"", ""m"", or ""h""; if no suffix is specified it will
						  be parsed as seconds. May also be specified via
						  VAULT_WRAP_TTL.
";

		// Meta contains the meta-options and functionality that nearly every
		// Vault command inherits.
		//~ type Meta struct {
		//~ 	ClientToken string
		//~ 	Ui          cli.Ui
		//~ 
		//~ 	// The things below can be set, but aren't common
		//~ 	ForceAddress string // Address to force for API clients
		//~ 
		//~ 	// These are set by the command line flags.
		//~ 	flagAddress    string
		//~ 	flagCACert     string
		//~ 	flagCAPath     string
		//~ 	flagClientCert string
		//~ 	flagClientKey  string
		//~ 	flagWrapTTL    string
		//~ 	flagInsecure   bool
		//~ 
		//~ 	// Queried if no token can be found
		//~ 	TokenHelper TokenHelperFunc
		//~ }
		public string ClientToken
		{ get; set; }

		public Ui UI
		{ get; set; }

		public string ForceAddress
		{ get; set; }

		string	_flagAddress   ;
		string	_flagCACert    ;
		string	_flagCAPath    ;
		string	_flagClientCert;
		string	_flagClientKey ;
		string	_flagWrapTTL   ;
		bool	_flagInsecure  ;

		public TokenHelperFunc TokenHelper
		{ get; set; }


		//~ func(m* Meta) DefaultWrappingLookupFunc(operation, path string) string {
		public string DefaultWrappingLookupFunc(string operation, string path)
		{
			//~ if m.flagWrapTTL != "" {
			//~ 	return m.flagWrapTTL
			//~ }
			if (!string.IsNullOrEmpty(_flagWrapTTL))
				return _flagWrapTTL;

			//~ return api.DefaultWrappingLookupFunc(operation, path)
			return DefaultWrappingLookupFunc(operation, path);
		}

		// Client returns the API client to a Vault server given the configured
		// flag settings for this command.
		//~ func (m *Meta) Client() (*api.Client, error) {
		public Client Client()
		{
			//~ config := api.DefaultConfig()
			var config = Config.DefaultConfig();

			//~ err := config.ReadEnvironment()
			//~ if err != nil {
			//~ 	return nil, errwrap.Wrapf("error reading environment: {{err}}", err)
			//~ }
			config.ReadEnvironment();

			//~ if m.flagAddress != "" {
			//~ 	config.Address = m.flagAddress
			//~ }
			//~ if m.ForceAddress != "" {
			//~ 	config.Address = m.ForceAddress
			//~ }
			//~ // If we need custom TLS configuration, then set it
			//~ if m.flagCACert != "" || m.flagCAPath != "" || m.flagClientCert != "" || m.flagClientKey != "" || m.flagInsecure {
			//~ 	t := &api.TLSConfig{
			//~ 		CACert:        m.flagCACert,
			//~ 		CAPath:        m.flagCAPath,
			//~ 		ClientCert:    m.flagClientCert,
			//~ 		ClientKey:     m.flagClientKey,
			//~ 		TLSServerName: "",
			//~ 		Insecure:      m.flagInsecure,
			//~ 	}
			//~ 	config.ConfigureTLS(t)
			//~ }
			if (!string.IsNullOrEmpty(_flagAddress))
				config.Address = _flagAddress;
			if (!string.IsNullOrEmpty(ForceAddress))
				config.Address = ForceAddress;
			if (!string.IsNullOrEmpty(_flagCACert) || !string.IsNullOrEmpty(_flagCAPath)
					|| !string.IsNullOrEmpty(_flagClientCert) || !string.IsNullOrEmpty(_flagClientKey)
					|| _flagInsecure)
			{
				var t = new API.TLSConfig
				{
					CACert = _flagCACert,
					CAPath = _flagCAPath,
					ClientCert = _flagClientCert,
					ClientKey = _flagClientKey,
					TLSServerName = "",
					Insecure = _flagInsecure,
				};
				config.ConfigureTLS(t);
			}

			// Build the client
			//~ client, err := api.NewClient(config)
			//~ if err != nil {
			//~ 	return nil, err
			//~ }
			var client = API.Client.NewClient(config);

			//~ client.SetWrappingLookupFunc(m.DefaultWrappingLookupFunc)
			client.SetWrappingLookupFunc(DefaultWrappingLookupFunc);

			// If we have a token directly, then set that
			//~ token := m.ClientToken
			var token = ClientToken;

			// Try to set the token to what is already stored
			//~ if token == "" {
			//~ 	token = client.Token()
			//~ }
			if (string.IsNullOrEmpty(token))
				token = client.Token();

			// If we don't have a token, check the token helper
			//~ if token == "" {
			//~ 	if m.TokenHelper != nil {
			//~ 		// If we have a token, then set that
			//~ 		tokenHelper, err := m.TokenHelper()
			//~ 		if err != nil {
			//~ 			return nil, err
			//~ 		}
			//~ 		token, err = tokenHelper.Get()
			//~ 		if err != nil {
			//~ 			return nil, err
			//~ 		}
			//~ 	}
			//~ }
			if (string.IsNullOrEmpty(token))
			{
				if (TokenHelper != null)
				{
					token = TokenHelper().Get();
				}
			}

			// Set the token
			//~ if token != "" {
			//~ 	client.SetToken(token)
			//~ }
			if (!string.IsNullOrEmpty(token))
				client.SetToken(token);

			//~ return client, nil
			return client;
		}
	}
}

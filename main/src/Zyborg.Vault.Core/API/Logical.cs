using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.API
{
    public class Logical
    {
		//~ const (
		//~ 	wrappedResponseLocation = "cubbyhole/response"
		//~ )
		const string WrappedResponseLocation = "cubbyhole/response";

		//~ var (
		// The default TTL that will be used with `sys/wrapping/wrap`, can be
		// changed
		//~ DefaultWrappingTTL = "5m"
		public const string DefaultWrappingTTL = "5m";

		// The default function used if no other function is set, which honors the
		// env var and wraps `sys/wrapping/wrap`
		//~ DefaultWrappingLookupFunc = func(operation, path string) string {
		//~ 	if os.Getenv(EnvVaultWrapTTL) != "" {
		//~ 		return os.Getenv(EnvVaultWrapTTL)
		//~ 	}
		//~ 
		//~ 	if (operation == "PUT" || operation == "POST") && path == "sys/wrapping/wrap" {
		//~ 		return DefaultWrappingTTL
		//~ 	}
		//~ 
		//~ 	return ""
		//~ }
		public static readonly Func<string, string, string> DefaultWrappingLookupFunc = (operation, path) =>
		{
			var v = Environment.GetEnvironmentVariable(Constants.EnvVaultWrapTTL);
			if (!string.IsNullOrEmpty(v))
				return v;
			if ((operation == "PUT" || operation == "POST") && path == "sys/wrapping/wrap")
				return DefaultWrappingTTL;
			return "";
		};
		//~ )
    }
}

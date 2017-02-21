using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Logical
{
	// Auth is the resulting authentication information that is part of
	// Response for credential backends.
	//~ type Auth struct {
	public class Auth : LeaseOptions
	{
		//~ LeaseOptions
		// Base Class
		public Auth()
		{ }

		public Auth(LeaseOptions lo)
		{
			this.Increment = lo.Increment;
			this.IssueTime = lo.IssueTime;
			this.Renewable = lo.Renewable;
			this.TTL = lo.TTL;
		}

		// InternalData is JSON-encodable data that is stored with the auth struct.
		// This will be sent back during a Renew/Revoke for storing internal data
		// used for those operations.
		//~ InternalData map[string]interface{} `json:"internal_data" mapstructure:"internal_data" structs:"internal_data"`
		[JsonProperty("internal_data")]
		public IDictionary<string, object> InternalData
		{ get; set; }

		// DisplayName is a non-security sensitive identifier that is
		// applicable to this Auth. It is used for logging and prefixing
		// of dynamic secrets. For example, DisplayName may be "armon" for
		// the github credential backend. If the client token is used to
		// generate a SQL credential, the user may be "github-armon-uuid".
		// This is to help identify the source without using audit tables.
		//~ DisplayName string `json:"display_name" mapstructure:"display_name" structs:"display_name"`
		[JsonProperty("display_name")]
		public string DisplayName
		{ get; set; }

		// Policies is the list of policies that the authenticated user
		// is associated with.
		//~ Policies[]string `json:"policies" mapstructure:"policies" structs:"policies"`
		[JsonProperty("policies")]
		public string[] Policies
		{ get; set; }

		// Metadata is used to attach arbitrary string-type metadata to
		// an authenticated user. This metadata will be outputted into the
		// audit log.
		//~ Metadata map[string]string `json:"metadata" mapstructure:"metadata" structs:"metadata"`
		[JsonProperty("metadata")]
		public IDictionary<string, string> Metadata
		{ get; set; }

		// ClientToken is the token that is generated for the authentication.
		// This will be filled in by Vault core when an auth structure is
		// returned. Setting this manually will have no effect.
		//~ ClientToken string `json:"client_token" mapstructure:"client_token" structs:"client_token"`
		[JsonProperty("client_token")]
		public string ClientToken
		{ get; set; }


		// Accessor is the identifier for the ClientToken. This can be used
		// to perform management functionalities (especially revocation) when
		// ClientToken in the audit logs are obfuscated. Accessor can be used
		// to revoke a ClientToken and to lookup the capabilities of the ClientToken,
		// both without actually knowing the ClientToken.
		//~ Accessor string `json:"accessor" mapstructure:"accessor" structs:"accessor"`
		[JsonProperty("accessor")]
		public string Accessor
		{ get; set; }

		// Period indicates that the token generated using this Auth object
		// should never expire. The token should be renewed within the duration
		// specified by this period.
		//~ Period time.Duration `json:"period" mapstructure:"period" structs:"period"`
		[JsonProperty("period")]
		public TimeSpan Period
		{ get; set; }

		//~ func (a *Auth) GoString() string {
		//~ 	return fmt.Sprintf("*%#v", *a)
		//~ }
		public string GoString()
		{
			return $"*{this}";  // TODO: not quite
		}

		public Auth DeepCopy()
		{
			var copy = new Auth
			{
				InternalData = this.InternalData?.DeepCopy(),
				DisplayName = this.DisplayName,
				Policies =  this.Policies?.DeepCopy(),
				Metadata = this.Metadata?.DeepCopy(),
				ClientToken = this.ClientToken,
				Accessor = this.Accessor,
				Period = this.Period,
				// LeaseOptions
				Increment = this.Increment,
				IssueTime = this.IssueTime,
				Renewable = this.Renewable,
				TTL = this.TTL,
			};

			return copy;
		}
	}
}

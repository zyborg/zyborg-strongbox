using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Audit
{
	//~ type AuditFormatWriter interface {
	public interface IAuditFormatWriter
	{
		//~ WriteRequest(io.Writer, *AuditRequestEntry) error
		//~ WriteResponse(io.Writer, *AuditResponseEntry) error
		void WriteRequest(Stream s, AuditRequestEntry entry);
		void WriteResponse(Stream s, AuditResponseEntry entry);
	}

	// AuditFormatter implements the Formatter interface, and allows the underlying
	// marshaller to be swapped out
	//~ type AuditFormatter struct {
	public class AuditFormatter : IAuditFormatWriter
	{
		//~ AuditFormatWriter


		//~ func (f *AuditFormatter) FormatRequest(
		//~ 	w io.Writer,
		//~ 	config FormatterConfig,
		//~ 	auth *logical.Auth,
		//~ 	req *logical.Request,
		//~ 	inErr error) error {
		public void FormatRequest(Stream w, FormatterConfig config,
				Logical.Auth auth, Logical.Request req, Exception inErr)
		{
			//~ if req == nil {
			//~ 	return fmt.Errorf("request to request-audit a nil request")
			//~ }
			//~ 
			//~ if w == nil {
			//~ 	return fmt.Errorf("writer for audit request is nil")
			//~ }
			//~ 
			//~ if f.AuditFormatWriter == nil {
			//~ 	return fmt.Errorf("no format writer specified")
			//~ }
			if (req == null)
				throw new ArgumentNullException(nameof(req));
			if (w == null)
				throw new ArgumentNullException(nameof(w));
			//if (this.AuditFormatWriter == null)
			//	throw new InvalidOperationException("no format writer present");

			using (var defer = new Util.Defer())
			{
				//~ if !config.Raw {
				if (!config.Raw)
				{
					// Before we copy the structure we must nil out some data
					// otherwise we will cause reflection to panic and die
					//~ if req.Connection != nil && req.Connection.ConnState != nil {
					//~ 	origReq := req
					//~ 	origState := req.Connection.ConnState
					//~ 	req.Connection.ConnState = nil
					//~ 	defer func() {
					//~ 		origReq.Connection.ConnState = origState
					//~ 	}()
					//~ }
					if (req?.Connection?.ConnectionState != null)
					{
						var origReq = req;
						var origState = req.Connection.ConnectionState;
						req.Connection.ConnectionState = null;
						defer.Add(() => origReq.Connection.ConnectionState = origState);
					}

					// Copy the auth structure
					//~ if auth != nil {
					//~ 	cp, err:= copystructure.Copy(auth)
					//~ 	if err != nil {
					//~ 		return err
					//~     }
					//~ 	auth = cp.(*logical.Auth)
					//~ }
					if (auth != null)
						auth = auth.DeepCopy();

					//~ cp, err:= copystructure.Copy(req)
					//~ if err != nil {
					//~ 	return err
					//~ }
					//~ req = cp.(*logical.Request)
					req = req.DeepCopy();

					// Hash any sensitive information
					//~ if auth != nil {
					//~ 	if err := Hash(config.Salt, auth); err != nil {
					//~ 		return err
					//~ 	}
					//~ }
					if (auth != null)
					{
						Hash(config.Salt, auth);
					}

					// Cache and restore accessor in the request
					//~ var clientTokenAccessor string
					//~ if !config.HMACAccessor && req != nil && req.ClientTokenAccessor != "" {
					//~ 	clientTokenAccessor = req.ClientTokenAccessor
					//~ }
					//~ if err := Hash(config.Salt, req); err != nil {
					//~ 	return err
					//~ }
					//~ if clientTokenAccessor != "" {
					//~ 	req.ClientTokenAccessor = clientTokenAccessor
					//~ }
					var clientTokenAccessor = config.HMACAccessor
						? null
						: req?.ClientTokenAccessor;
					Hash(config.Salt, req);
					if (!string.IsNullOrEmpty(clientTokenAccessor))
						req.ClientTokenAccessor = clientTokenAccessor;
				}

				// If auth is nil, make an empty one
				//~ if auth == nil {
				//~ 	auth = new(logical.Auth)
				//~ }
				if (auth == null)
					auth = new Logical.Auth();

				//~ var errString string
				//~ if inErr != nil {
				//~ 		errString = inErr.Error()
				//~ }
				string errString = null;
				if (inErr != null)
					errString = inErr.Message;

				//~reqEntry:= &AuditRequestEntry{
				var reqEntry = new AuditRequestEntry
				{
					//~ Type: "request",
					//~ Error: errString,
					//~ 
					//~ Auth: AuditAuth{
					//~ 	DisplayName: auth.DisplayName,
					//~ 	Policies: auth.Policies,
					//~ 	Metadata: auth.Metadata,
					//~ },
					Type = "request",
					Error = errString,
					Auth = new AuditAuth
					{
						DisplayName = auth.DisplayName,
						Policies = auth.Policies,
						Metadata = auth.Metadata,
					},

					//~ Request: AuditRequest{
					//~ 	ID: req.ID,
					//~ 	ClientToken: req.ClientToken,
					//~ 	ClientTokenAccessor: req.ClientTokenAccessor,
					//~ 	Operation: req.Operation,
					//~ 	Path: req.Path,
					//~ 	Data: req.Data,
					//~ 	RemoteAddr: getRemoteAddr(req),
					//~ 	ReplicationCluster: req.ReplicationCluster,
					//~ 	Headers: req.Headers,
					//~ },
					Request = new AuditRequest
					{
						ID = req.ID,
						ClientToken = req.ClientToken,
						ClientTokenAccessor = req.ClientTokenAccessor,
						Operation = req.Operation,
						Path = req.Path,
						Data = req.Data,
						RemoteAddr = GetRemoteAddr(req),
						ReplicationCluster = req.ReplicationCluster,
						Headers = req.Headers,
					},
				};

				//~ if req.WrapInfo != nil {
				//~ 	reqEntry.Request.WrapTTL = int(req.WrapInfo.TTL / time.Second)
				//~ }
				if (req.WrapInfo != null)
					reqEntry.Request.WrapTTL = (int)req.WrapInfo.TTL.TotalSeconds;

				//~ if !config.OmitTime {
				//~ 	reqEntry.Time = time.Now().UTC().Format(time.RFC3339)
				//~ }
				if (!config.OmitTime)
					reqEntry.Time = DateTime.UtcNow.FormatUtcAsRFC3339();

				//~ return f.AuditFormatWriter.WriteRequest(w, reqEntry)
				return this.WriteRequest(w, reqEntry);
			}
		}

		//~ func (f *AuditFormatter) FormatResponse(
		//~	w io.Writer,
		//~	config FormatterConfig,
		//~	auth *logical.Auth,
		//~	req *logical.Request,
		//~	resp *logical.Response,
		//~	inErr error) error {
		public void FormatResponse(Stream w, FormatterConfig config,
				Logical.Auth auth, Logical.Request req, Logical.Response resp, Exception inErr)
		{

			//~ if req == nil {
			//~ 	return fmt.Errorf("request to response-audit a nil request")
			//~ }
			//~ 
			//~ if w == nil {
			//~ 	return fmt.Errorf("writer for audit request is nil")
			//~ }
			//~ 
			//~ if f.AuditFormatWriter == nil {
			//~ 	return fmt.Errorf("no format writer specified")
			//~ }
			if (req == null)
				throw new ArgumentNullException(nameof(req));
			if (w == null)
				throw new ArgumentNullException(nameof(w));

			using (var defer = new Util.Defer())
			{
				//~ if !config.Raw {
				if (!config.Raw)
				{
					// Before we copy the structure we must nil out some data
					// otherwise we will cause reflection to panic and die
					//~ if req.Connection != nil && req.Connection.ConnState != nil {
					//~ origReq:= req
					//~ 	origState:= req.Connection.ConnState
					//~ 	req.Connection.ConnState = nil
					//~ 	defer func() {
					//~ 		origReq.Connection.ConnState = origState
					//~ 	} ()
					//~ }
					if (req?.Connection?.ConnectionState != null)
					{
						var origReq = req;
						var origState = req.Connection.ConnectionState;
						req.Connection.ConnectionState = null;
						defer.Add(() => origReq.Connection.ConnectionState = origState);
					}

					// Copy the auth structure
					//! if auth != nil {
					//! 	cp, err:= copystructure.Copy(auth)
					//! 	if err != nil {
					//! 		return err
					//! 	}
					//! 	auth = cp.(*logical.Auth)
					//! }
					if (auth != null)
						auth = auth.DeepCopy();

					//~ cp, err:= copystructure.Copy(req)
					//~ if err != nil {
					//~ 	return err
					//~ }
					//~ req = cp.(*logical.Request)
					req = req.DeepCopy();

					//~ if resp != nil {
					//~ 	cp, err:= copystructure.Copy(resp)
					//~ 	if err != nil {
					//~ 		return err
					//~ 	}
					//~ 	resp = cp.(*logical.Response)
					//~ }
					if (resp != null)
						resp = resp.DeepCopy();


					// Hash any sensitive information

					// Cache and restore accessor in the auth
					//~ if auth != nil {
					//~ 	var accessor string
					//~ 	if !config.HMACAccessor && auth.Accessor != "" {
					//~ 		accessor = auth.Accessor
					//~ 	}
					//~ 	if err := Hash(config.Salt, auth); err != nil {
					//~ 		return err
					//~ 	}
					//~ 	if accessor != "" {
					//~ 		auth.Accessor = accessor
					//~ 	}
					//~ }
					if (auth != null)
					{
						var accessor = config.HMACAccessor
							? null
							: auth.Accessor;
						Hash(config.Salt, auth);
						if (!string.IsNullOrEmpty(accessor))
							auth.Accessor = accessor;
					}

					// Cache and restore accessor in the request
					//~ var clientTokenAccessor string
					//~ if !config.HMACAccessor && req != nil && req.ClientTokenAccessor != "" {
					//~ 	clientTokenAccessor = req.ClientTokenAccessor
					//~ }
					//~ if err := Hash(config.Salt, req); err != nil {
					//~ 	return err
					//~ }
					//~ if clientTokenAccessor != "" {
					//~ 	req.ClientTokenAccessor = clientTokenAccessor
					//~ }
					var clientTokenAccessor = config.HMACAccessor
						? null
						: req?.ClientTokenAccessor;
					Hash(config.Salt, req);
					if (!string.IsNullOrEmpty(clientTokenAccessor))
						req.ClientTokenAccessor = clientTokenAccessor;

					// Cache and restore accessor in the response
					//~ if resp != nil {
					//~ 	var accessor, wrappedAccessor string
					//~ 	if !config.HMACAccessor && resp != nil && resp.Auth != nil && resp.Auth.Accessor != "" {
					//~ 		accessor = resp.Auth.Accessor
					//~ 	}
					//~ 	if !config.HMACAccessor && resp != nil && resp.WrapInfo != nil && resp.WrapInfo.WrappedAccessor != "" {
					//~ 		wrappedAccessor = resp.WrapInfo.WrappedAccessor
					//~ 	}
					//~ 	if err := Hash(config.Salt, resp); err != nil {
					//~ 		return err
					//~ 	}
					//~ 	if accessor != "" {
					//~ 		resp.Auth.Accessor = accessor
					//~ 	}
					//~ 	if wrappedAccessor != "" {
					//~ 		resp.WrapInfo.WrappedAccessor = wrappedAccessor
					//~ 	}
					//~ }
					if (resp != null)
					{
						var accessor = config.HMACAccessor
							? null
							: resp?.Auth?.Accessor;
						var wrappedAccessor = config.HMACAccessor
							? null
							: resp?.WrapInfo?.WrappedAccessor;
						Hash(config.Salt, resp);
						if (!string.IsNullOrEmpty(accessor))
							resp.Auth.Accessor = accessor;
						if (!string.IsNullOrEmpty(wrappedAccessor))
							resp.WrapInfo.WrappedAccessor = wrappedAccessor;
					}
				}

				// If things are nil, make empty to avoid panics
				//~ if auth == nil {
				//~ 	auth = new(logical.Auth)
				//~ }
				//~ if resp == nil {
				//~ 	resp = new(logical.Response)
				//~ }
				//~ var errString string
				//~ if inErr != nil {
				//~ 	errString = inErr.Error()
				//~ }
				if (auth == null)
					auth = new Logical.Auth();
				if (resp == null)
					resp = new Logical.Response();
				string errString = null;
				if (inErr != null)
					errString = inErr.Message;

				//~ var respAuth *AuditAuth
				//~ if resp.Auth != nil {
				//~ 	respAuth = &AuditAuth{
				//~ 		ClientToken: resp.Auth.ClientToken,
				//~ 		Accessor:    resp.Auth.Accessor,
				//~ 		DisplayName: resp.Auth.DisplayName,
				//~ 		Policies:    resp.Auth.Policies,
				//~ 		Metadata:    resp.Auth.Metadata,
				//~ 	}
				//~ }
				AuditAuth respAuth = null;
				if (resp.Auth != null)
				{
					respAuth = new AuditAuth
					{
						ClientToken = resp.Auth.ClientToken,
						Accessor = resp.Auth.Accessor,
						DisplayName = resp.Auth.DisplayName,
						Policies = resp.Auth.Policies,
						Metadata = resp.Auth.Metadata,
					};
				}

				//~ var respSecret *AuditSecret
				//~ if resp.Secret != nil {
				//~ 	respSecret = &AuditSecret{
				//~ 		LeaseID: resp.Secret.LeaseID,
				//~ 	}
				//~ }
				AuditSecret respSecret = null;
				if (resp.Secret != null)
					respSecret = new AuditSecret
					{
						LeaseID = resp.Secret.LeaseID,
					};

				//~ var respWrapInfo *AuditResponseWrapInfo
				//~ if resp.WrapInfo != nil {
				//~ 	token := resp.WrapInfo.Token
				//~ 	if jwtToken := parseVaultTokenFromJWT(token); jwtToken != nil {
				//~ 		token = *jwtToken
				//~ 	}
				//~ 	respWrapInfo = &AuditResponseWrapInfo{
				//~ 		TTL:             int(resp.WrapInfo.TTL / time.Second),
				//~ 		Token:           token,
				//~ 		CreationTime:    resp.WrapInfo.CreationTime.Format(time.RFC3339Nano),
				//~ 		WrappedAccessor: resp.WrapInfo.WrappedAccessor,
				//~ 	}
				//~ }
				AuditResponseWrapInfo respWrapInfo = null;
				if (resp.WrapInfo != null)
				{
					var token = resp.WrapInfo.Token;
					var jwtToken = ParseVaultTokenFromJWT(token);
					if (!string.IsNullOrEmpty(jwtToken))
						token = jwtToken;
					respWrapInfo = new AuditResponseWrapInfo
					{
						TTL = (int)resp.WrapInfo.TTL.TotalSeconds,
						Token = token,
						CreationTime = resp.WrapInfo.CreationTime.FormatUtcAsRFC3339Nano(),
						WrappedAccessor = resp.WrapInfo.WrappedAccessor,
					};
				}

				//~ respEntry := &AuditResponseEntry{
				var respEntry = new AuditResponseEntry
				{
					//~ Type:  "response",
					//~ Error: errString,
					Type = "response",
					Error = errString,

					//~ Auth: AuditAuth{
					//~ 	DisplayName: auth.DisplayName,
					//~ 	Policies:    auth.Policies,
					//~ 	Metadata:    auth.Metadata,
					//~ },
					Auth = new AuditAuth
					{
						DisplayName = auth.DisplayName,
						Policies = auth.Policies,
						Metadata = auth.Metadata,
					},

					//~ Request: AuditRequest{
					//~ 	ID:                  req.ID,
					//~ 	ClientToken:         req.ClientToken,
					//~ 	ClientTokenAccessor: req.ClientTokenAccessor,
					//~ 	Operation:           req.Operation,
					//~ 	Path:                req.Path,
					//~ 	Data:                req.Data,
					//~ 	RemoteAddr:          getRemoteAddr(req),
					//~ 	ReplicationCluster:  req.ReplicationCluster,
					//~ 	Headers:             req.Headers,
					//~ },
					Request = new AuditRequest
					{
						ID = req.ID,
						ClientToken = req.ClientToken,
						ClientTokenAccessor = req.ClientTokenAccessor,
						Operation = req.Operation,
						Path = req.Path,
						Data = req.Data,
						RemoteAddr = GetRemoteAddr(req),
						ReplicationCluster = req.ReplicationCluster,
						Headers = req.Headers,
					},

					//~ Response: AuditResponse{
					//~ 	Auth:     respAuth,
					//~ 	Secret:   respSecret,
					//~ 	Data:     resp.Data,
					//~ 	Redirect: resp.Redirect,
					//~ 	WrapInfo: respWrapInfo,
					//~ },
					Response = new AuditResponse
					{
						Auth = respAuth,
						Secret = respSecret,
						Data = resp.Data,
						Redirect = resp.Redirect,
						WrapInfo = respWrapInfo,
					},
				};

				//~ if req.WrapInfo != nil {
				//~ 	respEntry.Request.WrapTTL = int(req.WrapInfo.TTL / time.Second)
				//~ }
				if (req.WrapInfo != null)
					respEntry.Request.WrapTTL = (int)req.WrapInfo.TTL.TotalSeconds;

				//~ if !config.OmitTime {
				//~ 	respEntry.Time = time.Now().UTC().Format(time.RFC3339)
				//~ }
				if (!config.OmitTime)
					respEntry.Time = DateTime.UtcNow.FormatUtcAsRFC3339();

				//~ return f.AuditFormatWriter.WriteResponse(w, respEntry)
				return WriteResponse(w, respEntry);
			}
		}

		// getRemoteAddr safely gets the remote address avoiding a nil pointer
		//~ func getRemoteAddr(req *logical.Request) string {
		public static string GetRemoteAddr(Logical.Request req)
		{
			//~ if req != nil && req.Connection != nil {
			//~ 	return req.Connection.RemoteAddr
			//~ }
			//~ return ""
			if (req?.Connection != null)
				return req.Connection.RemoteAddr;
			return string.Empty;
		}

		// parseVaultTokenFromJWT returns a string iff the token was a JWT and we could
		// extract the original token ID from inside
		//~ func parseVaultTokenFromJWT(token string) *string {
		public static string ParseVaultTokenFromJWT(string token)
		{
			//~ if strings.Count(token, ".") != 2 {
			//~ 	return nil
			//~ }
			if (token.Split('.').Length != 3)
				return null;

			//~ wt, err := jws.ParseJWT([]byte(token))
			//~ if err != nil || wt == nil {
			//~ 	return nil
			//~ }
			var wt = Jose.JWT.Decode<Dictionary<string, string>>(token);

			//~ result, _ := wt.Claims().JWTID()
			//~
			//~ return &result

			// As per:
			//    https://tools.ietf.org/html/rfc7519#section-4.1.7
			if (wt.TryGetValue("jti", out var jwtid))
				return jwtid;
			return null;
		}
	}

	// AuditRequest is the structure of a request audit log entry in Audit.
	//~ type AuditRequestEntry struct {
	public class AuditRequestEntry
	{
		//~ Time    string       `json:"time,omitempty"`
		//~ Type    string       `json:"type"`
		//~ Auth    AuditAuth    `json:"auth"`
		//~ Request AuditRequest `json:"request"`
		//~ Error   string       `json:"error"`
		[JsonProperty("time,omitempty")] public string       Time    { get; set; }
		[JsonProperty("type")]           public string       Type    { get; set; }
		[JsonProperty("auth")]           public AuditAuth    Auth    { get; set; }
		[JsonProperty("request")]        public AuditRequest Request { get; set; }
		[JsonProperty("error")]          public string       Error   { get; set; }
	}

	// AuditResponseEntry is the structure of a response audit log entry in Audit.
	//~ type AuditResponseEntry struct {
	public class AuditResponseEntry
	{
		//~ Time     string        `json:"time,omitempty"`
		//~ Type     string        `json:"type"`
		//~ Auth     AuditAuth     `json:"auth"`
		//~ Request  AuditRequest  `json:"request"`
		//~ Response AuditResponse `json:"response"`
		//~ Error    string        `json:"error"`
		[JsonProperty("time,omitempty")] public string        Time     { get; set; }
		[JsonProperty("type")]			 public string        Type     { get; set; }
		[JsonProperty("auth")]			 public AuditAuth     Auth     { get; set; }
		[JsonProperty("request")]		 public AuditRequest  Request  { get; set; }
		[JsonProperty("response")]		 public AuditResponse Response { get; set; }
		[JsonProperty("error")]			 public string        Error    { get; set; }
	}

	//~ type AuditRequest struct {
	public class AuditRequest
	{
		//~ ID                  string                 `json:"id"`
		//~ ReplicationCluster  string                 `json:"replication_cluster,omitempty"`
		//~ Operation           logical.Operation      `json:"operation"`
		//~ ClientToken         string                 `json:"client_token"`
		//~ ClientTokenAccessor string                 `json:"client_token_accessor"`
		//~ Path                string                 `json:"path"`
		//~ Data                map[string]interface{} `json:"data"`
		//~ RemoteAddr          string                 `json:"remote_address"`
		//~ WrapTTL             int                    `json:"wrap_ttl"`
		//~ Headers             map[string][]string    `json:"headers"`
		[JsonProperty("id")] 							public string                 ID                  { get; set; } 
		[JsonProperty("replication_cluster,omitempty")] public string                 ReplicationCluster  { get; set; } 
		[JsonProperty("operation")] 					public Logical.Operation      Operation           { get; set; } 
		[JsonProperty("client_token")] 					public string                 ClientToken         { get; set; } 
		[JsonProperty("client_token_accessor")] 		public string                 ClientTokenAccessor { get; set; } 
		[JsonProperty("path")] 							public string                 Path                { get; set; } 
		[JsonProperty("data")]                          public IDictionary<string, object> Data                { get; set; } 
		[JsonProperty("remote_address")] 				public string                 RemoteAddr          { get; set; } 
		[JsonProperty("wrap_ttl")] 						public int                    WrapTTL             { get; set; } 
		[JsonProperty("headers")] 						public IDictionary<string, string[]>     Headers             { get; set; } 
		}

	//~ type AuditResponse struct {
	public class AuditResponse
	{
		//~ Auth     *AuditAuth             `json:"auth,omitempty"`
		//~ Secret   *AuditSecret           `json:"secret,omitempty"`
		//~ Data     map[string]interface{} `json:"data,omitempty"`
		//~ Redirect string                 `json:"redirect,omitempty"`
		//~ WrapInfo *AuditResponseWrapInfo `json:"wrap_info,omitempty"`
		[JsonProperty("auth,omitempty")] 	  public AuditAuth             Auth     { get; set; } 
		[JsonProperty("secret,omitempty")] 	  public AuditSecret           Secret   { get; set; } 
		[JsonProperty("data,omitempty")]      public IDictionary<string, object> Data     { get; set; } 
		[JsonProperty("redirect,omitempty")]  public string                 Redirect { get; set; } 
		[JsonProperty("wrap_info,omitempty")] public AuditResponseWrapInfo WrapInfo { get; set; } 
	}

	//~ type AuditAuth struct {
	public class AuditAuth
	{
		//~ ClientToken string            `json:"client_token"`
		//~ Accessor    string            `json:"accessor"`
		//~ DisplayName string            `json:"display_name"`
		//~ Policies    []string          `json:"policies"`
		//~ Metadata    map[string]string `json:"metadata"`
		[JsonProperty("client_token")] public string            ClientToken { get; set; } 
		[JsonProperty("accessor")] 	   public string            Accessor    { get; set; } 
		[JsonProperty("display_name")] public string            DisplayName { get; set; } 
		[JsonProperty("policies")] 	   public string[]          Policies    { get; set; } 
		[JsonProperty("metadata")] 	   public IDictionary<string, string> Metadata    { get; set; } 
	}

	//~ type AuditSecret struct {
	public class AuditSecret
	{
		//~ LeaseID string `json:"lease_id"`
		[JsonProperty("lease_id")] public string LeaseID { get; set; }
}

	//~ type AuditResponseWrapInfo struct {
	public class AuditResponseWrapInfo
	{
		//~ TTL             int    `json:"ttl"`
		//~ Token           string `json:"token"`
		//~ CreationTime    string `json:"creation_time"`
		//~ WrappedAccessor string `json:"wrapped_accessor,omitempty"`
		 [JsonProperty("ttl")] 						   public int    TTL             { get; set; }
		 [JsonProperty("token")] 					   public string Token           { get; set; }
		 [JsonProperty("creation_time")] 			   public string CreationTime    { get; set; }
		 [JsonProperty("wrapped_accessor,omitempty")]  public string WrappedAccessor { get; set; }
	}
}

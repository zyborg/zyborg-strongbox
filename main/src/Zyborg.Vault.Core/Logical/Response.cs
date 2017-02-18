using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Logical
{

	//~ type ResponseWrapInfo struct {
	public class ResponseWrapInfo
	{
		// Setting to non-zero specifies that the response should be wrapped.
		// Specifies the desired TTL of the wrapping token.
		//~ TTL time.Duration `json:"ttl" structs:"ttl" mapstructure:"ttl"`
		[JsonProperty("ttl")]
		public TimeSpan TTL
		{ get; set; }

		// The token containing the wrapped response
		//~ Token string `json:"token" structs:"token" mapstructure:"token"`
		[JsonProperty("token")]
		public string Token
		{ get; set; }

		// The creation time. This can be used with the TTL to figure out an
		// expected expiration.
		//~ CreationTime time.Time `json:"creation_time" structs:"creation_time" mapstructure:"cration_time"`
		[JsonProperty("creation_time")]
		public DateTime CreationTime
		{ get; set; }

		// If the contained response is the output of a token creation call, the
		// created token's accessor will be accessible here
		//~ WrappedAccessor string `json:"wrapped_accessor" structs:"wrapped_accessor" mapstructure:"wrapped_accessor"`
		[JsonProperty("wrapped_accessor")]
		public string WrappedAccessor
		{ get; set; }

		// The format to use. This doesn't get returned, it's only internal.
		//~ Format string `json:"format" structs:"format" mapstructure:"format"`
		[JsonProperty("format")]
		public string Format
		{ get; set; }

		public ResponseWrapInfo DeepCopy()
		{
			var copy = new ResponseWrapInfo
			{
				TTL = this.TTL,
				Token = this.Token,
				CreationTime = this.CreationTime,
				WrappedAccessor = this.WrappedAccessor,
				Format = this.Format,
			};
			return copy;
		}
	}

	// Response is a struct that stores the response of a request.
	// It is used to abstract the details of the higher level request protocol.
	//~ type Response struct {
	public class Response
	{
		//~ const (
			// HTTPContentType can be specified in the Data field of a Response
			// so that the HTTP front end can specify a custom Content-Type associated
			// with the HTTPRawBody. This can only be used for non-secrets, and should
			// be avoided unless absolutely necessary, such as implementing a specification.
			// The value must be a string.
			//~ HTTPContentType = "http_content_type"

			// HTTPRawBody is the raw content of the HTTP body that goes with the HTTPContentType.
			// This can only be specified for non-secrets, and should should be similarly
			// avoided like the HTTPContentType. The value must be a byte slice.
			//~ HTTPRawBody = "http_raw_body"

			// HTTPStatusCode is the response code of the HTTP body that goes with the HTTPContentType.
			// This can only be specified for non-secrets, and should should be similarly
			// avoided like the HTTPContentType. The value must be an integer.
			//~ HTTPStatusCode = "http_status_code"
		//~)
		public const string HTTPContentType = "http_content_type";
		public const string HTTPRawBody = "http_raw_body";
		public const string HTTPStatusCode = "http_status_code";

		// Secret, if not nil, denotes that this response represents a secret.
		//~ Secret* Secret `json:"secret" structs:"secret" mapstructure:"secret"`
		[JsonProperty("secret")]
		public Secret Secret
		{ get; set; }

		// Auth, if not nil, contains the authentication information for
		// this response. This is only checked and means something for
		// credential backends.
		//~ Auth *Auth `json:"auth" structs:"auth" mapstructure:"auth"`
		[JsonProperty("auth")]
		public Auth Auth
		{ get; set; }

		// Response data is an opaque map that must have string keys. For
		// secrets, this data is sent down to the user as-is. To store internal
		// data that you don't want the user to see, store it in
		// Secret.InternalData.
		//~ Data map[string]interface{} `json:"data" structs:"data" mapstructure:"data"`
		[JsonProperty("data")]
		public IDictionary<string, object> Data
		{ get; set; }

		// Redirect is an HTTP URL to redirect to for further authentication.
		// This is only valid for credential backends. This will be blanked
		// for any logical backend and ignored.
		//~ Redirect string `json:"redirect" structs:"redirect" mapstructure:"redirect"`
		[JsonProperty("redirect")]
		public string Redirect
		{ get; set; }

		// Warnings allow operations or backends to return warnings in response
		// to user actions without failing the action outright.
		// Making it private helps ensure that it is easy for various parts of
		// Vault (backend, core, etc.) to add warnings without accidentally
		// replacing what exists.
		//~ warnings []string `json:"warnings" structs:"warnings" mapstructure:"warnings"`
		[JsonProperty("warnings")]
		public IList<string> Warnings
		{ get; set; }

		// Information for wrapping the response in a cubbyhole
		//~ WrapInfo *ResponseWrapInfo `json:"wrap_info" structs:"wrap_info" mapstructure:"wrap_info"`
		[JsonProperty("wrap_info")]
		public ResponseWrapInfo WrapInfo
		{ get; set; }

		// init() is similar to a Class constructor but for Go Packages
		// This registers a copy function for the type of Response{}
		// with the global "Copiers" variable defined in the copystructure package
		// We just implement a Copy method down below to do the same thing

		//~ func init() {
		//~ 	copystructure.Copiers[reflect.TypeOf(Response{})] = func(v interface{}) (interface{}, error) {
		//~ 		input := v.(Response)
		//~ 		ret := Response{
		//~ 			Redirect: input.Redirect,
		//~ 		}
		//~ 
		//~ 		if input.Secret != nil {
		//~ 			retSec, err := copystructure.Copy(input.Secret)
		//~ 			if err != nil {
		//~ 				return nil, fmt.Errorf("error copying Secret: %v", err)
		//~ 			}
		//~ 			ret.Secret = retSec.(*Secret)
		//~ 		}
		//~ 
		//~ 		if input.Auth != nil {
		//~ 			retAuth, err := copystructure.Copy(input.Auth)
		//~ 			if err != nil {
		//~ 				return nil, fmt.Errorf("error copying Auth: %v", err)
		//~ 			}
		//~ 			ret.Auth = retAuth.(*Auth)
		//~ 		}
		//~ 
		//~ 		if input.Data != nil {
		//~ 			retData, err := copystructure.Copy(&input.Data)
		//~ 			if err != nil {
		//~ 				return nil, fmt.Errorf("error copying Data: %v", err)
		//~ 			}
		//~ 			ret.Data = *(retData.(*map[string]interface{}))
		//~ 		}
		//~ 
		//~ 		if input.Warnings() != nil {
		//~ 			for _, warning := range input.Warnings() {
		//~ 				ret.AddWarning(warning)
		//~ 			}
		//~ 		}
		//~ 
		//~ 		if input.WrapInfo != nil {
		//~ 			retWrapInfo, err := copystructure.Copy(input.WrapInfo)
		//~ 			if err != nil {
		//~ 				return nil, fmt.Errorf("error copying WrapInfo: %v", err)
		//~ 			}
		//~ 			ret.WrapInfo = retWrapInfo.(*ResponseWrapInfo)
		//~ 		}
		//~ 
		//~ 		return &ret, nil
		//~ 	}
		//~ }

		// AddWarning adds a warning into the response's warning list
		//~ func (r *Response) AddWarning(warning string) {
		public void AddWarning(string warning)
		{
			//~ if r.warnings == nil {
			//~ 	r.warnings = make([]string, 0, 1)
			//~ }
			//~ r.warnings = append(r.warnings, warning)
			if (this.Warnings == null)
				this.Warnings = new List<string>();
			this.Warnings.Add(warning);
		}

		// Warnings returns the list of warnings set on the response
		//~ func (r *Response) Warnings() []string {
		//~ 	return r.warnings
		//~ }
		// IMPL:  We don't need this since the property returns itself

		// ClearWarnings clears the response's warning list
		//~ func (r *Response) ClearWarnings() {
		//~ 	r.warnings = make([]string, 0, 1)
		//~ }
		public void ClearWarnings()
		{
			this.Warnings?.Clear();
		}

		// Copies the warnings from the other response to this one
		//func (r *Response) CloneWarnings(other *Response) {
		//	r.warnings = other.warnings
		//}
		public void CloneWarnings(Response other)
		{
			this.Warnings = new List<string>(other.Warnings);
		}

		// IsError returns true if this response seems to indicate an error.
		//func (r *Response) IsError() bool {
		//	return r != nil && r.Data != nil && len(r.Data) == 1 && r.Data["error"] != nil
		//}
		public bool IsError()
		{
			return this.Data?.Count == 1 && this.Data.ContainsKey("error");
		}

		//~ func (r *Response) Error() error {
		public Exception Error()
		{
			//~ if !r.IsError() {
			//~ 	return nil
			//~ }
			if (!IsError())
				return null;

			//! switch r.Data["error"].(type) {
			//! case string:
			//! 	return errors.New(r.Data["error"].(string))
			//! case error:
			//! 	return r.Data["error"].(error)
			//! }
			switch (Data["error"])
			{
				case string s:
					return new Exception(s);
				case Exception ex:
					return ex;
			}

			//~ return nil
			return null;
		}

		// HelpResponse is used to format a help response
		//~ func HelpResponse(text string, seeAlso []string) *Response {
		public static Response HelpResponse(string text, string[] seeAlso)
		{
			//~ return &Response{
			//~ 	Data: map[string]interface{}{
			//~ 		"help":     text,
			//~ 		"see_also": seeAlso,
			//~ 	},
			//~ }
			return new Response
			{
				Data = new Dictionary<string, object>
				{
					["help"] = text,
					["see_also"] = seeAlso,
				},
			};
		}

		// ErrorResponse is used to format an error response
		//~ func ErrorResponse(text string) *Response {
		public static Response ErrorResponse(string text)
		{
			//~ return &Response{
			//~ 	Data: map[string]interface{}{
			//~ 		"error": text,
			//~ 	},
			//~ }
			return new Response
			{
				Data = new Dictionary<string, object>
				{
					["error"] = text,
				},
			};
		}

		// ListResponse is used to format a response to a list operation.
		//~ func ListResponse(keys []string) *Response {
		public static Response ListResponse(string[] keys)
		{
			//~ resp := &Response{
			//~ 	Data: map[string]interface{}{},
			//~ }
			//~ if len(keys) != 0 {
			//~ 	resp.Data["keys"] = keys
			//~ }
			//~ return resp
			var resp = new Response
			{
				Data = new Dictionary<string, object>(),
			};
			if (keys?.Length > 0)
				resp.Data["keys"] = keys;
			return resp;
		}

		public Response DeepCopy()
		{
			//~ input := v.(Response)
			//~ ret := Response{
			//~ 	Redirect: input.Redirect,
			//~ }
			//~ 
			//~ if input.Secret != nil {
			//~ 	retSec, err := copystructure.Copy(input.Secret)
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("error copying Secret: %v", err)
			//~ 	}
			//~ 	ret.Secret = retSec.(*Secret)
			//~ }
			//~ 
			//~ if input.Auth != nil {
			//~ 	retAuth, err := copystructure.Copy(input.Auth)
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("error copying Auth: %v", err)
			//~ 	}
			//~ 	ret.Auth = retAuth.(*Auth)
			//~ }
			//~ 
			//~ if input.Data != nil {
			//~ 	retData, err := copystructure.Copy(&input.Data)
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("error copying Data: %v", err)
			//~ 	}
			//~ 	ret.Data = *(retData.(*map[string]interface{}))
			//~ }
			//~ 
			//~ if input.Warnings() != nil {
			//~ 	for _, warning := range input.Warnings() {
			//~ 		ret.AddWarning(warning)
			//~ 	}
			//~ }
			//~ 
			//~ if input.WrapInfo != nil {
			//~ 	retWrapInfo, err := copystructure.Copy(input.WrapInfo)
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("error copying WrapInfo: %v", err)
			//~ 	}
			//~ 	ret.WrapInfo = retWrapInfo.(*ResponseWrapInfo)
			//~ }

			var copy = new Response
			{
				Redirect = this.Redirect,
				Secret = this.Secret?.DeepCopy(),
				Auth = this.Auth?.DeepCopy(),
				Data = this.Data?.DeepCopy(),
				Warnings = this.Warnings?.DeepCopy(),
				WrapInfo = this.WrapInfo?.DeepCopy(),
			};

			return copy;
		}
	}
}

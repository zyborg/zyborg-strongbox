using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zyborg.Vault.API
{
	// Request is a raw request configuration structure used to initiate
	// API requests to the Vault server.
	//~ type Request struct {
	public class Request
	{
		//~ Method      string
		//~ URL         *url.URL
		//~ Params      url.Values
		//~ ClientToken string
		//~ WrapTTL     string
		//~ Obj         interface{}
		//~ Body        io.Reader
		//~ BodySize    int64
		public string Method
		{ get; set; }

		public UriBuilder URL
		{ get; set; }

		public List<string> Params
		{ get; set; }

		public string ClientToken
		{ get; set; }

		public string WrapTTL
		{ get; set; }

		public object Obj
		{ get; set; }

		public string Body
		{ get; set; }

		public int BodySize
		{ get; set; }

		// SetJSONBody is used to set a request body that is a JSON-encoded value.
		//~ func (r *Request) SetJSONBody(val interface{}) error {
		public void SetJSONBody(object val)
		{
			//~ buf := bytes.NewBuffer(nil)
			//~ enc := json.NewEncoder(buf)
			//~ if err := enc.Encode(val); err != nil {
			//~ 	return err
			//~ }
			var buf = JsonConvert.SerializeObject(val);

			//~ r.Obj = val
			//~ r.Body = buf
			//~ r.BodySize = int64(buf.Len())
			//~ return nil
			Obj = val;
			Body = buf;
			BodySize = buf.Length;
		}

		// ResetJSONBody is used to reset the body for a redirect
		//~ func (r *Request) ResetJSONBody() error {
		public void ResetJSONBody()
		{
			if (Body == null)
				return;
			//~ if r.Body == nil {
			//~ 	return nil
			//~ }
			//~ return r.SetJSONBody(r.Obj)
			SetJSONBody(Obj);
		}

		// ToHTTP turns this request into a valid *http.Request for use with the
		// net/http package.
		//~ func (r *Request) ToHTTP() (*http.Request, error) {
		//public void ToHTTP()
		//{
		//	// Encode the query parameters
		//	//~ r.URL.RawQuery = r.Params.Encode()
		//	URL.Query = Params

		//	// Create the HTTP request
		//	req, err := http.NewRequest(r.Method, r.URL.RequestURI(), r.Body)
		//	if err != nil {
		//		return nil, err
		//	}

		//	req.URL.Scheme = r.URL.Scheme
		//	req.URL.Host = r.URL.Host
		//	req.Host = r.URL.Host

		//	if len(r.ClientToken) != 0 {
		//		req.Header.Set("X-Vault-Token", r.ClientToken)
		//	}

		//	if len(r.WrapTTL) != 0 {
		//		req.Header.Set("X-Vault-Wrap-TTL", r.WrapTTL)
		//	}

		//	return req, nil
		//}
	}
}

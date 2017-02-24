using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.API
{
	// Response is a raw response that wraps an HTTP response.
	//~ type Response struct {
	public class Response
	{
		//~ *http.Response
		public System.Net.Http.HttpResponseMessage HttpResponse
		{ get; set; }


		// DecodeJSON will decode the response body to a JSON structure. This
		// will consume the response body, but will not close it. Close must
		// still be called.
		//~ func (r *Response) DecodeJSON(out interface{}) error {
		//~ 	return jsonutil.DecodeJSONFromReader(r.Body, out)
		//~ }
		public object DecodeJSON()
		{
			return JsonConvert.DeserializeObject(HttpResponse.Content.ReadAsStringAsync().Result);
		}

		// Error returns an error response if there is one. If there is an error,
		// this will fully consume the response body, but will not close it. The
		// body must still be closed manually.
		//~ func (r *Response) Error() error {
		public void Error()
		{
			// 200 to 399 are okay status codes
			//~ if r.StatusCode >= 200 && r.StatusCode < 400 {
			//~ 	return nil
			//~ }
			if (HttpResponse.StatusCode >= System.Net.HttpStatusCode.OK
					&& HttpResponse.StatusCode <= System.Net.HttpStatusCode.BadRequest)
				return;

			// We have an error. Let's copy the body into our own buffer first,
			// so that if we can't decode JSON, we can at least copy it raw.
			//~ var bodyBuf bytes.Buffer
			//~ if _, err := io.Copy(&bodyBuf, r.Body); err != nil {
			//~ 	return err
			//~ }
			var bodyBuf = HttpResponse.Content.ReadAsByteArrayAsync().Result;

			// Decode the error response if we can. Note that we wrap the bodyBuf
			// in a bytes.Reader here so that the JSON decoder doesn't move the
			// read pointer for the original buffer.
			//~ var resp ErrorResponse
			//~ if err := jsonutil.DecodeJSON(bodyBuf.Bytes(), &resp); err != nil {
			//~ 	// Ignore the decoding error and just drop the raw response
			//~ 	return fmt.Errorf(
			//~ 		"Error making API request.\n\n"+
			//~ 			"URL: %s %s\n"+
			//~ 			"Code: %d. Raw Message:\n\n%s",
			//~ 		r.Request.Method, r.Request.URL.String(),
			//~ 		r.StatusCode, bodyBuf.String())
			//~ }
			ErrorResponse resp;
			try
			{
				resp = JsonConvert.DeserializeObject<ErrorResponse>(bodyBuf.ToUtf8String());
			}
			catch (Exception)
			{
				throw new Exception($"Error making API request\n\n"
						+ $"  URL: {HttpResponse.RequestMessage.Method} {HttpResponse.RequestMessage.RequestUri}\n\n"
						+ $"  Code:  {HttpResponse.StatusCode}. Raw message\n\n{bodyBuf.ToUtf8String()}");
			}

			//~ var errBody bytes.Buffer
			//~ errBody.WriteString(fmt.Sprintf(
			//~ 	"Error making API request.\n\n" +
			//~ 		"URL: %s %s\n" +
			//~ 		"Code: %d. Errors:\n\n",
			//~ 	r.Request.Method, r.Request.URL.String(),
			//~ 	r.StatusCode))
			//~ for _, err := range resp.Errors {
			//~ 	errBody.WriteString(fmt.Sprintf("* %s", err))
			//~ }
			var errBody = "Error making API request.\n\n"
						+ $"  URL: {HttpResponse.RequestMessage.Method} {HttpResponse.RequestMessage.RequestUri}\n\n"
						+ $"  Code:  {HttpResponse.StatusCode}. Errors:\n\n";
			foreach (var err in resp.Errors)
				errBody += $"  * {err}";

			//~ return fmt.Errorf(errBody.String())
			throw new Exception(errBody);
		}

		// ErrorResponse is the raw structure of errors when they're returned by the
		// HTTP API.
		//~ type ErrorResponse struct {
		//~ 	Errors []string
		//~ }
		public class ErrorResponse
		{
			public List<string> Errors
			{ get; set; }
		}
	}
}

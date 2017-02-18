using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyborg.Util;
using Zyborg.Util.IO;

namespace Zyborg.Vault.Audit
{
	// JSONFormatWriter is an AuditFormatWriter implementation that structures data into
	// a JSON format.
	//~ type JSONFormatWriter struct {
	public class JsonFormatWriter : IAuditFormatWriter
	{
		//~ Prefix string
		public string Prefix
		{ get; set; }

		//~ func (f *JSONFormatWriter) WriteRequest(w io.Writer, req *AuditRequestEntry) error {
		public void WriteRequest(Stream w, AuditRequestEntry req)
		{
			//~ if req == nil {
			//~ 	return fmt.Errorf("request entry was nil, cannot encode")
			//~ }
			if (req == null)
				throw new ArgumentNullException(nameof(req));

			//~ if len(f.Prefix) > 0 {
			//~ 	_, err := w.Write([]byte(f.Prefix))
			//~ 	if err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			if (!string.IsNullOrEmpty(Prefix))
				w.Write(Prefix.ToUtf8Bytes());

			//~ enc := json.NewEncoder(w)
			//~ return enc.Encode(req)
			w.Write(JsonConvert.SerializeObject(req).ToUtf8Bytes());
		}

		//~ func (f *JSONFormatWriter) WriteResponse(w io.Writer, resp *AuditResponseEntry) error {
		public void WriteResponse(Stream w, AuditResponseEntry resp)
		{
			//~ if resp == nil {
			//~ 	return fmt.Errorf("response entry was nil, cannot encode")
			//~ }
			if (resp == null)
				throw new ArgumentNullException(nameof(resp));

			//~ if len(f.Prefix) > 0 {
			//~ 	_, err := w.Write([]byte(f.Prefix))
			//~ 	if err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			if (!string.IsNullOrEmpty(Prefix))
				w.Write(Prefix.ToUtf8Bytes());

			//~ enc := json.NewEncoder(w)
			//~ return enc.Encode(resp)
			w.Write(JsonConvert.SerializeObject(resp).ToUtf8Bytes());
		}
	}
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zyborg.Util;

namespace Zyborg.Vault.Logical
{
	public static class Constants
	{
		// ErrReadOnly is returned when a backend does not support
		// writing. This can be caused by a read-only replica or secondary
		// cluster operation.
		//~ var ErrReadOnly = errors.New("Cannot write to readonly storage")
		public static readonly Exception ErrReadOnly = new Exception("Cannot write to readonly storage");
	}

	// Storage is the way that logical backends are able read/write data.
	public interface IStorage
	{
		IEnumerable<string> List(string prefix);

		StorageEntry Get(string x);

		void Put(StorageEntry x);

		void Delete(string x);

		IStorage DeepCopy();
	}

	// StorageEntry is the entry for an item in a Storage implementation.
	public class StorageEntry
	{
		public string Key
		{ get; set; }

		public byte[] Value
		{ get; set; }

		// DecodeJSON decodes the 'Value' present in StorageEntry.
		//~ func(e* StorageEntry) DecodeJSON(out interface{ }) error {
		public void DecodeJSON(object @out)
		{
			//~return jsonutil.DecodeJSON(e.Value, out)
			var jsonDes = JsonSerializer.CreateDefault();
			using (var ms = new MemoryStream(Value))
			using (var tr = new StreamReader(ms))
			{
				jsonDes.Populate(tr, @out);
			}
		}

		// StorageEntryJSON creates a StorageEntry with a JSON-encoded value.
		//~ func StorageEntryJSON(k string, v interface{ }) (* StorageEntry, error) {
		//~      encodedBytes, err := jsonutil.EncodeJSON(v)
		//~      if err != nil {
		//~          return nil, fmt.Errorf("failed to encode storage entry: %v", err)
		//~      }
		//~     return &StorageEntry{
		//~         Key:   k,
		//~         Value: encodedBytes,
		//~     }, nil
		//~ }
		public static StorageEntry StorageEntryJSON(string k, object v)
		{
			return new StorageEntry
			{
				Key = k,
				Value = JsonConvert.SerializeObject(v).ToUtf8Bytes(),
			};
		}
	}

	public interface IClearableView
	{
		string[] List(string x);

		void Delete(string x);
	}

	public static class ClearableViewExtensions
	{
		// ScanView is used to scan all the keys in a view iteratively
		//~ func ScanView(view ClearableView, cb func(path string)) error {
		public static void ScanView(this IClearableView view, Action<string> cb)
		{
			//~ frontier := []string{""}
			var frontier = new string[] { "" };

			//~for len(frontier) > 0 {
			while (frontier.Length > 0)
			{
				//~ n := len(frontier)
				//~ current := frontier[n-1]
				//~ frontier = frontier[:n-1]
				var n = frontier.Length;
				var current = frontier[n - 1];
				frontier = frontier.Take(n - 1).ToArray();

				// List the contents
				//~ contents, err := view.List(current)
				//~ if err != nil {
				//~ 	return fmt.Errorf("list failed at path '%s': %v", current, err)
				//~ }
				var contents = view.List(current);

				// Handle the contents in the directory
				//~ for _, c := range contents {
				//~ 	fullPath := current + c
				//~ 	if strings.HasSuffix(c, "/") {
				//~ 		frontier = append(frontier, fullPath)
				//~ 	} else {
				//~ 		cb(fullPath)
				//~ 	}
				//~ }
				foreach (var c in contents)
				{
					var fullPath = current + c;
					if (c.EndsWith("/"))
						frontier = frontier.Append(fullPath).ToArray();
					else
						cb(fullPath);
				}
			}
			//~return nil
		}

		// CollectKeys is used to collect all the keys in a view
		//~ func CollectKeys(view ClearableView) ([]string, error) {
		public static IEnumerable<string> CollectKeys(this IClearableView view)
		{
			// Accumulate the keys
			//~ var existing []string
			//~ cb := func(path string) {
			//~ 	existing = append(existing, path)
			//~ }
			var existing = new List<string>();
			Action<string> cb = s => existing.Add(s);

			// Scan for all the keys
			//~ if err := ScanView(view, cb); err != nil {
			//~ 	return nil, err
			//~ }
			//~ return existing, nil
			ScanView(view, cb);
			return existing;
		}

		// ClearView is used to delete all the keys in a view
		//~ func ClearView(view ClearableView) error {
		public static void ClearView(this IClearableView view)
		{
			// Collect all the keys
			//keys, err := CollectKeys(view)
			//if err != nil {
			//	return err
			//}
			var keys = CollectKeys(view);

			// Delete all the keys
			//~ for _, key := range keys {
			//~ 	if err := view.Delete(key); err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			//~ return nil
			foreach (var key in keys)
				view.Delete(key);
		}
	}
}
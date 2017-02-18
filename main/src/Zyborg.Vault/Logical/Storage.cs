namespace Zyborg.Vault.Logical
{
    // Storage is the way that logical backends are able read/write data.
    public interface IStorage
    {
        string[] List(string prefix);

        StorageEntry Get(string x);

        void Put(StorageEntry x);
        
        void Delete(string x);
    }

    // StorageEntry is the entry for an item in a Storage implementation.
    public struct StorageEntry
    {
        public StorageEntry(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public string Key
        { get; }

        public byte[] Value
        { get; }
    }

// // DecodeJSON decodes the 'Value' present in StorageEntry.
// func (e *StorageEntry) DecodeJSON(out interface{}) error {
//     return jsonutil.DecodeJSON(e.Value, out)
// }

// // StorageEntryJSON creates a StorageEntry with a JSON-encoded value.
// func StorageEntryJSON(k string, v interface{}) (*StorageEntry, error) {
//     encodedBytes, err := jsonutil.EncodeJSON(v)
//     if err != nil {
//         return nil, fmt.Errorf("failed to encode storage entry: %v", err)
//     }

//     return &StorageEntry{
//         Key:   k,
//         Value: encodedBytes,
//     }, nil
// }

        public interface IClearableView
        {
            string[] List(string x);

            void Delete(string x);
        }

// // ScanView is used to scan all the keys in a view iteratively
// func ScanView(view ClearableView, cb func(path string)) error {
// 	frontier := []string{""}
// 	for len(frontier) > 0 {
// 		n := len(frontier)
// 		current := frontier[n-1]
// 		frontier = frontier[:n-1]

// 		// List the contents
// 		contents, err := view.List(current)
// 		if err != nil {
// 			return fmt.Errorf("list failed at path '%s': %v", current, err)
// 		}

// 		// Handle the contents in the directory
// 		for _, c := range contents {
// 			fullPath := current + c
// 			if strings.HasSuffix(c, "/") {
// 				frontier = append(frontier, fullPath)
// 			} else {
// 				cb(fullPath)
// 			}
// 		}
// 	}
// 	return nil
// }

// // CollectKeys is used to collect all the keys in a view
// func CollectKeys(view ClearableView) ([]string, error) {
// 	// Accumulate the keys
// 	var existing []string
// 	cb := func(path string) {
// 		existing = append(existing, path)
// 	}

// 	// Scan for all the keys
// 	if err := ScanView(view, cb); err != nil {
// 		return nil, err
// 	}
// 	return existing, nil
// }

// // ClearView is used to delete all the keys in a view
// func ClearView(view ClearableView) error {
// 	// Collect all the keys
// 	keys, err := CollectKeys(view)
// 	if err != nil {
// 		return err
// 	}

// 	// Delete all the keys
// 	for _, key := range keys {
// 		if err := view.Delete(key); err != nil {
// 			return err
// 		}
// 	}
// 	return nil
// }

}
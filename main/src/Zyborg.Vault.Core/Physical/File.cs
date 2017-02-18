using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Zyborg.Vault.Physical
{
	// // newFileBackend constructs a Filebackend using the given directory
	//~ func newFileBackend(conf map[string]string, logger log.Logger) (Backend, error) {
	//~     path, ok := conf["path"]
	//~     if !ok {
	//~         return nil, fmt.Errorf("'path' must be set")
	//~     }
	//~
	//~     return &FileBackend{
	//~         Path:   path,
	//~         logger: logger,
	//~     }, nil
	//~ }
	public class FileBackendFactory : IBackendFactory
	{
		public static readonly FileBackendFactory INSTANCE = new FileBackendFactory();

		public IBackend CreateBackend(IReadOnlyDictionary<string, string> conf, ILogger logger)
		{
			if (!conf.ContainsKey(nameof(FileBackend.Path)))
				//~ return nil, fmt.Errorf("'path' must be set")
				throw new Exception($"missing configuration parameter [{nameof(FileBackend.Path)}]");

			return new FileBackend(logger, conf[nameof(FileBackend.Path)]);
		}
	}

	// FileBackend is a physical backend that stores data on disk
	// at a given file path. It can be used for durable single server
	// situations, or to develop locally where durability is not critical.
	//
	// WARNING: the file backend implementation is currently extremely unsafe
	// and non-performant. It is meant mostly for local testing and development.
	// It can be improved in the future.
	public class FileBackend : IBackend
    {
		// This is the path separater used by clients
		// to navigate the tree-structured namespace
		public const char PathSeparator = '/';

		private ILogger _logger;
        private Mutex _l = new Mutex();

        public FileBackend(ILogger logger, string path)
        {
            _logger = logger;
            Path = path;
        }

		public string Path
		{ get; private set; }

		public void Delete(string path)
        {
			//~ if path == "" {
			//~ 	return nil
			//~ }
			if (string.IsNullOrEmpty(path))
                return;

			using (var defer = new Util.Defer())
			{

				//~ b.l.Lock()
				//~ defer b.l.Unlock()
				_l.WaitOne();
				defer.Add(() => _l.ReleaseMutex());

				//~ basePath, key := b.path(path)
				//~ fullPath := filepath.Join(basePath, key)
				var (basePath, key) = GetPath(path);
				var fullPath = System.IO.Path.Combine(basePath, key);

				//~ err := os.Remove(fullPath)
				//~ if err != nil && !os.IsNotExist(err) {
				//~ 	return fmt.Errorf("Failed to remove %q: %v", fullPath, err)
				//~ }
				//~ 
				//~ err = b.cleanupLogicalPath(path)
				//~ 
				//~ return err
				File.Delete(fullPath);
                CleanupLogicalPath(path);
            }
        }

        // cleanupLogicalPath is used to remove all empty nodes, begining with deepest
        // one, aborting on first non-empty one, up to top-level node.
        protected void CleanupLogicalPath(string path)
        {
            //~ nodes := strings.Split(path,  fmt.Sprintf("%c", os.PathSeparator))
            var nodes = path.Split(PathSeparator);

			//~ for i := len(nodes) - 1; i > 0; i-- {
			for (var i = nodes.Length - 1; i > 0; i--)
            {
                //~ fullPath := filepath.Join(b.Path, filepath.Join(nodes[:i]...))
                //~ dir, err := os.Open(fullPath)
                //~ if err != nil {
                //~     if os.IsNotExist(err) {
                //~         return nil
                //~     } else {
                //~         return err
                //~     }
                //~ }

                var fullPath = System.IO.Path.Combine(Path, System.IO.Path.Combine(nodes.Take(i).ToArray()));
                try
                {
                    var dir = Directory.GetFileSystemEntries(fullPath);

                    //~ list, err := dir.Readdir(1)
                    //~ dir.Close()
                    //~ if err != nil && err != io.EOF {
                    //~     return err
                    //~ }

                    // If we have no entries, it's an empty directory; remove it
                    //~ if err == io.EOF || list == nil || len(list) == 0 {
                    //~     err = os.Remove(fullPath)
                    //~     if err != nil {
                    //~         return err
                    //~     }
                    //~ }

                    if (dir.Length == 0)
                    {
                        Directory.Delete(fullPath);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    return;
                }
            }
        }

        public Entry Get(string k)
        {
            //~ b.l.Lock()
            //~ defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                var (path, key) = GetPath(k);
                path = System.IO.Path.Combine(path, key);

                if (!File.Exists(path))
                    return null;
                
                //~ f, err := os.Open(path)
                //~ if err != nil {
                //~     if os.IsNotExist(err) {
                //~         return nil, nil
                //~     }

                //~     return nil, err
                //~ }
                //~ defer f.Close()

                //~ var entry Entry
                //~ if err := jsonutil.DecodeJSONFromReader(f, &entry); err != nil {
                //~     return nil, err
                //~ }

                //~ return &entry, nil
                return JsonConvert.DeserializeObject<Entry>(File.ReadAllText(path));
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        public void Put(Entry entry)
        {
            var (path, key) = GetPath(entry.Key);

            //~ b.l.Lock()
            //~ defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                // Make the parent tree
                //~ if err := os.MkdirAll(path, 0755); err != nil {
                //~     return err
                //~ }
                Directory.CreateDirectory(path);

				// JSON encode the entry and write it
				//~ f, err := os.OpenFile(
				//~     filepath.Join(path, key),
				//~     os.O_CREATE|os.O_TRUNC|os.O_WRONLY,
				//~     0600)
				//~ if err != nil {
				//~     return err
				//~ }
				//~ defer f.Close()
				//~ enc := json.NewEncoder(f)
				//~ return enc.Encode(entry)
				File.WriteAllText(System.IO.Path.Combine(path, key),
						JsonConvert.SerializeObject(entry));
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        public IEnumerable<string> List(string prefix)
        {
            // b.l.Lock()
            // defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                var path = Path;
                if (!string.IsNullOrEmpty(prefix))
                    path = System.IO.Path.Combine(path, prefix);

                // Read the directory contents
                if (!Directory.Exists(path))
                    yield break;

				var dir = new DirectoryInfo(path);
                foreach (var fsi in dir.EnumerateFileSystemInfos())
                {
					var n = fsi.Name;
					if (n.StartsWith("_"))
						yield return n.Substring(1);
                    else
                        yield return n + "/";
                }
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        (string, string) GetPath(string k)
        {
            var path = System.IO.Path.Combine(Path, k);
            var key = System.IO.Path.GetFileName(path);
            path = System.IO.Path.GetDirectoryName(path);
            return ValueTuple.Create(path, "_" + key);
        }
    }
}
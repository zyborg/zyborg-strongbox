using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Zyborg.Vault.Physical
{

    // FileBackend is a physical backend that stores data on disk
    // at a given file path. It can be used for durable single server
    // situations, or to develop locally where durability is not critical.
    //
    // WARNING: the file backend implementation is currently extremely unsafe
    // and non-performant. It is meant mostly for local testing and development.
    // It can be improved in the future.
    public class FileBackend : IBackend
    {
        ILogger _logger;
        string _path;
        Mutex _l = new Mutex();

        public FileBackend(ILogger logger, string path)
        {
            _logger = logger;
            _path = path;
        }


        public void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            // b.l.Lock()
            // defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                string basePath;
                var basePath_key = GetPath(path);
                var basePath = basePath_key.Item1;
                var key = basePath_key.Item2;
                var fullPath = Path.Combine(basePath, key);

                File.Delete(fullPath);
                CleanupLogicalPath(path);
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        // cleanupLogicalPath is used to remove all empty nodes, begining with deepest
        // one, aborting on first non-empty one, up to top-level node.
        protected void CleanupLogicalPath(string path)
        {
            // nodes := strings.Split(path,  fmt.Sprintf("%c", os.PathSeparator))
            var nodes = path.Split(System.IO.Path.DirectorySeparatorChar);

            for (var i = nodes.Length - 1; i > 0; i--)
            {
                // fullPath := filepath.Join(b.Path, filepath.Join(nodes[:i]...))
                // dir, err := os.Open(fullPath)
                // if err != nil {
                //     if os.IsNotExist(err) {
                //         return nil
                //     } else {
                //         return err
                //     }
                // }

                var fullPath = Path.Combine(_path, Path.Combine(nodes.Take(i - 1).ToArray()));
                try
                {
                    var dir = Directory.GetFileSystemEntries(fullPath);

                    // list, err := dir.Readdir(1)
                    // dir.Close()
                    // if err != nil && err != io.EOF {
                    //     return err
                    // }

                    // If we have no entries, it's an empty directory; remove it
                    // if err == io.EOF || list == nil || len(list) == 0 {
                    //     err = os.Remove(fullPath)
                    //     if err != nil {
                    //         return err
                    //     }
                    // }

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
            // b.l.Lock()
            // defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                string path;
                string key = GetPath(k, out path);

                path = Path.Combine(path, key);

                if (!File.Exists(path))
                    return null;
                
                // f, err := os.Open(path)
                // if err != nil {
                //     if os.IsNotExist(err) {
                //         return nil, nil
                //     }

                //     return nil, err
                // }
                // defer f.Close()

                // var entry Entry
                // if err := jsonutil.DecodeJSONFromReader(f, &entry); err != nil {
                //     return nil, err
                // }

                // return &entry, nil
                return JsonConvert.DeserializeObject<Entry>(File.ReadAllText(path));
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        public void Put(Entry entry)
        {
            string path;
            string key = GetPath(entry.Key, out path);

            // b.l.Lock()
            // defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                // Make the parent tree
                // if err := os.MkdirAll(path, 0755); err != nil {
                //     return err
                // }
                Directory.CreateDirectory(path);

                // JSON encode the entry and write it
                // f, err := os.OpenFile(
                //     filepath.Join(path, key),
                //     os.O_CREATE|os.O_TRUNC|os.O_WRONLY,
                //     0600)
                // if err != nil {
                //     return err
                // }
                // defer f.Close()
                // enc := json.NewEncoder(f)
                // return enc.Encode(entry)
                File.WriteAllText(Path.Combine(path, key), JsonConvert.SerializeObject(entry));
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        public string[] List(string prefix)
        {
            // b.l.Lock()
            // defer b.l.Unlock()
            _l.WaitOne();
            try
            {
                var path = _path;
                if (!string.IsNullOrEmpty(prefix))
                {
                    path = Path.Combine(path, prefix);
                }

                // Read the directory contents
                if (!Directory.Exists(path))
                    return null;


                var names = new List<string>();
                
                foreach (var n in Directory.GetFileSystemEntries(path))
                {
                    if (n.StartsWith("_"))
                        names.Add(n.Substring(1));
                    else
                        names.Add(n + "/");
                }

                return names.ToArray();
            }
            finally
            {
                _l.ReleaseMutex();
            }
        }

        ValueTuple<string, string> GetPath(string k)
        {
            var path = Path.Combine(_path, k);
            var key = Path.GetFileName(path);
            path = Path.GetDirectoryName(path);
            return ValueTuple.Create(path, "_" + key);
        }
    }

    // // newFileBackend constructs a Filebackend using the given directory
    // func newFileBackend(conf map[string]string, logger log.Logger) (Backend, error) {
    //     path, ok := conf["path"]
    //     if !ok {
    //         return nil, fmt.Errorf("'path' must be set")
    //     }

    //     return &FileBackend{
    //         Path:   path,
    //         logger: logger,
    //     }, nil
    // }
    public class FileBackendFactory : IBackendFactory
    {
        public IBackend CreateBackend(IDictionary<string, string> conf, ILogger logger)
        {
            if (!conf.ContainsKey(nameof(FileBackend._path)))
                throw new Exception($"missing configuration parameter [{nameof(FileBackend._path)}]");

            return new FileBackend(logger, conf[nameof(FileBackend._path)]);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Zyborg.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Zyborg.Vault.Physical
{
	//// NewInmem constructs a new in-memory backend
	//~ func NewInmem(logger log.Logger) *InmemBackend {
	//~ 	in := &InmemBackend{
	//~ 		root:       radix.New(),
	//~ 		permitPool: NewPermitPool(DefaultParallelOperations),
	//~ 		logger:     logger,
	//~ 	}
	//~ 	return in
	//~ }
	public class InmemBackendFactory : IBackendFactory
	{
		public static readonly InmemBackendFactory INSTANCE = new InmemBackendFactory();

		public IBackend CreateBackend(IReadOnlyDictionary<string, string> config, ILogger logger)
		{
			var @in = new InmemBackend(logger);
			return @in;
		}
	}

	//// InmemBackend is an in-memory only physical backend. It is useful
	//// for testing and development situations where the data is not
	//// expected to be durable.
	//~ type InmemBackend struct {
	//~ 	root       *radix.Tree
	//~ 	l          sync.RWMutex
	//~ 	permitPool *PermitPool
	//~ 	logger     log.Logger
	//~ }
	public class InmemBackend : IBackend
	{
		private ILogger _logger;
		ReaderWriterLockSlim _l = new ReaderWriterLockSlim();
		RadixTree<Entry> _root = new RadixTree<Entry>();
		PermitPool _permitPool = new PermitPool(Constants.DefaultParallelOperations);


		public InmemBackend(ILogger logger)
		{
			_logger = logger;
		}

		public void Put(Entry entry)
		{
			PutAsync(entry).Wait();
		}

		public Entry Get(string key)
		{
			return GetAsync(key).Result;
		}

		public void Delete(string key)
		{
			DeleteAsync(key).Wait();
		}

		public IEnumerable<string> List(string prefix)
		{
			return ListAsync(prefix).Result;
		}

		// Put is used to insert or update an entry
		//~ func (i *InmemBackend) Put(entry *Entry) error {
		public async Task PutAsync(Entry entry)
		{
			//~ i.permitPool.Acquire()
			//~ defer i.permitPool.Release()
			//~ 
			//~ i.l.Lock()
			//~ defer i.l.Unlock()
			//~ 
			//~ i.root.Insert(entry.Key, entry)
			//~ return nil

			using (var defer = new Util.Defer())
			{
				await _permitPool.Acquire();
				defer.Add(async () => await _permitPool.Release());

				_l.EnterWriteLock();
				defer.Add(() => _l.ExitWriteLock());

				_root.GoInsert(entry.Key, entry);
			}
		}

		// Get is used to fetch an entry
		//~ func (i *InmemBackend) Get(key string) (*Entry, error) {
		public async Task<Entry> GetAsync(string key)
		{
			//~ i.permitPool.Acquire()
			//~ defer i.permitPool.Release()
			//~ 
			//~ i.l.RLock()
			//~ defer i.l.RUnlock()
			//~ 
			//~ if raw, ok := i.root.Get(key); ok {
			//~ 	return raw.(*Entry), nil
			//~ }
			//~ return nil, nil
			using (var defer = new Util.Defer())
			{
				await _permitPool.Acquire();
				defer.Add(async () => await _permitPool.Release());

				_l.EnterReadLock();
				defer.Add(() => _l.ExitReadLock());

				return _root.GoGet(key).value;
			}
		}

		// Delete is used to permanently delete an entry
		//~ func (i *InmemBackend) Delete(key string) error {
		public async Task DeleteAsync(string key)
		{
			//~ i.permitPool.Acquire()
			//~ defer i.permitPool.Release()
			//~ 
			//~ i.l.Lock()
			//~ defer i.l.Unlock()
			//~ 
			//~ i.root.Delete(key)
			//~ return nil
			using (var defer = new Util.Defer())
			{
				await _permitPool.Acquire();
				defer.Add(async () => await _permitPool.Release());

				_l.EnterWriteLock();
				defer.Add(() => _l.ExitWriteLock());

				_root.GoDelete(key);
			}
		}

		// List is used ot list all the keys under a given
		// prefix, up to the next prefix.
		//~ func (i *InmemBackend) List(prefix string) ([]string, error) {
		public async Task<IEnumerable<string>> ListAsync(string prefix)
		{
			using (var defer = new Util.Defer())
			{
				//~ i.permitPool.Acquire()
				//~ defer i.permitPool.Release()
				await _permitPool.Acquire();

				//~ i.l.RLock()
				//~ defer i.l.RUnlock()
				_l.EnterReadLock();
				defer.Add(() => _l.ExitReadLock());

				//~ var out []string
				//~ seen := make(map[string]interface{})
				var @out = new List<string>();
				var seen = new Dictionary<string, bool>();

				//~ walkFn := func(s string, v interface{}) bool {
				//~ 	trimmed := strings.TrimPrefix(s, prefix)
				//~ 	sep := strings.Index(trimmed, "/")
				//~ 	if sep == -1 {
				//~ 		out = append(out, trimmed)
				//~ 	} else {
				//~ 		trimmed = trimmed[:sep+1]
				//~ 		if _, ok := seen[trimmed]; !ok {
				//~ 			out = append(out, trimmed)
				//~ 			seen[trimmed] = struct{}{}
				//~ 		}
				//~ 	}
				//~ 	return false
				//~ }
				Walker<Entry> walkFn = (s, v) =>
				{
					var trimmed = s.StartsWith(prefix) ? s.Substring(prefix.Length) : s;
					var sep = trimmed.IndexOf('/');
					if (sep == -1)
					{
						@out.Add(trimmed);
					}
					else
					{
						trimmed = trimmed.Substring(0, sep + 1);
						if (!seen.ContainsKey(trimmed))
						{
							@out.Add(trimmed);
							seen[trimmed] = true;
						}
					}
					return false;
				};

				//~ i.root.WalkPrefix(prefix, walkFn)
				//~ return out, nil
				_root.WalkPrefix(prefix, walkFn);
				return @out;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Util.Threading;

namespace Zyborg.Vault.Logical
{

	// InmemStorage implements Storage and stores all data in memory.
	//~type InmemStorage struct {
	public class InmemStorage : IStorage
	{
		//~ phys *physical.InmemBackend
		Physical.IBackend _phys;

		//~ once sync.Once
		Once _once = new Once();

		public IStorage DeepCopy()
		{
			throw new NotImplementedException();
		}

		//~ func (s *InmemStorage) List(prefix string) ([]string, error) {
		public IEnumerable<string> List(string prefix)
		{
			//~ s.once.Do(s.init)
			//~ 
			//~ return s.phys.List(prefix)
			_once.Execute(Init);
			return _phys.List(prefix);
		}

		//~ func (s *InmemStorage) Get(key string) (*StorageEntry, error) {
		public StorageEntry Get(string key)
		{
			//~s.once.Do(s.init)
			_once.Execute(Init);

			//~ entry, err := s.phys.Get(key)
			//~ if err != nil {
			//~ 	return nil, err
			//~ }
			//~ if entry == nil {
			//~ 	return nil, nil
			//~ }
			var entry = _phys.Get(key);
			if (entry == null)
				return null;

			//~return &StorageEntry{
			//~	Key:   entry.Key,
			//~	Value: entry.Value,
			//~}, nil
			return new StorageEntry
			{
				Key = entry.Key,
				Value = entry.Value,
			};
		}

		//~ func (s *InmemStorage) Put(entry *StorageEntry) error {
		public void Put(StorageEntry entry)
		{
			//~ s.once.Do(s.init)
			_once.Execute(Init);
			//~ physEntry := &physical.Entry{
			//~ 	Key:   entry.Key,
			//~ 	Value: entry.Value,
			//~ }
			//~ return s.phys.Put(physEntry)
			_phys.Put(new Physical.Entry
			{
				Key = entry.Key,
				Value = entry.Value,
			});
		}

		//~ func (s *InmemStorage) Delete(k string) error {
		public void Delete(string k)
		{
			//~ s.once.Do(s.init)
			_once.Execute(Init);
			//~ return s.phys.Delete(k)
			_phys.Delete(k);
		}

		//~ func (s *InmemStorage) init() {
		private void Init()
		{
			//~ s.phys = physical.NewInmem(nil)
			_phys = Physical.InmemBackendFactory.INSTANCE.CreateBackend(null, null);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault
{
	// BarrierView wraps a SecurityBarrier and ensures all access is automatically
	// prefixed. This is used to prevent anyone with access to the view to access
	// any data in the durable storage outside of their prefix. Conceptually this
	// is like a "chroot" into the barrier.
	//
	// BarrierView implements logical.Storage so it can be passed in as the
	// durable storage mechanism for logical views.
	//~ type BarrierView struct {
	public class BarrierView
	{
		//~ barrier  BarrierStorage
		//~ prefix   string
		//~ readonly bool
		internal IBarrierStorage barrier;
		internal string prefix;
		internal bool readOnly;

		// NewBarrierView takes an underlying security barrier and returns
		// a view of it that can only operate with the given prefix.
		//~ func NewBarrierView(barrier BarrierStorage, prefix string) *BarrierView {
		public static BarrierView NewBarrierView(IBarrierStorage barrier, string prefix)
		{
			//~ return &BarrierView{
			//~ 	barrier: barrier,
			//~ 	prefix:  prefix,
			//~ }
			return new BarrierView
			{
				barrier = barrier,
				prefix = prefix,
			};
		}

		// sanityCheck is used to perform a sanity check on a key
		//~ func (v *BarrierView) sanityCheck(key string) error {
		internal void sanityCheck(string key)
		{
			//~ if strings.Contains(key, "..") {
			//~ 	return fmt.Errorf("key cannot be relative path")
			//~ }
			//~ return nil
			if (key.Contains(".."))
				throw new Exception("key cannot be relative path");
		}

		// logical.Storage impl.
		//~ func (v *BarrierView) List(prefix string) ([]string, error) {
		public string[] List(string prefix)
		{
			//~ if err := v.sanityCheck(prefix); err != nil {
			//~ 	return nil, err
			//~ }
			//~ return v.barrier.List(v.expandKey(prefix))
			sanityCheck(prefix);
			return barrier.List(expandKey(prefix));
		}

		// logical.Storage impl.
		//~ func (v *BarrierView) Get(key string) (*logical.StorageEntry, error) {
		public Logical.StorageEntry Get(string key)
		{
			//~ if err := v.sanityCheck(key); err != nil {
			//~ 	return nil, err
			//~ }
			sanityCheck(key);
			//~ entry, err := v.barrier.Get(v.expandKey(key))
			//~ if err != nil {
			//~ 	return nil, err
			//~ }
			//~ if entry == nil {
			//~ 	return nil, nil
			//~ }
			//~ if entry != nil {
			//~ 	entry.Key = v.truncateKey(entry.Key)
			//~ }
			var entry = barrier.Get(expandKey(key));
			if (entry == null)
				return null;
			entry.Key = truncateKey(entry.Key);

			//~ return &logical.StorageEntry{
			//~ 	Key:   entry.Key,
			//~ 	Value: entry.Value,
			//~ }, nil
			return new Logical.StorageEntry
			{
				Key = entry.Key,
				Value = entry.Value,
			};
		}

		// logical.Storage impl.
		//~ func (v *BarrierView) Put(entry *logical.StorageEntry) error {
		public void Put(Logical.StorageEntry entry)
		{
			//~ if err := v.sanityCheck(entry.Key); err != nil {
			//~ 	return err
			//~ }
			sanityCheck(entry.Key);

			//~ expandedKey := v.expandKey(entry.Key)
			var expandedKey = expandKey(entry.Key);

			//~ if v.readonly {
			//~ 	return logical.ErrReadOnly
			//~ }
			if (readOnly)
				throw new Logical.Globals.ErrReadOnly();

			//~ nested := &Entry{
			//~ 	Key:   expandedKey,
			//~ 	Value: entry.Value,
			//~ }
			//~ return v.barrier.Put(nested)
			var nested = new Entry
			{
				Key = expandedKey,
				Value = entry.Value,
			};
			barrier.Put(nested);
		}

		// logical.Storage impl.
		//~ func (v *BarrierView) Delete(key string) error {
		public void Delete(string key)
		{
			//~ if err := v.sanityCheck(key); err != nil {
			//~ 	return err
			//~ }
			sanityCheck(key);

			//~ expandedKey := v.expandKey(key)
			var expandedKey = expandKey(key);

			//~ if v.readonly {
			//~ 	return logical.ErrReadOnly
			//~ }
			if (readOnly)
				throw new Logical.Globals.ErrReadOnly();

			//~ return v.barrier.Delete(expandedKey)
			barrier.Delete(expandedKey);
		}

		// SubView constructs a nested sub-view using the given prefix
		//~ func (v *BarrierView) SubView(prefix string) *BarrierView {
		public BarrierView SubView(string prefix)
		{
			//~ sub := v.expandKey(prefix)
			//~ return &BarrierView{barrier: v.barrier, prefix: sub, readonly: v.readonly}
			var sub = expandKey(prefix);
			return new BarrierView
			{
				barrier = this.barrier,
				prefix = sub,
				readOnly = this.readOnly,
			};
		}

		// expandKey is used to expand to the full key path with the prefix
		//~ func (v *BarrierView) expandKey(suffix string) string {
		internal string expandKey(string suffix)
		{
			//~ return v.prefix + suffix
			return prefix + suffix;
		}

		// truncateKey is used to remove the prefix of the key
		//~ func (v *BarrierView) truncateKey(full string) string {
		internal string truncateKey(string full)
		{
			//~ return strings.TrimPrefix(full, v.prefix)
			if (full.StartsWith(prefix))
				return full.Substring(prefix.Length);
			return full;
		}
	}
}

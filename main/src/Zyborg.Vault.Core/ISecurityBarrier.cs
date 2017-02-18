using System;

namespace Zyborg.Vault
{
    // SecurityBarrier is a critical component of Vault. It is used to wrap
    // an untrusted physical backend and provide a single point of encryption,
    // decryption and checksum verification. The goal is to ensure that any
    // data written to the barrier is confidential and that integrity is preserved.
    // As a real-world analogy, this is the steel and concrete wrapper around
    // a Vault. The barrier should only be Unlockable given its key.
    public interface ISecurityBarrier
    {
        // Initialized checks if the barrier has been initialized
        // and has a master key set.
        bool IsInitialized
        { get; }

        // Initialize works only if the barrier has not been initialized
        // and makes use of the given master key.
        void Initialize(byte[] mkey);

        // GenerateKey is used to generate a new key
        byte[] GenerateKey();

        // KeyLength is used to sanity check a key
        Tuple<int, int> KeyLength();

        // Sealed checks if the barrier has been unlocked yet. The Barrier
        // is not expected to be able to perform any CRUD until it is unsealed.
        bool IsSealed
        { get; }

        // Unseal is used to provide the master key which permits the barrier
        // to be unsealed. If the key is not correct, the barrier remains sealed.
        void Unseal(byte[] mkey);

        // VerifyMaster is used to check if the given key matches the master key
        void VerifyMaster(byte[] mkey);

        // ReloadKeyring is used to re-read the underlying keyring.
        // This is used for HA deployments to ensure the latest keyring
        // is present in the leader.
        void ReloadKeyring();

        // ReloadMasterKey is used to re-read the underlying masterkey.
        // This is used for HA deployments to ensure the latest master key
        // is available for keyring reloading.
        void ReloadMasterKey();

        // Seal is used to re-seal the barrier. This requires the barrier to
        // be unsealed again to perform any further operations.
        void Seal();

        // Rotate is used to create a new encryption key. All future writes
        // should use the new key, while old values should still be decryptable.
        uint Rotate();

        // CreateUpgrade creates an upgrade path key to the given term from the previous term
        void CreateUpgrade(uint term);

        // DestroyUpgrade destroys the upgrade path key to the given term
        void DestroyUpgrade(uint term);

        // CheckUpgrade looks for an upgrade to the current term and installs it
        Tuple<bool, uint> CheckUpgrade();

        // ActiveKeyInfo is used to inform details about the active key
        KeyInfo ActiveKeyInfo();

        // Rekey is used to change the master key used to protect the keyring
        void Rekey(byte[] unk);

        // SecurityBarrier must provide the storage APIs
        IBarrierStorage Storage
        { get; }
    }


    // BarrierStorage is the storage only interface required for a Barrier.
    public interface IBarrierStorage
    {
        // Put is used to insert or update an entry
        void Put(Entry entry);

        // Get is used to fetch an entry
        Entry Get(string key);

        // Delete is used to permanently delete an entry
        void Delete(string key);

        // List is used ot list all the keys under a given
        // prefix, up to the next prefix.
        string[] List(string prefix);
    }

    // Entry is used to represent data stored by the security barrier
    public struct Entry
    {
        public Entry(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        string Key
        { get; }

        byte[] Value
        { get; }

        // Logical turns the Entry into a logical storage entry.
        Logical.StorageEntry Logical()
        {
            return new Logical.StorageEntry
			{
				Key = Key,
				Value = Value,
			};
        }
    }

    // KeyInfo is used to convey information about the encryption key
    public struct KeyInfo
    {
        public KeyInfo(int term, DateTime installTime)
        {
            Term = term;
            InstallTime = installTime;
        }

        public int Term
        { get; }
        public DateTime InstallTime
        { get; }
    }
}

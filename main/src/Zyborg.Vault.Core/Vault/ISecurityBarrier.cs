using System;

namespace Zyborg.Vault
{
	public static partial class Contants
	{
		//~ const (
		// barrierInitPath is the path used to store our init sentinel file
		//~ barrierInitPath = "barrier/init"
		public const string barrierInitPath = "barrier/init";

		// keyringPath is the location of the keyring data. This is encrypted
		// by the master key.
		//~ keyringPath = "core/keyring"
		public const string keyringPath = "core/keyring";

		// keyringUpgradePrefix is the path used to store keyring update entries.
		// When running in HA mode, the active instance will install the new key
		// and re-write the keyring. For standby instances, they need an upgrade
		// path from key N to N+1. They cannot just use the master key because
		// in the event of a rekey, that master key can no longer decrypt the keyring.
		// When key N+1 is installed, we create an entry at "prefix/N" which uses
		// encryption key N to provide the N+1 key. The standby instances scan
		// for this periodically and refresh their keyring. The upgrade keys
		// are deleted after a few minutes, but this provides enough time for the
		// standby instances to upgrade without causing any disruption.
		//~ keyringUpgradePrefix = "core/upgrade/"
		public const string keyringUpgradePrefix = "core/upgrade/";

		// masterKeyPath is the location of the master key. This is encrypted
		// by the latest key in the keyring. This is only used by standby instances
		// to handle the case of a rekey. If the active instance does a rekey,
		// the standby instances can no longer reload the keyring since they
		// have the old master key. This key can be decrypted if you have the
		// keyring to discover the new master key. The new master key is then
		// used to reload the keyring itself.
		//~ masterKeyPath = "core/master"
		public const string masterKeyPath = "core/master";
	}

	public static partial class Globals
	{
		//~ var (
		// ErrBarrierSealed is returned if an operation is performed on
		// a sealed barrier. No operation is expected to succeed before unsealing
		//~ ErrBarrierSealed = errors.New("Vault is sealed")
		public class ErrBarrierSealed : Exception
		{
			public ErrBarrierSealed() : base("Vault is sealed") { }
		}

		// ErrBarrierAlreadyInit is returned if the barrier is already
		// initialized. This prevents a re-initialization.
		//~ ErrBarrierAlreadyInit = errors.New("Vault is already initialized")
		public class ErrBarrierAlreadyInit : Exception
		{
			public ErrBarrierAlreadyInit() : base("Vault is already initialized") { }
		}

		// ErrBarrierNotInit is returned if a non-initialized barrier
		// is attempted to be unsealed.
		//~ ErrBarrierNotInit = errors.New("Vault is not initialized")
		public class ErrBarrierNotInit : Exception
		{
			public ErrBarrierNotInit() : base("Vault is not initialized") { }
		}

		// ErrBarrierInvalidKey is returned if the Unseal key is invalid
		//~ ErrBarrierInvalidKey = errors.New("Unseal failed, invalid key")
		public class ErrBarrierInvalidKey : Exception
		{
			public ErrBarrierInvalidKey() : base("Unseal failed, invalid key") { }
		}
	}

    // SecurityBarrier is a critical component of Vault. It is used to wrap
    // an untrusted physical backend and provide a single point of encryption,
    // decryption and checksum verification. The goal is to ensure that any
    // data written to the barrier is confidential and that integrity is preserved.
    // As a real-world analogy, this is the steel and concrete wrapper around
    // a Vault. The barrier should only be Unlockable given its key.
    public interface ISecurityBarrier : IBarrierStorage, IBarrierEncryptor
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
        (int, int) KeyLength();

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
		//~ IBarrierStorage Storage
		// IMPL: base interface

		// SecurityBarrier must provide the encryption APIs
		//~ BarrierEncryptor
		// IMPL: base interface
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

	// BarrierEncryptor is the in memory only interface that does not actually
	// use the underlying barrier. It is used for lower level modules like the
	// Write-Ahead-Log and Merkle index to allow them to use the barrier.
	//~ type BarrierEncryptor interface {
	public interface IBarrierEncryptor
	{
		byte[] Encrypt(string key, byte[] plaintext);
		byte[] Decrypt(string key, byte[] ciphertext);
	}

    // Entry is used to represent data stored by the security barrier
    public class Entry
    {
        public string Key
        { get; set; }

        public byte[] Value
        { get; set;  }

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
    public class KeyInfo
    {
        public int Term
        { get; set; }
        public DateTime InstallTime
        { get; set; }
    }
}

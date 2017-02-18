using System;
using Newtonsoft.Json;

namespace Zyborg.Vault
{

    public interface ISeal
    {
        void SetCore(Core x);
        void Init();
        void Finalize();

        bool StoredKeysSupported();
        void SetStoredKeys(byte[][] x);
        byte[][] GetStoredKeys();

        string BarrierType();
        SealConfig BarrierConfig();
        void SetBarrierConfig(SealConfig x);

        bool RecoveryKeySupported();
        string RecoveryType();
        SealConfig RecoveryConfig();
        void SetRecoveryConfig(SealConfig);
        void SetRecoveryKey(byte[] x);
        void VerifyRecoveryKey(byte x);
    }

    public class DefaultSeal
    {
	    public SealConfig Config;

        public Core Core;

        public void CheckCore()
        {
            if (Core == null)
                throw new InvalidOperationException("seal does not have a core set");
        }

        public void SetCore(Core core)
        {
            this.Core = core;
        }

        public void Init()
        { }

        public void Final() // Finalize
        { }

        public string BarrierType
        { get; } = "shamir";

        public bool StoredKeysSupported
        { get; } = false;

        public bool RecoveryKeySupported
        { get; } = false;

        public void SetStoredKeys(byte[][] keys)
        {
            throw new InvalidOperationException("core: stored keys are not supported");
        }

        public byte[][] GetStoredKeys()
        {
            throw new InvalidOperationException("core: stored keys are not supported");
        }

        public SealConfig BarrierConfig()
        {
            if (Config != null)
            {
                return Config.Clone();
            }

            CheckCore();
            // if err := d.checkCore(); err != nil {
            // 	return nil, err
            // }

            // // Fetch the core configuration
            // pe, err := d.core.physical.Get(barrierSealConfigPath)
            // if err != nil {
            // 	d.core.logger.Error("core: failed to read seal configuration", "error", err)
            // 	return nil, fmt.Errorf("failed to check seal configuration: %v", err)
            // }

            // // If the seal configuration is missing, we are not initialized
            // if pe == nil {
            // 	d.core.logger.Info("core: seal configuration missing, not initialized")
            // 	return nil, nil
            // }

            // var conf SealConfig

            // // Decode the barrier entry
            // if err := jsonutil.DecodeJSON(pe.Value, &conf); err != nil {
            // 	d.core.logger.Error("core: failed to decode seal configuration", "error", err)
            // 	return nil, fmt.Errorf("failed to decode seal configuration: %v", err)
            // }

            // switch conf.Type {
            // // This case should not be valid for other types as only this is the default
            // case "":
            // 	conf.Type = d.BarrierType()
            // case d.BarrierType():
            // default:
            // 	d.core.logger.Error("core: barrier seal type does not match loaded type", "barrier_seal_type", conf.Type, "loaded_seal_type", d.BarrierType())
            // 	return nil, fmt.Errorf("barrier seal type of %s does not match loaded type of %s", conf.Type, d.BarrierType())
            // }

            // // Check for a valid seal configuration
            // if err := conf.Validate(); err != nil {
            // 	d.core.logger.Error("core: invalid seal configuration", "error", err)
            // 	return nil, fmt.Errorf("seal validation failed: %v", err)
            // }

            // d.config = &conf
            return Config.Clone();
        }

        public void SetBarrierConfig(SealConfig config)
        {
            CheckCore();

            // Provide a way to wipe out the cached value (also prevents actually
            // saving a nil config)
            if (config == null)
            {
                Config = null;
                return;
            }

            config.Type = BarrierType;

            // Encode the seal configuration
            var buf = JsonConvert.SerializeObject(config); //json.Marshal(config);
            
            // if err != nil {
            // 	return fmt.Errorf("failed to encode seal configuration: %v", err)
            // }

            // // Store the seal configuration
            // pe := &physical.Entry{
            // 	Key:   barrierSealConfigPath,
            // 	Value: buf,
            // }

            // if err := d.core.physical.Put(pe); err != nil {
            // 	d.core.logger.Error("core: failed to write seal configuration", "error", err)
            // 	return fmt.Errorf("failed to write seal configuration: %v", err)
            // }

            Config = config.Clone();
        }

        public string RecoveryType()
        {
            return "unsupported";
        }

        public SealConfig RecoveryConfig()
        {
            throw new NotSupportedException("recovery not supported");
        }

        public void SetRecoveryConfig(SealConfig config)
        {
            throw new NotSupportedException("recovery not supported");
        }

        public void VerifyRecoveryKey(byte[] x)
        {
            throw new NotSupportedException("recovery not supported");
        }

        public void SetRecoveryKey(byte[] key)
        {
            throw new NotSupportedException("recovery not supported");
        }
    }

    // SealConfig is used to describe the seal configuration
    public class SealConfig
    {
        // The type, for sanity checking
        [JsonProperty("type")]
        public string Type
        { get; set; }

        // SecretShares is the number of shares the secret is split into. This is
        // the N value of Shamir.
        [JsonProperty("secret_shares")]
        public int SecretShares
        { get; set; }

        // SecretThreshold is the number of parts required to open the vault. This
        // is the T value of Shamir.
        [JsonProperty("secret_threshold")]
        public int SecretThreshold
        { get; set; }

        // PGPKeys is the array of public PGP keys used, if requested, to encrypt
        // the output unseal tokens. If provided, it sets the value of
        // SecretShares. Ordering is important.
        [JsonProperty("pgp_keys")]
        public string[] PGPKeys
        { get; set; }

        // Nonce is a nonce generated by Vault used to ensure that when unseal keys
        // are submitted for a rekey operation, the rekey operation itself is the
        // one intended. This prevents hijacking of the rekey operation, since it
        // is unauthenticated.
        [JsonProperty("nonce")]
        public string Nonce
        { get; set; }

        // Backup indicates whether or not a backup of PGP-encrypted unseal keys
        // should be stored at coreUnsealKeysBackupPath after successful rekeying.
        [JsonProperty("backup")]
        public bool Backup
        { get; set; }

        // How many keys to store, for seals that support storage.
        [JsonProperty("stored_shares")]
        public int StoredShares
        { get; set; }
    
        // Validate is used to sanity check the seal configuration
        public void Validate()
        {
            if (SecretShares < 1) {
                throw new Exception("shares must be at least one");
            }
            if (SecretThreshold < 1) {
                throw new Exception("threshold must be at least one");
            }
            if (SecretShares > 1 && SecretThreshold == 1) {
                throw new Exception("threshold must be greater than one for multiple shares");
            }
            if (SecretShares > 255) {
                throw new Exception("shares must be less than 256");
            }
            if (SecretThreshold > 255) {
                throw new Exception("threshold must be less than 256");
            }
            if (SecretThreshold > SecretShares) {
                throw new Exception("threshold cannot be larger than shares");
            }
            if (StoredShares > SecretShares) {
                throw new Exception("stored keys cannot be larger than shares");
            }
            if (PGPKeys.Length > 0 && PGPKeys.Length != SecretShares - StoredShares) {
                throw new Exception("count mismatch between number of provided PGP keys and number of shares");
            }
            if (PGPKeys.Length > 0) {
                foreach (var keystring in PGPKeys)
                {
                    var data = Convert.FromBase64String(keystring);
                    // if err != nil {
                    //     return fmt.Errorf("Error decoding given PGP key: %s", err)
                    // }
                    // _, err = openpgp.ReadEntity(packet.NewReader(bytes.NewBuffer(data)))
                    // if err != nil {
                    //     return fmt.Errorf("Error parsing given PGP key: %s", err)
                    // }
                }
            }
        }

        public SealConfig Clone()
        {
            var ret = new SealConfig
            {
                Type            = Type,
                SecretShares    = SecretShares,
                SecretThreshold = SecretThreshold,
                Nonce           = Nonce,
                Backup          = Backup,
                StoredShares    = StoredShares,
            };
            
            if (PGPKeys.Length > 0)
            {
                ret.PGPKeys = new string[PGPKeys.Length];
                Array.Copy(PGPKeys, ret.PGPKeys, PGPKeys.Length);
            }
            // if len(s.PGPKeys) > 0 {
            //     ret.PGPKeys = make([]string, len(s.PGPKeys))
            //     copy(ret.PGPKeys, s.PGPKeys)
            // }

            return ret;
        }
    }


    public class SealAccess
    {
        ISeal Seal;

        public void SetSeal(ISeal seal)
        {
            Seal = seal;
        }

        public bool StoredKeysSupported()
        {
            return Seal.StoredKeysSupported();
        }

        public SealConfig BarrierConfig()
        {
            return Seal.BarrierConfig();
        }

        public bool RecoveryKeySupported()
        {
            return Seal.RecoveryKeySupported();
        }

        public SealConfig RecoveryConfig()
        {
            return Seal.RecoveryConfig();
        }
    }
}

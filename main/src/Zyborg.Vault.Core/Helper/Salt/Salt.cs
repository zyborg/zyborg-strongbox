using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Helper.Salt
{
	//~ type HashFunc func([]byte) [] byte
	public delegate byte[] HashFunc(byte[] bytes);

	// Config is used to parameterize the Salt
	//~ type Config struct {
	public class Config
	{
		// Location is the path in the storage backend for the
		// salt. Uses DefaultLocation if not specified.
		//~ Location string
		public string Location
		{ get; set; }

		// HashFunc is the hashing function to use for salting.
		// Defaults to SHA1 if not provided.
		//~ HashFunc HashFunc
		public HashFunc HashFunc
		{ get; set; }

		// HMAC allows specification of a hash function to use for
		// the HMAC helpers
		//~ HMAC func() hash.Hash
		public Func<HMAC> HMAC
		{ get; set; }

		// String prepended to HMAC strings for identification.
		// Required if using HMAC
		//~ HMACType string
		public string HMACType
		{ get; set; }
	}

	// Salt is used to manage a persistent salt key which is used to
	// hash values. This allows keys to be generated and recovered
	// using the global salt. Primarily, this allows paths in the storage
	// backend to be obfuscated if they may contain sensitive information.
	//~ type Salt struct {
	public class Salt
	{
		//~ const (
		//~ 	// DefaultLocation is the path in the view we store our key salt
		//~ 	// if no other path is provided.
		//~ 	DefaultLocation = "salt"
		//~ )
		public const string DefaultLocation = "salt";

		//~ config    *Config
		//~ salt      string
		//~ generated bool
		public Config Config
		{ get; set; }

		public string SaltValue
		{ get; set; }

		public bool Generated
		{ get; set; }

		// NewSalt creates a new salt based on the configuration
		//~ func NewSalt(view logical.Storage, config *Config) (*Salt, error) {
		public static Salt NewSalt(Logical.IStorage view, Config config)
		{
			// Setup the configuration
			//~ if config == nil {
			//~ 	config = &Config{}
			//~ }
			//~ if config.Location == "" {
			//~ 	config.Location = DefaultLocation
			//~ }
			//~ if config.HashFunc == nil {
			//~ 	config.HashFunc = SHA256Hash
			//~ }
			//~ if config.HMAC == nil {
			//~ 	config.HMAC = sha256.New
			//~ 	config.HMACType = "hmac-sha256"
			//~ }
			if (config == null)
				config = new Config();
			if (string.IsNullOrEmpty(config.Location))
				config.Location = DefaultLocation;
			if (config.HashFunc == null)
				config.HashFunc = SHA256Hash;
			if (config.HMAC == null)
			{
				config.HMAC = () => new HMACSHA256();
				config.HMACType = "hmac-sha256";
			}


			// Create the salt
			//~ s := &Salt{
			//~ 	config: config,
			//~ }
			var s = new Salt
			{
				Config = config,
			};

			// Look for the salt
			//~ var raw *logical.StorageEntry
			//~ var err error
			//~ if view != nil {
			//~ 	raw, err = view.Get(config.Location)
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("failed to read salt: %v", err)
			//~ 	}
			//~ }
			Logical.StorageEntry raw = null;
			if (view != null)
				raw = view.Get(config.Location);

			// Restore the salt if it exists
			//~ if raw != nil {
			//~ 	s.salt = string(raw.Value)
			//~ }
			s.SaltValue = raw?.Value?.ToUtf8String();

			// Generate a new salt if necessary
			//~ if s.salt == "" {
			//~ 	s.salt, err = uuid.GenerateUUID()
			//~ 	if err != nil {
			//~ 		return nil, fmt.Errorf("failed to generate uuid: %v", err)
			//~ 	}
			//~ 	s.generated = true
			//~ 	if view != nil {
			//~ 		raw := &logical.StorageEntry{
			//~ 			Key:   config.Location,
			//~ 			Value: []byte(s.salt),
			//~ 		}
			//~ 		if err := view.Put(raw); err != nil {
			//~ 			return nil, fmt.Errorf("failed to persist salt: %v", err)
			//~ 		}
			//~ 	}
			//~ }
			if (string.IsNullOrEmpty(s.SaltValue))
			{
				s.SaltValue = Guid.NewGuid().ToString();
				s.Generated = true;
				if (view != null)
				{
					raw = new Logical.StorageEntry
					{
						Key = config.Location,
						Value = s.SaltValue.ToUtf8Bytes(),
					};
					view.Put(raw);
				}
			}

			//~ if config.HMAC != nil {
			//~ 	if len(config.HMACType) == 0 {
			//~ 		return nil, fmt.Errorf("HMACType must be defined")
			//~ 	}
			//~ }
			if (config.HMAC != null)
			{
				if (string.IsNullOrEmpty(config.HMACType))
					throw new Exception("HMACType must be defined");
			}

			//~ return s, nil
			return s;
		}

		// SaltID is used to apply a salt and hash function to an ID to make sure
		// it is not reversible
		//~ func (s *Salt) SaltID(id string) string {
		public string SaltID(string id)
		{
			//~ 	return SaltID(s.salt, id, s.config.HashFunc)
			return SaltID(SaltValue, id, this.Config.HashFunc);
		}

		// GetHMAC is used to apply a salt and hash function to data to make sure it is
		// not reversible, with an additional HMAC
		//~ func (s *Salt) GetHMAC(data string) string {
		public string GetHMAC(string data)
		{
			//~ hm := hmac.New(s.config.HMAC, []byte(s.salt))
			//~ hm.Write([]byte(data))
			//~ return hex.EncodeToString(hm.Sum(nil))
			using (var hm = Config.HMAC())
			{
				hm.Key = SaltValue.ToUtf8Bytes();
				var hash = hm.ComputeHash(data.ToUtf8Bytes());
				return BitConverter.ToString(hash).Replace("-", "").ToLower();
			}
		}

		// GetIdentifiedHMAC is used to apply a salt and hash function to data to make
		// sure it is not reversible, with an additional HMAC, and ID prepended
		//~ func (s *Salt) GetIdentifiedHMAC(data string) string {
		public string GetIdentifiedHMAC(string data)
		{
			//~ return s.config.HMACType + ":" + s.GetHMAC(data)
			return $"{Config.HMACType}:{GetHMAC(data)}";
		}

		// DidGenerate returns if the underlying salt value was generated
		// on initialization or if an existing salt value was loaded
		//~ func (s *Salt) DidGenerate() bool {
		public bool DidGenerate()
		{
			//~ return s.generated
			return Generated;
		}

		// SaltID is used to apply a salt and hash function to an ID to make sure
		// it is not reversible
		//~ func SaltID(salt, id string, hash HashFunc) string {
		public static string SaltID(string salt, string id, HashFunc hash)
		{
			//~ comb := salt + id
			//~ hashVal := hash([]byte(comb))
			//~ return hex.EncodeToString(hashVal)
			var comb = salt + id;
			var hashVal = hash(comb.ToUtf8Bytes());
			return BitConverter.ToString(hashVal).Replace("-", "");
		}

		//~ func HMACValue(salt, val string, hashFunc func() hash.Hash) string {
		public static string HMACValue(string salt, string val, Func<HMAC> hashFunc)
		{
			//~ hm := hmac.New(hashFunc, []byte(salt))
			//~ hm.Write([]byte(val))
			//~ return hex.EncodeToString(hm.Sum(nil))
			using (var hm = hashFunc())
			{
				hm.Key = salt.ToUtf8Bytes();
				var hash = hm.ComputeHash(val.ToUtf8Bytes());
				return BitConverter.ToString(hash).Replace("-", "");
			}
		}

		//~ func HMACIdentifiedValue(salt, val, hmacType string, hashFunc func() hash.Hash) string {
		public static string HMACIdentifiedValue(string salt, string val, string hmacType, Func<HMAC> hashFunc)
		{
			//~ return hmacType + ":" + HMACValue(salt, val, hashFunc)
			return $"{hmacType}:{HMACValue(salt, val, hashFunc)}";
		}

		// SHA1Hash returns the SHA1 of the input
		//~ func SHA1Hash(inp []byte) []byte {
		public static byte[] SHA1Hash(byte[] inp)
		{
			//~ hashed := sha1.Sum(inp)
			//~ return hashed[:]
			using (var sha = SHA1.Create())
			{
				return sha.ComputeHash(inp);
			}
		}

		// SHA256Hash returns the SHA256 of the input
		//~ func SHA256Hash(inp []byte) []byte {
		public static byte[] SHA256Hash(byte[] inp)
		{
			//~ hashed := sha256.Sum256(inp)
			//~ return hashed[:]
			using (var sha = SHA256.Create())
			{
				return sha.ComputeHash(inp);
			}
		}
	}
}

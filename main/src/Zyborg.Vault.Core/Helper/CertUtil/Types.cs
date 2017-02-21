using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Helper.CertUtil
{
    public class Types
    {

	}

	// Secret is used to attempt to unmarshal a Vault secret
	// JSON response, as a convenience
	//~ type Secret struct {
	//~ 	Data map[string]interface{} `json:"data"`
	//~ }
	public class Secret
	{
		[JsonProperty("data")]
		public Dictionary<string, object> Data
		{ get; set; }
	}

	// PrivateKeyType holds a string representation of the type of private key (ec
	// or rsa) referenced in CertBundle and ParsedCertBundle. This uses colloquial
	// names rather than official names, to eliminate confusion
	//~ type PrivateKeyType string
	//~ 
	//~ //Well-known PrivateKeyTypes
	//~ const (
	//~ 	UnknownPrivateKey PrivateKeyType = ""
	//~ 	RSAPrivateKey     PrivateKeyType = "rsa"
	//~ 	ECPrivateKey      PrivateKeyType = "ec"
	//~ )
	public class PrivateKeyType : LabeledEnum<PrivateKeyType, string>
	{
		public static readonly PrivateKeyType UnknownPrivateKey = From(string.Empty);
		public static readonly PrivateKeyType RSAPrivateKey = From("rsa");
		public static readonly PrivateKeyType ECPrivateKey = From("ec");
	}

	// TLSUsage controls whether the intended usage of a *tls.Config
	// returned from ParsedCertBundle.GetTLSConfig is for server use,
	// client use, or both, which affects which values are set
	//~ type TLSUsage int
	//~ 
	//~ //Well-known TLSUsage types
	//~ const (
	//~ 	TLSUnknown TLSUsage = 0
	//~ 	TLSServer  TLSUsage = 1 << iota
	//~ 	TLSClient
	//~ )

	public class TLSUsage : LabeledEnum<TLSUsage, int>
	{
		public static readonly TLSUsage TLSUnknown = From(0);
		public static readonly TLSUsage TLSServer = From(1);
		public static readonly TLSUsage TLSClient = From(2);
	}


	//BlockType indicates the serialization format of the key
	//~ type BlockType string
	//~ 
	//~ //Well-known formats
	//~ const (
	//~ 	PKCS1Block BlockType = "RSA PRIVATE KEY"
	//~ 	PKCS8Block BlockType = "PRIVATE KEY"
	//~ 	ECBlock    BlockType = "EC PRIVATE KEY"
	//~ )
	public class BlockType : LabeledEnum<BlockType, string>
	{
		public static readonly BlockType PKCS1Block = From( "RSA PRIVATE KEY");
		public static readonly BlockType PKCS8Block = From("PRIVATE KEY");
		public static readonly BlockType ECBlock = From("EC PRIVATE KEY");
	}

	//ParsedPrivateKeyContainer allows common key setting for certs and CSRs
	//~ type ParsedPrivateKeyContainer interface {
	//~ 	SetParsedPrivateKey(crypto.Signer, PrivateKeyType, []byte)
	//~ }
	public interface ParsedPrivateKeyContainer
	{
		void SetParsedPrivateKey(object signer, PrivateKeyType pkt, byte[] key);
	}

	// CertBlock contains the DER-encoded certificate and the PEM
	// block's byte array
	//~ type CertBlock struct {
	//~ 	Certificate *x509.Certificate
	//~ 	Bytes       []byte
	//~ }
	public class CertBlock
	{
		public X509Certificate2 Certificate
		{ get; set; }

		public byte[] Bytes
		{ get; set; }
	}
}

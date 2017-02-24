using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault
{
	// InitParams keeps the init function from being littered with too many
	// params, that's it!
	//~ type InitParams struct {
	public class InitParams
	{
		//~ BarrierConfig   *SealConfig
		//~ RecoveryConfig  *SealConfig
		//~ RootTokenPGPKey string
		public SealConfig BarrierConfig
		{ get; set; }
		public SealConfig RecoveryConfig
		{ get; set; }
		public string RootTokenPGPKey
		{ get; set; }
	}

	// InitResult is used to provide the key parts back after
	// they are generated as part of the initialization.
	//~ type InitResult struct {
	public class InitResult
	{
		//~ SecretShares   [][]byte
		//~ RecoveryShares [][]byte
		//~ RootToken      string
		public byte[][] SecretShares
		{ get; set; }
		public byte[][] RecoveryShares
		{ get; set; }
		public string RootToken
		{ get; set; }
	}
}

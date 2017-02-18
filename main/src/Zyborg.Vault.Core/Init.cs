namespace Zyborg.Vault
{
    // InitParams keeps the init function from being littered with too many
    // params, that's it!
    public struct InitParams
    {
        public SealConfig BarrierConfig;
        public SealConfig RecoveryConfig;

        public string RootTokenPGPKey;
    }

    // InitResult is used to provide the key parts back after
    // they are generated as part of the initialization.
    public struct InitResult
    {
        public byte[][] SecretShares;
        public byte[][] RecoveryShares;
        public string RootToken;
    }
}
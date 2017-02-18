namespace Zyborg.Vault.Routing
{
    public class RoutePaths
    {
        // barrierInitPath is the path used to store our init sentinel file
        public const string barrierInitPath = "barrier/init";

        // keyringPath is the location of the keyring data. This is encrypted
        // by the master key.
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
        public const string keyringUpgradePrefix = "core/upgrade/";

        // masterKeyPath is the location of the master key. This is encrypted
        // by the latest key in the keyring. This is only used by standby instances
        // to handle the case of a rekey. If the active instance does a rekey,
        // the standby instances can no longer reload the keyring since they
        // have the old master key. This key can be decrypted if you have the
        // keyring to discover the new master key. The new master key is then
        // used to reload the keyring itself.
        public const string masterKeyPath = "core/master";
    }
}
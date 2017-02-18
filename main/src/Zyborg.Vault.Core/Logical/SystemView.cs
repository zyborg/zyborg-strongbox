using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{

	// SystemView exposes system configuration information in a safe way
	// for logical backends to consume
	//~ type SystemView interface {
	public interface ISystemView
	{
		// DefaultLeaseTTL returns the default lease TTL set in Vault configuration
		//~ DefaultLeaseTTL() time.Duration
		TimeSpan DefaultLeaseTTL
		{ get; }

		// MaxLeaseTTL returns the max lease TTL set in Vault configuration; backend
		// authors should take care not to issue credentials that last longer than
		// this value, as Vault will revoke them
		//~ MaxLeaseTTL() time.Duration
		TimeSpan MaxLeaseTTL
		{ get; }

		// SudoPrivilege returns true if given path has sudo privileges
		// for the given client token
		//~ SudoPrivilege(path string, token string) bool
		bool SudoPrivilege(string path, string token);

		// Returns true if the mount is tainted. A mount is tainted if it is in the
		// process of being unmounted. This should only be used in special
		// circumstances; a primary use-case is as a guard in revocation functions.
		// If revocation of a backend's leases fails it can keep the unmounting
		// process from being successful. If the reason for this failure is not
		// relevant when the mount is tainted (for instance, saving a CRL to disk
		// when the stored CRL will be removed during the unmounting process
		// anyways), we can ignore the errors to allow unmounting to complete.
		//~ Tainted() bool
		bool IsTainted
		{ get; }

		// Returns true if caching is disabled. If true, no caches should be used,
		// despite known slowdowns.
		//~ CachingDisabled() bool
		bool IsCachingDisabled
		{ get; }

		// ReplicationState indicates the state of cluster replication
		//~ ReplicationState() ReplicationState
		ReplicationState ReplicationState
		{ get; }
	}

	//~ type StaticSystemView struct {
	public class StaticSystemView : ISystemView
	{
		//~ DefaultLeaseTTLVal  time.Duration
		//~ MaxLeaseTTLVal      time.Duration
		//~ SudoPrivilegeVal    bool
		//~ TaintedVal          bool
		//~ CachingDisabledVal  bool
		//~ Primary             bool
		//~ ReplicationStateVal ReplicationState

		//~ func(d StaticSystemView) DefaultLeaseTTL() time.Duration {
		//~ 	return d.DefaultLeaseTTLVal
		//~ }
		//~ 
		//~ func(d StaticSystemView) MaxLeaseTTL() time.Duration {
		//~ 	return d.MaxLeaseTTLVal
		//~ }
		//~ 
		//~ func(d StaticSystemView) SudoPrivilege(path string, token string) bool {
		//~ 	return d.SudoPrivilegeVal
		//~ }
		//~ 
		//~ func(d StaticSystemView) Tainted() bool {
		//~ 	return d.TaintedVal
		//~ }
		//~ 
		//~ func(d StaticSystemView) CachingDisabled() bool {
		//~ 	return d.CachingDisabledVal
		//~ }
		//~ 
		//~ func(d StaticSystemView) ReplicationState() ReplicationState {
		//~ 	return d.ReplicationStateVal
		//~ }

		private bool _sudoPrivilege = false;

		public TimeSpan DefaultLeaseTTL
		{ get; set; }

		public TimeSpan MaxLeaseTTL
		{ get; set; }

		public bool SudoPrivilege(string path, string token)
		{
			return _sudoPrivilege;
		}

		public bool IsTainted
		{ get; set; }

		public bool IsCachingDisabled
		{ get; set; }

		public ReplicationState ReplicationState
		{ get; set; }
	}
}

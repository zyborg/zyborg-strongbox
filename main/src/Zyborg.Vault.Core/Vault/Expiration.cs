using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault
{
	internal static partial class Constants
	{
		//~ const (
		// expirationSubPath is the sub-path used for the expiration manager
		// view. This is nested under the system view.
		//~ expirationSubPath = "expire/"
		internal const string expirationSubPath = "expire/";

		// leaseViewPrefix is the prefix used for the ID based lookup of leases.
		//~ leaseViewPrefix = "id/"
		internal const string leaseViewPrefix = "id/";

		// tokenViewPrefix is the prefix used for the token based lookup of leases.
		//~ tokenViewPrefix = "token/"
		internal const string tokenViewPrefix = "token/";

		// maxRevokeAttempts limits how many revoke attempts are made
		//~ maxRevokeAttempts = 6
		internal const int maxRevokeAttempts = 6;

		// revokeRetryBase is a baseline retry time
		//~ revokeRetryBase = 10 * time.Second
		internal static readonly TimeSpan revokeRetryBase = TimeSpan.FromSeconds(10);

		// minRevokeDelay is used to prevent an instant revoke on restore
		//~ minRevokeDelay = 5 * time.Second
		internal static readonly TimeSpan minRevokeDelay = TimeSpan.FromSeconds(5);

		// maxLeaseDuration is the default maximum lease duration
		//~ maxLeaseTTL = 32 * 24 * time.Hour
		internal static readonly TimeSpan maxLeaseTTL = TimeSpan.FromHours(32 * 24);

		// defaultLeaseDuration is the default lease duration used when no lease is specified
		//~ defaultLeaseTTL = maxLeaseTTL
		internal static readonly TimeSpan defaultLeaseTTL = maxLeaseTTL;
	}
}

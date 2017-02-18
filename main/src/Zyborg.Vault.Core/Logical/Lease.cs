using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	// LeaseOptions is an embeddable struct to capture common lease
	// settings between a Secret and Auth
	//~ type LeaseOptions struct {
	public class LeaseOptions
	{
		// Lease is the duration that this secret is valid for. Vault
		// will automatically revoke it after the duration.
		//~ TTL time.Duration `json:"lease"`
		[JsonProperty("lease")]
		public TimeSpan TTL
		{ get; set; }

		// Renewable, if true, means that this secret can be renewed.
		//~ Renewable bool `json:"renewable"`
		[JsonProperty("renewable")]
		public bool Renewable
		{ get; set; }

		// Increment will be the lease increment that the user requested.
		// This is only available on a Renew operation and has no effect
		// when returning a response.
		//~ Increment time.Duration `json:"-"`
		[JsonIgnore]
		public TimeSpan Increment
		{ get; set; }

		// IssueTime is the time of issue for the original lease. This is
		// only available on a Renew operation and has no effect when returning
		// a response. It can be used to enforce maximum lease periods by
		// a logical backend.
		//~ IssueTime time.Time `json:"-"`
		[JsonIgnore]
		public DateTime IssueTime  // TODO: should we include NodaTime???
		{ get; set; }


		// LeaseEnabled checks if leasing is enabled
		//~ func (l *LeaseOptions) LeaseEnabled() bool {
		public bool LeaseEnabled()
		{
			//~ return l.TTL > 0
			return TTL > TimeSpan.Zero;
		}

		// LeaseTotal is the lease duration with a guard against a negative TTL
		//~ func (l *LeaseOptions) LeaseTotal() time.Duration {
		public TimeSpan LeaseTotal()
		{
			//~ if l.TTL <= 0 {
			//~ 	return 0
			//~ }
			//~ 
			//~ return l.TTL
			return TTL <= TimeSpan.Zero
				? TimeSpan.Zero
				: TTL;
		}

		// ExpirationTime computes the time until expiration including the grace period
		//~ func (l *LeaseOptions) ExpirationTime() time.Time {
		public DateTime ExpirationTime()
		{
			//~ var expireTime time.Time
			//~ if l.LeaseEnabled() {
			//~ 	expireTime = time.Now().Add(l.LeaseTotal())
			//~ }
			//~ return expireTime

			// IMPL: The "zero value" of GO time.Time is equivalent to DateTime.MinValue
			var expireTime = DateTime.MinValue;
			if (LeaseEnabled())
			{
				expireTime = DateTime.Now.Add(LeaseTotal());
			}
			return expireTime;
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	[TestClass]
    public class LeaseTests
    {
		[TestMethod]
		//~ func TestLeaseOptionsLeaseTotal(t *testing.T) {
		public void TestLeaseOptionsLeaseTotal()
		{
			//~ var l LeaseOptions
			//~ l.TTL = 1 * time.Hour
			//~ 
			//~ actual := l.LeaseTotal()
			//~ expected := l.TTL
			//~ if actual != expected {
			//~ 	t.Fatalf("bad: %s", actual)
			//~ }
			var l = new LeaseOptions { TTL = TimeSpan.FromHours(1), };
			var actual = l.LeaseTotal();
			var expected = l.TTL;
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		//~ func TestLeaseOptionsLeaseTotal_grace(t *testing.T) {
		public void TestLeaseOptionsLeaseTotal_grace()
		{
			//~ var l LeaseOptions
			//~ l.TTL = 1 * time.Hour
			//~ 
			//~ actual := l.LeaseTotal()
			//~ if actual != l.TTL {
			//~ 	t.Fatalf("bad: %s", actual)
			//~ }
			var l = new LeaseOptions { TTL = TimeSpan.FromHours(1), };
			var actual = l.LeaseTotal();
			Assert.AreEqual(l.TTL, actual);
		}

		[TestMethod]
		//~ func TestLeaseOptionsLeaseTotal_negLease(t *testing.T) {
		public void TestLeaseOptionsLeaseTotal_negLease()
		{
			//~ var l LeaseOptions
			//~ l.TTL = -1 * 1 * time.Hour
			//~ 
			//~ actual := l.LeaseTotal()
			//~ expected := time.Duration(0)
			//~ if actual != expected {
			//~ 	t.Fatalf("bad: %s", actual)
			//~ }
			var l = new LeaseOptions { TTL = TimeSpan.FromHours(-1), };
			var actual = l.LeaseTotal();
			var expected = TimeSpan.Zero;
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		//~ func TestLeaseOptionsExpirationTime(t *testing.T) {
		public void TestLeaseOptionsExpirationTime()
		{
			//~ var l LeaseOptions
			//~ l.TTL = 1 * time.Hour
			//~ 
			//~ limit := time.Now().Add(time.Hour)
			//~ exp := l.ExpirationTime()
			//~ if exp.Before(limit) {
			//~ 	t.Fatalf("bad: %s", exp)
			//~ }
			var l = new LeaseOptions { TTL = TimeSpan.FromHours(1), };
			var limit = DateTime.Now.AddHours(1);
			var exp = l.ExpirationTime();
			Assert.IsTrue(exp >= limit);
		}

		[TestMethod]
		//~ func TestLeaseOptionsExpirationTime_noLease(t *testing.T) {
		public void TestLeaseOPtionsExpirationTime_noLease()
		{
			//~ var l LeaseOptions
			//~ if !l.ExpirationTime().IsZero() {
			//~ 	t.Fatal("should be zero")
			//~ }
			var l = new LeaseOptions();
			Assert.AreEqual(DateTime.MinValue, l.ExpirationTime());
		}
	}
}

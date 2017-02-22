using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyborg.Vault.Helper.Salt;

namespace Zyborg.Vault.Audit
{
	[TestClass]
	public class FormatTests
	{
		//~ type noopFormatWriter struct {
		//~ }
		class noopFormatWriter : IAuditFormatWriter
		{
			//~ func (n *noopFormatWriter) WriteRequest(_ io.Writer, _ *AuditRequestEntry) error {
			//~ 	return nil
			//~ }
			public void WriteRequest(Stream s, AuditRequestEntry entry)
			{ }

			//~ func(n* noopFormatWriter) WriteResponse(_ io.Writer, _* AuditResponseEntry) error {
			//~ 	return nil
			//~ }
			public void WriteResponse(Stream s, AuditResponseEntry entry)
			{ }
		}

		[TestMethod]
		//~ func TestFormatRequestErrors(t *testing.T) {
		public void TestFormatRequestErrors()
		{
			//~ salter, _ := salt.NewSalt(nil, nil)
			//~ config := FormatterConfig{
			//~ 	Salt: salter,
			//~ }
			//~ formatter := AuditFormatter{
			//~ 	AuditFormatWriter: &noopFormatWriter{},
			//~ }
			var salter = Salt.NewSalt(null, null);
			var config = new FormatterConfig
			{
				Salt = salter,
			};
			var formatter = new AuditFormatter(new noopFormatWriter());

			//~ if err := formatter.FormatRequest(ioutil.Discard, config, nil, nil, nil); err == nil {
			//~ 	t.Fatal("expected error due to nil request")
			//~ }
			//~ if err := formatter.FormatRequest(nil, config, nil, &logical.Request{}, nil); err == nil {
			//~ 	t.Fatal("expected error due to nil writer")
			//~ }
			Assert.ThrowsException<ArgumentNullException>(() =>
				formatter.FormatRequest(Stream.Null, config, null, null, null));
			Assert.ThrowsException<ArgumentNullException>(() =>
				formatter.FormatRequest(null, config, null, new Logical.Request(), null));
		}

		[TestMethod]
		//~ func TestFormatResponseErrors(t *testing.T) {
		public void TestFormatResponseErrors()
		{
			//~ salter, _ := salt.NewSalt(nil, nil)
			//~ config := FormatterConfig{
			//~ 	Salt: salter,
			//~ }
			//~ formatter := AuditFormatter{
			//~ 	AuditFormatWriter: &noopFormatWriter{},
			//~ }
			var salter = Salt.NewSalt(null, null);
			var config = new FormatterConfig
			{
				Salt = salter,
			};
			var formatter = new AuditFormatter(new noopFormatWriter());

			//~ if err := formatter.FormatResponse(ioutil.Discard, config, nil, nil, nil, nil); err == nil {
			//~ 	t.Fatal("expected error due to nil request")
			//~ }
			//~ if err := formatter.FormatResponse(nil, config, nil, &logical.Request{}, nil, nil); err == nil {
			//~ 	t.Fatal("expected error due to nil writer")
			//~ }
			Assert.ThrowsException<ArgumentNullException>(() =>
				formatter.FormatResponse(Stream.Null, config, null, null, null, null));
			Assert.ThrowsException<ArgumentNullException>(() =>
				formatter.FormatResponse(null, config, null, new Logical.Request(), null, null));
		}
	}
}

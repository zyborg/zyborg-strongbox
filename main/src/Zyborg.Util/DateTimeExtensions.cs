using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Zyborg.Util
{
    public static class DateTimeExtensions
    {
		public static string FormatUtcAsRFC3339(this DateTime utc, bool assertUtc = true)
		{
			if (assertUtc && utc.Kind != DateTimeKind.Utc)
				throw new ArgumentException("kind is not UTC", nameof(utc));

			// This solution was found on StackOverflow:
			//return XmlConvert.ToString(utc, XmlDateTimeSerializationMode.Utc);

			// As per https://golang.org/pkg/time/#pkg-constants
			//	RFC3339     = "2006-01-02T15:04:05Z07:00"
			//	RFC3339Nano = "2006-01-02T15:04:05.999999999Z07:00"
			return utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'zzz");
		}

		public static string FormatUtcAsRFC3339Nano(this DateTime utc, bool assertUtc = true)
		{
			if (assertUtc && utc.Kind != DateTimeKind.Utc)
				throw new ArgumentException("kind is not UTC", nameof(utc));

			// As per https://golang.org/pkg/time/#pkg-constants
			//	RFC3339     = "2006-01-02T15:04:05Z07:00"
			//	RFC3339Nano = "2006-01-02T15:04:05.999999999Z07:00"
			return utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffff'Z'zzz");
		}
	}
}

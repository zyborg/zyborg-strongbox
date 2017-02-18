using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zyborg.Util.JSON
{
    public static class JsonHelper
    {
		public static string SerializeObject(object value, string prefix, string indent)
		{
			var json = JsonConvert.SerializeObject(value, Formatting.Indented);
			var lines = json.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			var newLines = new List<string>();

			foreach (var l in lines)
			{
				if (newLines.Count == 0)
				{
					newLines.Add(l);
					continue;
				}

				var oldL = l;
				var newL = prefix + indent;
				while (oldL.StartsWith("  "))
				{
					newL += indent;
					oldL = oldL.Substring(2);
				}
				newL += oldL;
				newLines.Add(newL);
			}
			return string.Join("\r\n", newLines);
		}

		// Based on default impl of JsonConvert.SerializeObjectInternal(...)
		//    https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/JsonConvert.cs#L647
		//
		public static string SerializeObjectBroken(object value, string prefix, string indent)
		{
			StringBuilder sb = new StringBuilder(256);
			StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

			var formatting = Formatting.Indented;
			var jsonSerializer = JsonSerializer.CreateDefault();
			jsonSerializer.Formatting = formatting;

			using (var jsonWriter = new PrefixIndentTextWriter(sw, prefix, indent))
			{
				jsonWriter.Formatting = formatting;
				jsonSerializer.Serialize(jsonWriter, value);
			}

			return sw.ToString();
		}

		public class PrefixIndentTextWriter : JsonTextWriter
		{
			string _prefix;
			string _indent;

			TextWriter _textWriter;

			public PrefixIndentTextWriter(TextWriter textWriter, string prefix, string indent)
				: base(textWriter)
			{
				_textWriter = textWriter;
				_prefix = prefix;
				_indent = indent;
			}

			protected override void WriteIndent()
			{
				_textWriter.Write(_prefix);
				for (int i = 0; i < Top; i++)
					_textWriter.Write(_indent);
			}

			public override void WriteValue(object value)
			{
				base.WriteValue(value);
			}
		}
	}
}

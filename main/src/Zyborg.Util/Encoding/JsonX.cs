using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zyborg.Util.Encoding
{
	// namedContainer wraps a gabs.Container to carry name information with it
	//~ type namedContainer struct {
	//~ 	name string
	//~ 	*gabs.Container
	//~ }
	public class NamedContainer : Container
	{
		//~ const (
		//~ 	XMLHeader = `<?xml version="1.0" encoding="UTF-8"?>`
		//~ 	Header    = `<json:object xsi:schemaLocation="http://www.datapower.com/schemas/json jsonx.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:json="http://www.ibm.com/xmlns/prod/2009/jsonx">`
		//~ 	Footer    = `</json:object>`
		//~ )
		public const string XMLHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
		public const string Header = @"<json:object xsi:schemaLocation=""http://www.datapower.com/schemas/json jsonx.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:json=""http://www.ibm.com/xmlns/prod/2009/jsonx"">";
		public const string Footer = @"</json:object>";

		public NamedContainer() : base()
		{ }

		public NamedContainer(object obj) : base(obj)
		{ }

		public NamedContainer(string name, object obj) : base(obj)
		{
			Name = name;
		}

		public NamedContainer(Container cont) : base(cont.Object)
		{
			this.Name = (cont as NamedContainer)?.Name;
		}

		public NamedContainer(string name, Container cont) : base(cont.Object)
		{
			this.Name = name;
		}

		public string Name
		{ get; set; }


		// Marshal marshals the input data into JSONx.
		//~ func Marshal(input interface{}) (string, error) {
		public static string Marshal(object input)
		{
			//~ jsonBytes, err := json.Marshal(input)
			//~ if err != nil {
			//~ 	return "", err
			//~ }
			var jsonBytes = JsonConvert.SerializeObject(input);
			//~ xmlBytes, err := EncodeJSONBytes(jsonBytes)
			//~ if err != nil {
			//~ 	return "", err
			//~ }
			//~ return fmt.Sprintf("%s%s%s%s", XMLHeader, Header, string(xmlBytes), Footer), nil
			var xmlBytes = EncodeJSONBytes(jsonBytes.ToUtf8Bytes());
			return $"{XMLHeader}{Header}{xmlBytes.ToUtf8String()}{Footer}";
		}

		// EncodeJSONBytes encodes JSON-formatted bytes into JSONx. It is designed to
		// be used for multiple entries so does not prepend the JSONx header tag or
		// append the JSONx footer tag. You can use jsonx.Header and jsonx.Footer to
		// easily add these when necessary.
		//~ func EncodeJSONBytes(input []byte) ([]byte, error) {
		public static byte[] EncodeJSONBytes(byte[] input)
		{
			//~ o := bytes.NewBuffer(nil)
			//~ reader := bytes.NewReader(input)
			//~ dec := json.NewDecoder(reader)
			//~ dec.UseNumber()
			var o = new StringBuilder();

			//~ cont, err := gabs.ParseJSONDecoder(dec)
			//~ if err != nil {
			//~ 	return nil, err
			//~ }
			var cont = Container.ParseJSON(input);

			//~ if err := sortAndTransformObject(o, &namedContainer{Container: cont}); err != nil {
			//~ 	return nil, err
			//~ }
			//~ 
			//~ return o.Bytes(), nil
			SortAndTransformObject(o, new NamedContainer(cont.Object));
			return o.ToString().ToUtf8Bytes();
		}

		// sortAndTransformObject sorts object keys to make the output predictable so
		// the package can be tested; logic is here to prevent code duplication
		//~ func sortAndTransformObject(o *bytes.Buffer, cont *namedContainer) error {
		private static void SortAndTransformObject(StringBuilder o, NamedContainer cont)
		{
			//~ objectChildren, err := cont.ChildrenMap()
			//~ if err != nil {
			//~ 	return err
			//~ }
			var objectChildren = cont.ChildrenMap();

			//~ sortedNames := make([]string, 0, len(objectChildren))
			//~ for name, _ := range objectChildren {
			//~ 	sortedNames = append(sortedNames, name)
			//~ }
			//~ sort.Strings(sortedNames)
			//~ for _, name := range sortedNames {
			//~ 	if err := transformContainer(o, &namedContainer{name: name, Container: objectChildren[name]}); err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			var sortedNames = new List<string>();
			foreach (var name in objectChildren.Keys)
				sortedNames.Add(name);

			sortedNames.Sort();
			foreach (var name in sortedNames)
				TransformContainer(o, new NamedContainer(name, objectChildren[name]));

			//~ return nil
		}

		//~ func transformContainer(o *bytes.Buffer, cont *namedContainer) error {
		private static void TransformContainer(StringBuilder o, NamedContainer cont)
		{
			//~var printName string
			string printName = string.Empty;

			//~ if cont.name != "" {
			//~ 	escapedNameBuf := bytes.NewBuffer(nil)
			//~ 	err := xml.EscapeText(escapedNameBuf, []byte(cont.name))
			//~ 	if err != nil {
			//~ 		return err
			//~ 	}
			//~ 	printName = fmt.Sprintf(" name=\"%s\"", escapedNameBuf.String())
			//~ }
			if (!string.IsNullOrEmpty(cont.Name))
			{
				var escapedNameBuf = XmlEscapeText(cont.Name);
				printName = $" name=\"{escapedNameBuf}\"";
			}

			//~ data := cont.Data()
			//~ switch data.(type) {
			var data = cont.Object;
			switch (data)
			{
				//~ case nil:
				//~ 	o.WriteString(fmt.Sprintf("<json:null%s />", printName))
				case null:
					o.Append($"<json:null{printName} />");
					break;

				//~ case bool:
				//~ 	o.WriteString(fmt.Sprintf("<json:boolean%s>%t</json:boolean>", printName, data))
				case bool x:
					o.Append($"<json:boolean{printName}>{data}</json:boolean>");
					break;

				//~ case json.Number:
				//~ 	o.WriteString(fmt.Sprintf("<json:number%s>%v</json:number>", printName, data))
				case object x when x is Int16 || x is Int32 || x is Int64
						|| x is UInt16 || x is UInt32 || x is UInt64
						|| x is System.Single || x is System.Double || x is System.Decimal:
					o.Append($"<json:number{printName}>{data}</json:number>");
					break;

				//~ case string:
				//~ 	o.WriteString(fmt.Sprintf("<json:string%s>%v</json:string>", printName, data))
				case string x:
					o.Append($"<json:string{printName}>{data}</json:string>");
					break;

				//~ case []interface{}:
				//~ 	o.WriteString(fmt.Sprintf("<json:array%s>", printName))
				//~ 	arrayChildren, err := cont.Children()
				//~ 	if err != nil {
				//~ 		return err
				//~ 	}
				//~ 	for _, child := range arrayChildren {
				//~ 		if err := transformContainer(o, &namedContainer{Container: child}); err != nil {
				//~ 			return err
				//~ 		}
				//~ 	}
				//~ 	o.WriteString("</json:array>")
				case IList<object> x:
					o.Append($"<json:array{printName}>");
					foreach (var child in cont.Children())
						TransformContainer(o, new NamedContainer(child));
					o.Append("</json:array>");
					break;

				//~ case map[string]interface{}:
				//~ 	o.WriteString(fmt.Sprintf("<json:object%s>", printName))
				//~ 
				//~ 	if err := sortAndTransformObject(o, cont); err != nil {
				//~ 		return err
				//~ 	}
				//~ 
				//~ 	o.WriteString("</json:object>")
				case IDictionary<string, object> x:
					o.Append($"<json:object{printName}>");
					SortAndTransformObject(o, cont);
					o.Append("</json:object>");
					break;

				default:
					throw new NotSupportedException($"unexpected data type [{data.GetType().FullName}]");
			}

			//~ return nil
		}

		//~ var (
		//~ 	esc_quot = []byte("&#34;") // shorter than "&quot;"
		//~ 	esc_apos = []byte("&#39;") // shorter than "&apos;"
		//~ 	esc_amp  = []byte("&amp;")
		//~ 	esc_lt   = []byte("&lt;")
		//~ 	esc_gt   = []byte("&gt;")
		//~ 	esc_tab  = []byte("&#x9;")
		//~ 	esc_nl   = []byte("&#xA;")
		//~ 	esc_cr   = []byte("&#xD;")
		//~ 	esc_fffd = []byte("\uFFFD") // Unicode replacement character
		//~ )
		private const string esc_quot = ("&#34;"); // shorter than "&quot;"
		private const string esc_apos = ("&#39;");// shorter than "&apos;"
		private const string esc_amp = ("&amp;");
		private const string esc_lt = ("&lt;");
		private const string esc_gt = ("&gt;");
		private const string esc_tab = ("&#x9;");
		private const string esc_nl = ("&#xA;");
		private const string esc_cr = ("&#xD;");
		private const string esc_fffd = ("\uFFFD"); // Unicode replacement character

		// escapeText writes to w the properly escaped XML equivalent
		// of the plain text data s. If escapeNewline is true, newline
		// characters will be escaped.
		//~ func escapeText(w io.Writer, s []byte, escapeNewline bool) error {
		private static string XmlEscapeText(string s, bool escapeNewline = true)
		{
			//~ var esc []byte
			//~ last := 0
			string esc = string.Empty;
			var last = 0;
			var w = new StringBuilder();

			//~ for i := 0; i < len(s); {
			for (int i = 0; i < s.Length; )
			{
				//~ r, width := utf8.DecodeRune(s[i:])
				//~ i += width
				var r = s[i];
				i++;

				//~ switch r {
				//~ case '"':
				//~ 	esc = esc_quot
				//~ case '\'':
				//~ 	esc = esc_apos
				//~ case '&':
				//~ 	esc = esc_amp
				//~ case '<':
				//~ 	esc = esc_lt
				//~ case '>':
				//~ 	esc = esc_gt
				//~ case '\t':
				//~ 	esc = esc_tab
				//~ case '\n':
				//~ 	if !escapeNewline {
				//~ 		continue
				//~ 	}
				//~ 	esc = esc_nl
				//~ case '\r':
				//~ 	esc = esc_cr
				//~ default:
				//~ 	if !isInCharacterRange(r) || (r == 0xFFFD && width == 1) {
				//~ 		esc = esc_fffd
				//~ 		break
				//~ 	}
				//~ 	continue
				//~ }
				switch (r)
				{
					case '"':
						esc = esc_quot;
						break;
					case '\'':
							esc = esc_apos;
							break;
					case '&':
							esc = esc_amp;
							break;
					case '<':
							esc = esc_lt;
							break;
					case '>':
							esc = esc_gt;
							break;
					case '\t':
							esc = esc_tab;
							break;
					case '\n':
						if (!escapeNewline) {
							continue;
						}
						esc = esc_nl;
						break;
					case '\r':
						esc = esc_cr;
						break;
					default:
						//if (!isInCharacterRange(r) || (r == 0xFFFD && width == 1) {
						//	esc = esc_fffd;
						//	break;
						//}
						continue;
				}

				//~ if _, err := w.Write(s[last : i-width]); err != nil {
				//~ 	return err
				//~ }
				w.Append(s.Substring(last, i - 1));
				//~ if _, err := w.Write(esc); err != nil {
				//~ 	return err
				//~ }
				w.Append(esc);
				//~ last = i
				last = i;
			}
			//~ if _, err := w.Write(s[last:]); err != nil {
			//~ 	return err
			//~ }
			//~ return nil
			w.Append(s.Substring(last));

			return w.ToString();
		}
	}
}

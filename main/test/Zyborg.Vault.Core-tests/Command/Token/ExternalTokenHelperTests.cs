using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Zyborg.Util;
using Zyborg.Util.Collections;

namespace Zyborg.Vault.Command.Token
{
	[TestClass]
    public class ExternalTokenHelperTests
    {
		[TestMethod]
		//~ func TestExternalTokenHelperPath(t *testing.T) {
		public void TestExternalTokenHelperPath()
		{
			//~ cases := map[string]string{}
			var cases = new OrderedDictionary<string, string>();

			//~ unixCases := map[string]string{
			//~ 	"/foo": "/foo",
			//~ }
			var unixCases = new OrderedDictionary<string, string>
			{
				["/foo"] = "/foo",
			};
			//~ windowsCases := map[string]string{
			//~ 	"C:/foo":           "C:/foo",
			//~ 	`C:\Program Files`: `C:\Program Files`,
			//~ }
			var windowsCases = new OrderedDictionary<string, string>
			{
				["C:/foo"] = "C:/foo",
				[@"C:\Program Files"] = @"C:\Program Files",
			};

			//~ var runtimeCases map[string]string
			//~ if runtime.GOOS == "windows" {
			//~ 	runtimeCases = windowsCases
			//~ } else {
			//~ 	runtimeCases = unixCases
			//~ }
			var runtimeCases = unixCases;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				runtimeCases = windowsCases;

			//~ for k, v := range runtimeCases {
			//~ 	cases[k] = v
			//~ }
			foreach (var kv in runtimeCases)
				cases[kv.Key] = kv.Value;

			// We don't expect those to actually exist, so we expect an error. For now,
			// I'm commenting out the rest of this code as we don't have real external
			// helpers to test with and the os.Stat will fail with our fake test cases.
			/*
				for k, v := range cases {
					actual, err := ExternalTokenHelperPath(k)
						if err != nil {
							t.Fatalf("error getting external helper path: %v", err)
						}
						if actual != v {
							t.Fatalf(
								"input: %s, expected: %s, got: %s",
								k, v, actual)
						}
				}
			*/
		}

		[TestMethod]
		//~ func TestExternalTokenHelper(t *testing.T) {
		public void TestExternalTokenHelper()
		{
			//~ Test(t, testExternalTokenHelper(t))
			Testing.Test(testExternalTokenHelper());
		}

		//~ func testExternalTokenHelper(t *testing.T) *ExternalTokenHelper {
		private ExternalTokenHelper testExternalTokenHelper()
		{
			//~ return &ExternalTokenHelper{BinaryPath: helperPath("helper"), Env: helperEnv()}
			return new ExternalTokenHelper
			{
				BinaryPath = helperPath("helper"),
				Env = helperEnv(),
			};
		}

		//~ func helperPath(s ...string) string {
		private string helperPath(params string[] s)
		{
			//~ cs := []string{"-test.run=TestExternalTokenHelperProcess", "--"}
			//~ cs = append(cs, s...)
			//~ return fmt.Sprintf(
			//~ 	"%s %s",
			//~ 	os.Args[0],
			//~ 	strings.Join(cs, " "))
			var cs = new List<string>
			{
				"-test.run=TestExternalTokenHelperProcess", "--"
			};
			cs.InsertRange(0, Environment.GetCommandLineArgs());
			cs.AddRange(s);
			return $"\"{ProcessExtensions.GetArv0()}\" {string.Join(" ", cs)}";
		}

		//~ func helperEnv() []string {
		private string[] helperEnv()
		{
			//~ var env []string


			//~ tf, err := ioutil.TempFile("", "vault")
			//~ if err != nil {
			//~ 	panic(err)
			//~ }
			//~ tf.Close()
			var tf = Zyborg.Util.IO.File.CreateTempFile(prefix: "vault");

			//~ env = append(env, "GO_HELPER_PATH="+tf.Name(), "GO_WANT_HELPER_PROCESS=1")
			//~ return env
			return new string[] { $"GO_HELPER_PATH={tf}", "GO_WANT_HELPER_PROCESS=1" };
		}

		// This is not a real test. This is just a helper process kicked off by tests.
		//~ func TestExternalTokenHelperProcess(*testing.T) {
		public void TestExternalTokenHelperProcess()
		{
			//~ if os.Getenv("GO_WANT_HELPER_PROCESS") != "1" {
			//~ 	return
			//~ }
			if (Environment.GetEnvironmentVariable("GO_WANT_HELPER_PROCESS") != "1")
				return;
			using (var defer = new Util.Defer())
			{
				//~ defer os.Exit(0)
				defer.Add(() => Environment.Exit(0));

				//~ args:= os.Args
				//~ for len(args) > 0 {
				//~ 	if args[0] == "--" {
				//~ 		args = args[1:]
				//~ 		break
				//~ 	}
				//~ 
				//~ 	args = args[1:]
				//~ }
				var args = Environment.GetCommandLineArgs().ToList();
				while (args.Count > 0)
				{
					if (args[0] == "--")
					{
						args.RemoveAt(0);
						break;
					}
					args.RemoveAt(0);
				}

				//~ if len(args) == 0 {
				//~ 	fmt.Fprintf(os.Stderr, "No command\n")
				//~ 	os.Exit(2)
				//~ }
				if (args.Count == 0)
					Assert.Fail("No command");

				//~ cmd, args:= args[0], args[1:]
				var cmd = args[0];
				args.RemoveAt(0);
				//~ switch cmd {
				switch (cmd)
				{
					//~ case "helper":
					//~ 	path:= os.Getenv("GO_HELPER_PATH")
					case "helper":
						var path = Environment.GetEnvironmentVariable("GO_HELPER_PATH");

						//~ switch args[0] {
						switch (args[0])
						{
							case "erase":
								//~ os.Remove(path)
								File.Delete(path);
								break;

							case "get":
								//~ f, err:= os.Open(path)
								//~ if os.IsNotExist(err) {
								//~ 			return
								//~ }
								//~ if err != nil {
								//~ 	fmt.Fprintf(os.Stderr, "Err: %s\n", err)
								//~ 	os.Exit(1)
								//~ }
								//~ defer f.Close()
								//~ io.Copy(os.Stdout, f)
								if (!File.Exists(path))
									return;
								var f = File.Open(path, FileMode.Open);
								defer.Add(() => f.Dispose());
								f.CopyTo(Console.OpenStandardOutput());
								break;
							case "store":
								//~ f2, err:= os.Create(path)
								//~ if err != nil {
								//~ 			fmt.Fprintf(os.Stderr, "Err: %s\n", err)
								//~ 	os.Exit(1)
								//~ }
								//~ defer f.Close()
								//~ io.Copy(f, os.Stdin)
								f = File.Open(path, FileMode.Create);
								defer.Add(() => f.Dispose());
								Console.OpenStandardInput().CopyTo(f);
								break;
						}
						break;

					default:
						//~ fmt.Fprintf(os.Stderr, "Unknown command: %q\n", cmd)
						//~ os.Exit(2)
						throw new Exception("Unknown command: " + cmd);
				}
			}
		}
	}
}

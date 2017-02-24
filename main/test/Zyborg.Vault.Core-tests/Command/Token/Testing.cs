using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zyborg.Util;
using Zyborg.Util.CLI;

namespace Zyborg.Vault.Command.Token
{
    public class Testing
    {
		// Test is a public function that can be used in other tests to
		// test that a helper is functioning properly.
		//~ func Test(t *testing.T, h TokenHelper) {
		public static void Test(ITokenHelper h)
		{
			//~ if err := h.Store("foo"); err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			h.Store("foo");
			//~ v, err := h.Get()
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			var v = h.Get();
			//~ if v != "foo" {
			//~ 	t.Fatalf("bad: %#v", v)
			//~ }
			Assert.AreEqual("foo", v);
			//~ if err := h.Erase(); err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			h.Erase();
			//~ v, err = h.Get()
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			v = h.Get();
			//~ if v != "" {
			//~ 	t.Fatalf("bad: %#v", v)
			//~ }
			Assert.IsNull(v);
		}

		// TestProcess is used to re-execute this test in order to use it as the
		// helper process. For this to work, the TestExternalTokenHelperProcess function must
		// exist.
		//~ func TestProcess(t *testing.T, s ...string) {
		public static void TestProcess(params string[] s)
		{
			//~ h := &ExternalTokenHelper{BinaryPath: TestProcessPath(t, s...)}
			//~ Test(t, h)
			var h = new ExternalTokenHelper
			{
				BinaryPath = TestProcessPath(s),
			};
			Test(h);
		}

		// TestProcessPath returns the path to the test process.
		//~ func TestProcessPath(t *testing.T, s ...string) string {
		public static string TestProcessPath(params string[] s)
		{
			//~ cs := []string{"-test.run=TestExternalTokenHelperProcess", "--", "GO_WANT_HELPER_PROCESS"}
			//~ cs = append(cs, s...)
			//~ return fmt.Sprintf(
			//~ 	"%s %s",
			//~ 	os.Args[0],
			//~ 	strings.Join(cs, " "))
			var cs = new List<string>
			{
				"-test.run=TestExternalTokenHelperProcess", "--", "GO_WANT_HELPER_PROCESS"
			};
			cs.AddRange(s);
			return $"\"{ProcessExtensions.GetArv0()}\" {string.Join(" ", cs)}";
		}

		// TestExternalTokenHelperProcessCLI can be called to implement TestExternalTokenHelperProcess
		// for TestProcess that just executes a CLI command.
		//~ func TestExternalTokenHelperProcessCLI(t *testing.T, cmd cli.Command) {
		public static void TestExternalTokenHelperProcessCLI(ICommand cmd)
		{
			//~ args := os.Args
			//~ for len(args) > 0 {
			//~ 	if args[0] == "--" {
			//~ 		args = args[1:]
			//~ 		break
			//~ 	}
			//~ 
			//~ 	args = args[1:]
			//~ }
			//~ if len(args) == 0 || args[0] != "GO_WANT_HELPER_PROCESS" {
			//~ 	return
			//~ }
			//~ args = args[1:]
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
			if (args.Count == 0 || args[0] != "GO_WANT_HELPER_PROCESS")
				return;
			args.RemoveAt(0);

			//~ os.Exit(cmd.Run(args))
			Environment.Exit(cmd.Run(args.ToArray()));
		}
	}
}

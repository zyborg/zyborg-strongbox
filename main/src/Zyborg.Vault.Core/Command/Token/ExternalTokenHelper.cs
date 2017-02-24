using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IOPath = System.IO.Path;
using System.Runtime.InteropServices;
using System.Text;
using Zyborg.Util;

namespace Zyborg.Vault.Command.Token
{
	// ExternalTokenHelper is the struct that has all the logic for storing and retrieving
	// tokens from the token helper. The API for the helpers is simple: the
	// BinaryPath is executed within a shell with environment Env. The last argument
	// appended will be the operation, which is:
	//
	//   * "get" - Read the value of the token and write it to stdout.
	//   * "store" - Store the value of the token which is on stdin. Output
	//       nothing.
	//   * "erase" - Erase the contents stored. Output nothing.
	//
	// Any errors can be written on stdout. If the helper exits with a non-zero
	// exit code then the stderr will be made part of the error value.
	//~ type ExternalTokenHelper struct {
	//~ 	BinaryPath string
	//~ 	Env        []string
	//~ }
	public class ExternalTokenHelper : ITokenHelper
    {
		public string BinaryPath
		{ get; set; }

		public string[] Env
		{ get; set; }

		// ExternalTokenHelperPath takes the configured path to a helper and expands it to
		// a full absolute path that can be executed. As of 0.5, the default token
		// helper is internal, to avoid problems running in dev mode (see GH-850 and
		// GH-783), so special assumptions of prepending "vault token-" no longer
		// apply.
		//
		// As an additional result, only absolute paths are now allowed. Looking in the
		// path or a current directory for an arbitrary executable could allow someone
		// to switch the expected binary for one further up the path (or in the current
		// directory), potentially opening up execution of an arbitrary binary.
		//~ func ExternalTokenHelperPath(path string) (string, error) {
		public static string ExternalTokenHelperPath(string path)
		{
			//~ if !filepath.IsAbs(path) {
			//~ 	var err error
			//~ 	path, err = filepath.Abs(path)
			//~ 	if err != nil {
			//~ 		return "", err
			//~ 	}
			//~ }
			if (!IOPath.IsPathRooted(path))
				path = IOPath.GetFullPath(path);

			//~ if _, err := os.Stat(path); err != nil {
			//~ 	return "", fmt.Errorf("unknown error getting the external helper path")
			//~ }
			//~ 
			//~ return path, nil
			if (File.Exists(path))
				new FileInfo(path);
			else if (Directory.Exists(path))
				new DirectoryInfo(path);
			else
				throw new System.IO.FileNotFoundException();

			return path;
		}

		// Erase deletes the contents from the helper.
		//~ func (h *ExternalTokenHelper) Erase() error {
		public void Erase()
		{
			//~ cmd, err := h.cmd("erase")
			//~ if err != nil {
			//~ 	return fmt.Errorf("Error: %s", err)
			//~ }
			//~ if output, err := cmd.CombinedOutput(); err != nil {
			//~ 	return fmt.Errorf(
			//~ 		"Error: %s\n\n%s", err, string(output))
			//~ }
			//~ return nil
			var cmd = Cmd("erase");
			var output = new StringBuilder();
			DataReceivedEventHandler h = (s, ev) => output.AppendLine(ev.Data);
			cmd.OutputDataReceived += h;
			cmd.ErrorDataReceived += h;
			try
			{
				cmd.Start();
				cmd.BeginOutputReadLine();
				cmd.BeginErrorReadLine();
				cmd.WaitForExit();
			}
			catch (Exception ex)
			{
				throw new Exception("Erase Error: " + output.ToString(), ex);
			}
		}

		// Get gets the token value from the helper.
		//~ func (h *ExternalTokenHelper) Get() (string, error) {
		public string Get()
		{
			//~ var buf, stderr bytes.Buffer
			//~ cmd, err := h.cmd("get")
			//~ if err != nil {
			//~ 	return "", fmt.Errorf("Error: %s", err)
			//~ }
			//~ cmd.Stdout = &buf
			//~ cmd.Stderr = &stderr
			//~ if err := cmd.Run(); err != nil {
			//~ 	return "", fmt.Errorf(
			//~ 		"Error: %s\n\n%s", err, stderr.String())
			//~ }
			var cmd = Cmd("get");
			var buf = new StringBuilder();
			var stderr = new StringBuilder();
			cmd.OutputDataReceived += (s, ev) => buf.AppendLine(ev.Data);
			cmd.ErrorDataReceived += (s, ev) => stderr.AppendLine(ev.Data);

			try
			{
				cmd.Start();
				cmd.BeginOutputReadLine();
				cmd.BeginErrorReadLine();
				cmd.WaitForExit();
				return buf.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception("Get Error:  " + stderr.ToString(), ex);
			}
		}

		// Store stores the token value into the helper.
		//~ func (h *ExternalTokenHelper) Store(v string) error {
		public void Store(string v)
		{
			//~ buf := bytes.NewBufferString(v)
			//~ cmd, err := h.cmd("store")
			//~ if err != nil {
			//~ 	return fmt.Errorf("Error: %s", err)
			//~ }
			//~ cmd.Stdin = buf
			//~ if output, err := cmd.CombinedOutput(); err != nil {
			//~ 	return fmt.Errorf(
			//~ 		"Error: %s\n\n%s", err, string(output))
			//~ }
			var cmd = Cmd("store");
			var output = new StringBuilder();
			DataReceivedEventHandler h = (s, ev) => output.AppendLine(ev.Data);
			cmd.OutputDataReceived += h;
			cmd.ErrorDataReceived += h;
			try
			{
				cmd.Start();
				cmd.BeginOutputReadLine();
				cmd.BeginErrorReadLine();
				cmd.StandardInput.Write(v);
				cmd.WaitForExit();
			}
			catch (Exception ex)
			{
				throw new Exception("Store Error: " + output.ToString(), ex);
			}

			//~ return nil
		}

		//~ func (h *ExternalTokenHelper) Path() string {
		public string Path
		{
			//~ return h.BinaryPath
			get => BinaryPath;
		}

		//~ func (h *ExternalTokenHelper) cmd(op string) (*exec.Cmd, error) {
		public Process Cmd(string op)
		{
			//~ script := strings.Replace(h.BinaryPath, "\\", "\\\\", -1) + " " + op
			//~ cmd, err := ExecScript(script)
			//~ if err != nil {
			//~ 	return nil, err
			//~ }
			//~ cmd.Env = h.Env
			//~ return cmd, nil
			var script = BinaryPath.Replace("\\", "\\\\") + " " + op;
			var cmd = ExecScript(script);

			var env = cmd.StartInfo.Environment;
			env.Clear();
			foreach (var e in Env)
			{
				var kv = e.Split(new char[] { '=' }, 2);
				env.Add(kv[0], kv[1]);
			}

			return cmd;
		}

		// ExecScript returns a command to execute a script
		//~ func ExecScript(script string) (*exec.Cmd, error) {
		public static Process ExecScript(string script)
		{
			//~ var shell, flag string
			//~ if runtime.GOOS == "windows" {
			//~ 	shell = "cmd"
			//~ 	flag = "/C"
			//~ } else {
			//~ 	shell = "/bin/sh"
			//~ 	flag = "-c"
			//~ }
			string shell;
			string flag;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				shell = "cmd";
				flag = "/C";
			}
			else
			{
				shell = "/bin/sh";
				flag = "-c";
			}
			//~ if other := os.Getenv("SHELL"); other != "" {
			//~ 	shell = other
			//~ }
			//~ cmd := exec.Command(shell, flag, script)
			//~ return cmd, nil
			var other = Environment.GetEnvironmentVariable("SHELL");
			if (!string.IsNullOrEmpty(other))
				shell = other;

			var cmd = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = shell,
					Arguments = string.Join(" ", flag, script),
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					RedirectStandardInput = true,
				}
			};
			return cmd;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util.CLI
{
	// IMPL:  just the interface from
	//    https://github.com/mitchellh/cli/blob/master/ui.go
	// to support dependents

	// Ui is an interface for interacting with the terminal, or "interface"
	// of a CLI. This abstraction doesn't have to be used, but helps provide
	// a simple, layerable way to manage user interactions.
	//~ type Ui interface {
	public interface Ui
	{
		// Ask asks the user for input using the given query. The response is
		// returned as the given string, or an error.
		//~ Ask(string) (string, error)
		string Ask(string prompt);

		// AskSecret asks the user for input using the given query, but does not echo
		// the keystrokes to the terminal.
		//~ AskSecret(string) (string, error)
		string AskSecret(string prompt);

		// Output is called for normal standard output.
		//~ Output(string)
		void Output(string message);

		// Info is called for information related to the previous output.
		// In general this may be the exact same as Output, but this gives
		// Ui implementors some flexibility with output formats.
		//~ Info(string)
		void Info(string s);

		// Error is used for any error messages that might appear on standard
		// error.
		//~ Error(string)
		void Error(string s);

		// Warn is used for any warning messages that might appear on standard
		// error.
		//~ Warn(string)
		void Warn(string s);
    }
}

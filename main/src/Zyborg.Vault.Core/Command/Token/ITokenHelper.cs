using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Command.Token
{
	// TokenHelper is an interface that contains basic operations that must be
	// implemented by a token helper
	//~ type TokenHelper interface {
	//~ 	// Path displays a backend-specific path; for the internal helper this
	//~ 	// is the location of the token stored on disk; for the external helper
	//~ 	// this is the location of the binary being invoked
	//~ 	Path() string
	//~ 
	//~ 	Erase() error
	//~ 	Get() (string, error)
	//~ 	Store(string) error
	//~ }
	public interface ITokenHelper
    {
		string Path
		{ get; }

		void Erase();

		string Get();

		void Store(string s);
    }
}

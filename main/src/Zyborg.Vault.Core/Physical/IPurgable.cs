using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	// Purgable is an optional interface for backends that support
	// purging of their caches.
	//~ type Purgable interface {
	//~ 	Purge()
	//~ }
    public interface IPurgable
    {
		void Purge();
    }
}

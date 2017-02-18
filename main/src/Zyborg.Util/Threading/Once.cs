using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Zyborg.Util.Threading
{
    public class Once
    {
		private bool _done;
		private object _l = new object();

		public Once()
		{ }

		public void Execute(Action action)
		{
			lock (_l)
			{
				if (!_done)
				{
					_done = true;
					action();
				}
			}
		}
    }
}

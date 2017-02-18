using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
	/// <summary>
	/// Implements deferrable actions that can be delayed until the end of a block
	/// of statements.
	/// </summary>
	/// <remarks>
	/// This class is meant to mimic the behavior of the Golang
	/// [defer statement](https://blog.golang.org/defer-panic-and-recover)
	/// in that it allows one to collect a set of actions whose execution should
	/// be delayed.  Although Golang statement automatically is tied to the end
	/// of the enclosing function call, this class can be used in a similar fashion
	/// when used in concert with the disposable resource construction <c>using</c>.
	/// <para>
	/// For example, you would use it as follows:
	/// <code>
	/// using (var defer = new Defer())
	/// {
	///		var fs = new FileStream("somefile.txt");
	///     defer.Add(() => fs.Close());
	///		
	///     // Do some logic here
	///     
	///     var ms = new MemoryStream();
	///     defer.Add(() => ms.Close());
	///     
	///     fs.CopyTo(ms);
	///     
	///		// Do some more logic here
	///		
	///     // When the using statement block ends, 
	///     // the deferred statements will be executed
	///     // in reverse order, e.g.:
	///     //    > ms.Close();
	///     //    > fs.Close();
	/// }
	/// </code>
	/// </para>
	/// </remarks>
	public class Defer : IDisposable
	{
		private List<Action> _deferredActions = new List<Action>();

		public void Add(Action deferred)
		{
			_deferredActions.Add(deferred);
		}

		public void Add<T1>(Action<T1> deferred, T1 arg1)
		{
			_deferredActions.Add(() => deferred(arg1));
		}

		public void Add<T1, T2>(Action<T1, T2> deferred, T1 arg1, T2 arg2)
		{
			_deferredActions.Add(() => deferred(arg1, arg2));
		}

		public void Add<T1, T2, T3>(Action<T1, T2, T3> deferred,
				T1 arg1, T2 arg2, T3 arg3)
		{
			_deferredActions.Add(() => deferred(arg1, arg2, arg3));
		}

		public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> deferred,
				T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			_deferredActions.Add(() => deferred(arg1, arg2, arg3, arg4));
		}

		public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> deferred,
				T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			_deferredActions.Add(() => deferred(arg1, arg2, arg3, arg4, arg5));
		}

		public void Dispose()
		{
			// TODO: how should exceptions be handled?
			// pby want to catch and collect all of them
			// then throw as an Aggregate if there are any

			// We reverse the entries to mimic last-in-first-out order
			// as per the Golang defer behavior
			_deferredActions.Reverse();
			var exList = new List<Exception>();
			foreach (var da in _deferredActions)
			{
				try
				{
					da();
				}
				catch (Exception ex)
				{
					exList.Add(ex);
				}
			}

			if (exList.Count > 0)
				throw new AggregateException("deferred action generated exceptions", exList);
		}
	}
}

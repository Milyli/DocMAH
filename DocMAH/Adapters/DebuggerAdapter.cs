using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DocMAH.Adapters
{
	public class DebuggerAdapter : IDebugger 
	{
		public bool IsAttached { get { return Debugger.IsAttached; } }
	}
}

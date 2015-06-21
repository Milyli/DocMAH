using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Adapters
{
	public interface IDebugger
	{
		bool IsAttached { get; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Data
{
	public interface IDatabaseConfiguration
	{
		int DatabaseHelpVersion { get; set; }
		int DatabaseSchemaVersion { get; set; }
	}
}

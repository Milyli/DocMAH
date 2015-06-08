using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Data
{
	public interface IConfigurationService
	{
		int DatabaseHelpVersion { get; set; }
		int DatabaseSchemaVersion { get; set; }
	}
}

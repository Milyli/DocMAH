using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Web.Authorization
{
	public interface IEditAuthorizer
	{
		bool Authorize();
	}
}

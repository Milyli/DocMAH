using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Data.Sql
{
	public abstract class BaseSqlRepository
	{
		#region Constructors

		public BaseSqlRepository(ISqlConnectionFactory sqlConnectionFactory)
		{
			SqlConnectionFactory = sqlConnectionFactory;
		}

		#endregion

		#region Protected Properties

		protected ISqlConnectionFactory SqlConnectionFactory { get; set; }

		#endregion

	}
}

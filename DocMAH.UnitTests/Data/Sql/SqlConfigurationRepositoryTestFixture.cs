﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlConfigurationRepositoryTestFixture : BaseSqlRepositoryTestFixture
	{
		#region Tests

		[Test]
		[Description("Exercises any existing configuration table CRUD.")]
		public void Crud_Success()
		{
			var helpVersion = ConfigurationRepository.Read(DocMAH.Configuration.DataStoreConfiguration.DataStoreSchemaVersionKey);
			Assert.That(helpVersion, Is.EqualTo((int)EnumExtensions.GetMaxValue<DataStoreSchemaVersions>()), "Current version should match the update script.");

			ConfigurationRepository.Update(DocMAH.Configuration.DataStoreConfiguration.DataStoreSchemaVersionKey, int.MaxValue);
			helpVersion = ConfigurationRepository.Read(DocMAH.Configuration.DataStoreConfiguration.DataStoreSchemaVersionKey);
			Assert.That(helpVersion, Is.EqualTo(int.MaxValue), "Version should have been updated to the max int value.");
		}
		
		#endregion
	}
}

using DocMAH.Data.Sql;

namespace DocMAH.Data
{
	public class ConfigurationService : IConfigurationService
	{
		#region Constructors

		public ConfigurationService(IConfigurationRepository configurationRepository)
		{
			_configurationRepository = configurationRepository;
		}

		#endregion

		#region Constants

		public const string DataStoreSchemaVersionKey = "DatabaseSchemaVersion";
		public const string HelpContentVersionKey = "HelpContentVersion";

		#endregion

		#region Private Fields

		private IConfigurationRepository _configurationRepository;

		#endregion

		#region Public Properties

		public int HelpContentVersion
		{
			get { return _configurationRepository.Read(HelpContentVersionKey); }
			set { _configurationRepository.Update(HelpContentVersionKey, value); }
		}

		public int DataStoreSchemaVersion
		{
			get { return _configurationRepository.Read(DataStoreSchemaVersionKey); }
			set { _configurationRepository.Update(DataStoreSchemaVersionKey, value); }
		}

		#endregion
	}
}

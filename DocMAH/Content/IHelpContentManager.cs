
namespace DocMAH.Content
{
	/// <summary>
	/// Manages the help content file.
	/// </summary>
	public interface IHelpContentManager
	{
		/// <summary>
		/// Generates an XML based file containing all content that may be used to move content to different environments.
		/// </summary>
		void ExportContent(string fileName);

		/// <summary>
		/// Reads the XML file containing all content and imports it into the data store.
		/// </summary>
		void ImportContent(string fileName);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data
{
	public interface IDocumentationPageRepository
	{
		/// <summary>
		/// Add a new page in the data store.
		/// </summary>
		/// <param name="page"></param>
		void Create(DocumentationPage page);

		/// <summary>
		/// Delete an existing page from the data store.   
		/// </summary>
		/// <param name="id"></param>
		void Delete(int id);

		/// <summary>
		/// Used during import to delete outdated content.
		/// </summary>
		/// <param name="pageIds"></param>
		void DeleteExcept(List<int> pageIds);

		/// <summary>
		/// Imports a page into the data store to be created or updated based on its 
		/// current state in the data store.
		/// </summary>
		/// <param name="page"></param>
		void Import(DocumentationPage page);

		/// <summary>
		/// Reads a page by its id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		DocumentationPage Read(int id);

		/// <summary>
		/// Read all pages from the data store.
		/// </summary>
		/// <returns></returns>
		IEnumerable<DocumentationPage> ReadAll();

		/// <summary>
		/// Reads pages by parent id.
		/// </summary>
		/// <param name="parentId">null to read pages without parents.</param>
		/// <returns></returns>
		List<DocumentationPage> ReadByParentId(int? parentId);
		
		/// <summary>
		/// Reads page id, order, title and parent id.
		/// Result list first contains pages having no parents 
		/// followed by their children and so on.
		/// </summary>
		/// <param name="includeHidden">false for public pages only.</param>
		/// <returns></returns>
		List<DocumentationPage> ReadTableOfContents(bool includeHidden);

		/// <summary>
		/// Updates an existing page in the datastore.
		/// </summary>
		/// <param name="page"></param>
		void Update(DocumentationPage page);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data
{
	public interface IPageRepository
	{
		/// <summary>
		/// Add a new page in the data store.
		/// </summary>
		/// <param name="page"></param>
		void Create(Page page);

		/// <summary>
		/// Delete an existing page from the data store.   
		/// </summary>
		/// <param name="id"></param>
		void Delete(int id);

		/// <summary>
		/// Reads a page by its id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Page Read(int id);

		/// <summary>
		/// Read all pages from the data store.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Page> ReadAll();

		/// <summary>
		/// Reads pages by parent id.
		/// </summary>
		/// <param name="parentId">null to read pages without parents.</param>
		/// <returns></returns>
		List<Page> ReadByParentId(int? parentId);

		/// <summary>
		/// Reads a page for the requested URL.
		/// The requested URL will match wildcards stored in data store.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		Page ReadByUrl(string url);

		/// <summary>
		/// Reads page id, order, title and parent id.
		/// Result list first contains pages having no parents 
		/// followed by their children and so on.
		/// </summary>
		/// <param name="includeHidden">false for public pages only.</param>
		/// <returns></returns>
		List<Page> ReadTableOfContents(bool includeHidden);

		/// <summary>
		/// Updates an existing page in the datastore.
		/// </summary>
		/// <param name="page"></param>
		void Update(Page page);
	}
}

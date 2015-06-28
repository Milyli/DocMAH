using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data
{
	public interface IFirstTimeHelpRepository
	{
		void Create(FirstTimeHelp help);
		void Delete(int id);
		void DeleteExcept(List<int> helpIds);
		void Import(FirstTimeHelp help);
		FirstTimeHelp Read(int id);
		IEnumerable<FirstTimeHelp> ReadAll();
		FirstTimeHelp ReadByUrl(string url);
		void Update(FirstTimeHelp help);
	}
}

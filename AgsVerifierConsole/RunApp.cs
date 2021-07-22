using AgsVerifierLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierConsole
{
	public class RunApp
	{
		private DataAccess _dataAccess;

		public RunApp()
		{
			_dataAccess = new DataAccess();

			string filePath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\ags example.ags";
			string dictPath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\Standard_dictionary_v4_1_0.ags";


			_dataAccess.ParseAgsDictionary(dictPath);
			_dataAccess.ParseAgsFile(filePath);

			//var groups = ProcessFile(filePath, true); // TEMP added true to stop error checks

			//GroupBaseRules.CheckGroups(groups, RuleErrors);
		}
	}
}

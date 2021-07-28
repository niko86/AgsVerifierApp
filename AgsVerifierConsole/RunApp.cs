using AgsVerifierLibrary;

namespace AgsVerifierConsole
{
    public class RunApp
	{
		private DataAccess _dataAccess;

		public RunApp()
		{
			_dataAccess = new DataAccess();

			string filePath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\rule11-3.ags";
			string dictPath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\Standard_dictionary_v4_1_0.ags";


			_dataAccess.ParseAgsDictionary(dictPath);
			_dataAccess.ParseAgsFile(filePath);

			//_dataAccess.
		}
	}
}

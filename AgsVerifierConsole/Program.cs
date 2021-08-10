using AgsVerifierLibrary;
using System;

namespace AgsVerifierConsole
{
    public class Program
    {
        public static void Main()
        {
            _ = new RunApp();
        }
    }

    public class RunApp
    {
        private readonly DataAccess _dataAccess;

        public RunApp()
        {
            string filePath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\ags example.ags";
            string dictPath = @"C:\Users\i.nicholson.fugro-global\Desktop\csharp-test\Standard_dictionary_v4_1_0.ags";

            //_dataAccess = new DataAccess(dictPath, filePath);

            //_dataAccess.ParseAgsDictionary();
            //_dataAccess.ParseAgsFile();
        }
    }
}

using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary
{
    public class DataAccess
    {
        private List<AgsGroupModel> _stdDictionary;
        private List<AgsGroupModel> _agsGroups;

        public DataAccess()
        {

        }

        public void ParseAgsDictionary(string dictPath)
        {
            ProcessAgsFile processAgsFile = new(dictPath);
            _stdDictionary = processAgsFile.ReturnGroupModels(rowChecks: false); // Dictionaries not compliant with \r\n line ending rule why was having issues with df
        }

        public void ParseAgsFile(string filePath) //TODO add indexes for group heading unit type and data (1st only??)
        {
            DataFrame df = _stdDictionary.FirstOrDefault(d => d.Name == "DICT").DataFrame;
            ProcessAgsFile processAgsFile = new(filePath, df);
            _agsGroups = processAgsFile.ReturnGroupModels(rowChecks: true);   
        }
    }
}

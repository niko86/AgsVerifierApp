using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
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
        private readonly List<RuleErrorModel> _ruleErrors;
        private List<AgsGroupModel> _stdDictionary;
        private List<AgsGroupModel> _agsGroups;

        public DataAccess()
        {
            _ruleErrors = new List<RuleErrorModel>();
        }

        public void ParseAgsDictionary(string dictPath)
        {
            ProcessAgsFile processAgsFile = new(dictPath);
            _stdDictionary = processAgsFile.ReturnGroupModels(rowChecks: false);
        }

        public void ParseAgsFile(string filePath) 
        {
            AgsGroupModel stdDictGroup = _stdDictionary.FirstOrDefault(d => d.Name == "DICT");
            ProcessAgsFile processAgsFile = new(filePath, stdDictGroup, _ruleErrors);
            _agsGroups = processAgsFile.ReturnGroupModels(rowChecks: true);
            GroupBasedRules.CheckGroups(_agsGroups, _ruleErrors);
        }
    }
}

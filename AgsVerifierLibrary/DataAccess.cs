using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;
using System.Linq;

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
            ProcessAgsFile processAgsFile = new(filePath, _ruleErrors, _stdDictionary);
            _agsGroups = processAgsFile.ReturnGroupModels(rowChecks: true);
            
            GroupBasedRules groupRules = new(_agsGroups, _stdDictionary, _ruleErrors);
            groupRules.CheckGroups();

            _ruleErrors.Sort((a, b) => a.RuleId.CompareTo(b.RuleId));

            foreach (var error in _ruleErrors)
            {
                System.Console.WriteLine($"{error.RuleId} - {error.Message}");
            }
        }
    }
}

using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;

namespace AgsVerifierLibrary
{
    public class DataAccess
    {
        private readonly AgsContainer _ags;
        private readonly List<RuleError> _ruleErrors;
        private readonly AgsContainer _stdDictionary;

        public DataAccess(string dictPath, string filePath)
        {
            _ags = new AgsContainer(filePath);
            _ruleErrors = new List<RuleError>();
            _stdDictionary = new AgsContainer(dictPath);
        }

        public void ParseAgsDictionary()
        {
            ProcessAgsFile processAgsFile = new(_stdDictionary);
            processAgsFile.Process();
        }

        public void ParseAgsFile() 
        {
            ProcessAgsFile processAgsFile = new(_ags, _ruleErrors, _stdDictionary);
            processAgsFile.Process();
            
            GroupBasedRules groupRules = new(_ags, _ruleErrors, _stdDictionary);
            groupRules.CheckGroups();

            foreach (var error in _ruleErrors)
            {
                System.Console.WriteLine($"{error.RuleId} - {error.Group} - {error.Message}");
            }
        }
    }
}

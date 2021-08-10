using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Comparers;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary
{
    public class DataAccess
    {
        private readonly AgsContainer _ags;
        private readonly List<RuleError> _errors;
        private readonly AgsContainer _stdDictionary;

        public List<RuleError> Errors => _errors;

        public DataAccess()
        {
            _ags = new AgsContainer();
            _errors = new List<RuleError>();
            _stdDictionary = new AgsContainer();
        }

        public bool ValidateAgsFile(AgsVersion version, string filePath)
        {
            // TODO make api more fluent and check that in nested checks that continue/return are being used correctly. Too complex logic.
            // TODO AGS 4.0.4 do i need to check types if exist in std dictionary?
            try
            {
                _ags.FilePath = filePath;

                _ = new ProcessAgsFile(version, _stdDictionary);
                _ = new ProcessAgsFile(version, _ags, _errors, _stdDictionary);
                _ = new GroupBasedRules(_ags, _errors, _stdDictionary);

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}

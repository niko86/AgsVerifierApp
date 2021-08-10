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
        public AgsContainer Ags { get; private set; }
        public List<RuleError> Errors { get; private set; }
        public AgsContainer StdDictionary { get; private set; }

        public DataAccess()
        {
            Ags = new AgsContainer();
            Errors = new List<RuleError>();
            StdDictionary = new AgsContainer();
        }

        public bool ValidateAgsFile(AgsVersion version, string filePath)
        {
            // TODO make api more fluent and check that in nested checks that continue/return are being used correctly. Too complex logic.
            // TODO AGS 4.0.4 do i need to check types if exist in std dictionary?
            try
            {
                Ags.FilePath = filePath;

                _ = new ProcessAgsFile(version, StdDictionary);
                _ = new ProcessAgsFile(version, Ags, Errors, StdDictionary);
                _ = new GroupBasedRules(Ags, Errors, StdDictionary);

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}

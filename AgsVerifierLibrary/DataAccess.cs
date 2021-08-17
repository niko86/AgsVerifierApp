using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<bool> ValidateAgsFile(AgsVersion version, string filePath)
        {
            // TODO make api more fluent and check that in nested checks that continue/return are being used correctly. Too complex logic.
            // TODO AGS 4.0.4 do i need to check types if exist in std dictionary?
            try
            {
                Ags.FilePath = filePath;

                ProcessAgsFile processStdDictionary = new(version, StdDictionary);
                await Task.Run(() => processStdDictionary.Process());

                ProcessAgsFile processAgsFile = new(version, Ags, Errors, StdDictionary);
                await Task.Run(() => processAgsFile.Process());

                PerFileRules fileRules = new(Ags, Errors, StdDictionary);
                await Task.Run(() => fileRules.Process());

                PerGroupRules groupRules = new(Ags, Errors, StdDictionary);
                await Task.Run(() => groupRules.Process());

                return true;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
                //return false;
            }
        }
    }
}

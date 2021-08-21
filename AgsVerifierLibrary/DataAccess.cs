using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AgsVerifierLibrary
{
    public class DataAccess
    {
        public AgsContainer Ags { get; private set; }
        public List<RuleError> Errors { get; private set; }

        public DataAccess()
        {
            Ags = new AgsContainer();
            Errors = new List<RuleError>();
        }

        public async Task<bool> ValidateAgsFile(StreamReader stream, AgsVersion version, string filePath = null)
        {
            // Filepath only needed for Rule 20. Which would be useless in an API (unless a zip file?), work out how to remove/accomodate.
            Ags.FilePath = filePath;

            AgsContainer stdDictionary = new();
            stdDictionary.Groups = LoadResource.StdDictionary(version);

            ProcessAgsFile processAgsFile = new(Ags, Errors, stdDictionary);
            await Task.Run(() => processAgsFile.Process(stream));

            PerFileRules fileRules = new(Ags, Errors, stdDictionary);
            await Task.Run(() => fileRules.Process());

            PerGroupRules groupRules = new(Ags, Errors, stdDictionary);
            await Task.Run(() => groupRules.Process());

            return true;
        }
    }
}

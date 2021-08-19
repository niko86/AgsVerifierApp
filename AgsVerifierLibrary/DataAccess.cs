using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibrary.Properties;
using AgsVerifierLibrary.Rules;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public async Task<bool> ValidateAgsFile(StreamReader stream, AgsVersion version, string filePath)
        {
            // Filepath only needed for Rule 20. Which would be useless in an API (unless a zip file?), work out how to remove/accomodate.

            Ags.FilePath = filePath;

            ProcessAgsFile processStdDictionary = new(StdDictionary);

            using (StreamReader reader = new(new MemoryStream(Resources.ResourceManager.GetObject(version.ToString(), CultureInfo.InvariantCulture) as byte[])))
            {
                await Task.Run(() => processStdDictionary.Process(reader));
            }

            ProcessAgsFile processAgsFile = new(Ags, Errors, StdDictionary);
            await Task.Run(() => processAgsFile.Process(stream));

            PerFileRules fileRules = new(Ags, Errors, StdDictionary);
            await Task.Run(() => fileRules.Process());

            PerGroupRules groupRules = new(Ags, Errors, StdDictionary);
            await Task.Run(() => groupRules.Process());

            return true;
        }
    }
}

using AgsVerifierLibrary;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Models;
using AgsVerifierLibraryTests.Properties;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AgsVerifierLibraryTests.Tests
{
    public class Rule1Test
    {
        [Fact]
        public async void PassingRule1Raised()
        {
            var errors = await ReturnErrors(AgsVersion.V410, "V410_rule1");
            Assert.Contains(errors, e => e.RuleId == 100);
            Assert.Single(errors);
        }

        static async Task<List<RuleError>> ReturnErrors(AgsVersion version, string objectName)
        {
            DataAccess dataAccess = new();
            using (StreamReader reader = new(new MemoryStream(Resources.ResourceManager.GetObject(objectName) as byte[])))
            {
                await dataAccess.ValidateAgsFile(reader, version);
            }
            return dataAccess.Errors;
        }
    }
}

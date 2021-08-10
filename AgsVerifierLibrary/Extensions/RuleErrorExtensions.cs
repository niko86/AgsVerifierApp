using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Extensions
{
    public static class RuleErrorExtensions
    {
        public static bool Contains(this IEnumerable<RuleError> errors, string text)
        {
            return errors.Select(e => e.RuleName).Contains(text);
        }

        public static bool Contains(this IEnumerable<RuleError> errors, int id)
        {
            return errors.Any(e => e.RuleId == id);
        }
    }
}

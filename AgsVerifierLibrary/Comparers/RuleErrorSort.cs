using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Comparers
{
    public class RuleErrorSort : IComparer<RuleError>
    {
        private static readonly Regex _regex = new(@"(\d+)");

        public int Compare(RuleError a, RuleError b)
        {
            var resultA = _regex.Match(a.RuleId);
            var resultB = _regex.Match(b.RuleId);

            if (resultA.Success && resultB.Success)
            {
                return int.Parse(resultA.Groups[1].Value).CompareTo(int.Parse(resultB.Groups[1].Value));
            }

            return a.RuleId.CompareTo(b.RuleId);
        }
    }
}

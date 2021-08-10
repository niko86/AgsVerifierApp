using AgsVerifierLibrary.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Comparers
{
    public class RuleErrorSort : IComparer<RuleError>
    {
        private static readonly Regex _regex = new(@"(\d+)");

        public int Compare(RuleError a, RuleError b)
        {
            var resultA = _regex.Match(a.RuleName);
            var resultB = _regex.Match(b.RuleName);

            return resultA.Success && resultB.Success
                ? int.Parse(resultA.Groups[1].Value).CompareTo(int.Parse(resultB.Groups[1].Value))
                : a.RuleName.CompareTo(b.RuleName);
        }
    }
}

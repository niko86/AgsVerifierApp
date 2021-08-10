using System;

namespace AgsVerifierLibrary.Models
{
    public class RuleError
    {
        public string Status { get; set; }
        public string RuleName { get; set; }
        public int RuleId { get; set; }
        public int RowNumber { get; set; }
        public string Message { get; set; }
        public string Group { get; set; }
        public string Field { get; set; }
    }
}
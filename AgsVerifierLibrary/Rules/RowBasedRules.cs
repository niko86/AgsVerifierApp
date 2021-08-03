using AgsVerifierLibrary.Models;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    public static class RowBasedRules
    {
        private static readonly Regex _regexCsvRowSplit = new(@",(?=(?:""[^""]*?(?:[^""]*)*))|,(?=[^"", ]+(?:, |$))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regexOnlyWhiteSpace = new(@"^\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public static void CheckRow(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            Rule1(csv, errors);
            Rule2a(csv, errors, group);
            Rule2c(csv, errors, group);
            Rule3(csv, errors, group);
            Rule4a(csv, errors, group);
            Rule4b(csv, errors, group);
            Rule5(csv, errors, group);
            Rule6();
        }

        private static void Rule1(CsvReader csv, List<RuleError> errors)
        {
            if (Encoding.UTF8.GetByteCount(csv.Parser.RawRecord) == csv.Parser.RawRecord.Length)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "1",
                RowNumber = csv.Parser.RawRow,
                Message = "Has Non-ASCII character(s).",
            });
        }

        private static void Rule2a(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            if (csv.Parser.RawRecord.EndsWith("\r\n"))
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "2a",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Is not terminated by <CR> and <LF> characters.",
            }
            );
        }

        private static void Rule2c(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            // DOES THIS EXIST???

            if (csv.GetField(0) != "HEADING")
                return;

            if (csv.Parser.Record.GroupBy(r => r).Count() == csv.Parser.Record.Length)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "2c",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "HEADER row has duplicate fields.",
            }
            );
        }

        private static void Rule3(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            List<string> descriptors = new() { "GROUP", "HEADING", "TYPE", "UNIT", "DATA" };

            if (descriptors.Any(d => d.Contains(csv.GetField(0))))
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "3",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Does not start with a valid data descriptor.",
            }
            );
        }

        private static void Rule4a(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            if (csv.GetField(0) != "GROUP")
                return;

            if (csv.Parser.Record.Length == 2)
                return;

            if (csv.Parser.Record.Length > 2)
            {
                errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleId = "4a",
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "GROUP row has more than one field.",
                });
                return;
            }

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "4a",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "GROUP row is malformed.",
            }
            );
        }

        private static void Rule4b(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            List<string> descriptors = new() { "TYPE", "UNIT", "DATA" };

            if (descriptors.Any(d => d.Contains(csv.GetField(0))) == false)
                return;

            if (group.Columns.Select(c => c.Heading) == null)
            {
                errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleId = "4b",
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "HEADING row missing.",
                }
                );
                return;
            }

            var headings = group.Columns.Select(c => c.Heading);

            if (headings.Count() == csv.Parser.Record.Length)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "4b",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Number of fields does not match the HEADING row.",
            }
            );
        }

        private static void Rule5(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            if (csv.Parser.RawRecord == "\r\n" || csv.Parser.RawRecord == "\n")
                return;

            var rawSplit = _regexCsvRowSplit.Split(csv.Parser.RawRecord.TrimEnd());

            bool containsOrphanQuotes = csv.Parser.Record.Count(r => r.Contains('"')) / 2 > 0;

            if (containsOrphanQuotes)
            {
                errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleId = "5",
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "Contains quotes within a data field. All such quotes should be enclosed by a second quote.",
                });
            }

            bool containsOnlySpaces = csv.Parser.Record.Where(r => _regexOnlyWhiteSpace.IsMatch(r) && string.IsNullOrEmpty(r) == false).Any();

            if (containsOnlySpaces)
            {
                errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleId = "5",
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "Contains only white space characters.",
                });
            }

            bool allStartsWithDoubleQuote = rawSplit.All(s => s.StartsWith('"'));
            bool allEndsWithDoubleQuote = rawSplit.All(s => s.EndsWith('"'));

            if (allStartsWithDoubleQuote && allEndsWithDoubleQuote)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleId = "5",
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Contains fields that are not enclosed in double quotes.",
            }
            );
        }

        private static void Rule6()
        {
            return;
        }
    }
}

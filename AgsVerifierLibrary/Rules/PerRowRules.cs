using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    public static class PerRowRules
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

        /// <summary>
        /// The data file shall be entirely composed of ASCII characters.
        /// </summary>
        private static void Rule1(CsvReader csv, List<RuleError> errors)
        {
            if (Encoding.UTF8.GetByteCount(csv.Parser.RawRecord) == csv.Parser.RawRecord.Length)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "1",
                RuleId = 100,
                RowNumber = csv.Parser.RawRow,
                Message = "Has Non-ASCII character(s).",
            });
        }

        /// <summary>
        /// Each row is located on a separate line, delimited by a new line consisting 
        /// of a carriage return (ASCII character 13) and a line feed (ASCII character 10).
        /// </summary>
        private static void Rule2a(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            if (csv.Parser.RawRecord.EndsWith("\r\n"))
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "2a",
                RuleId = 210,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Is not terminated by <CR> and <LF> characters.",
            });
        }

        /// <summary>
        /// Not official rule: No duplicate HEADER row fields.
        /// </summary>
        private static void Rule2c(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            if (csv.GetField(0) != "HEADING")
                return;

            if (csv.Parser.Record.GroupBy(r => r).Count() == csv.Parser.Record.Length)
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "2c",
                RuleId = 230,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "HEADER row has duplicate fields.",
            });
        }

        /// <summary>
        /// Each row in the data file must start with a DATA DESCRIPTOR that defines the 
        /// contents of that row. The following Data Descriptors are used as described below:
        /// <list type="bullet">
        /// <item><description>Each GROUP row shall be preceded by the "GROUP" Data Descriptor.</description></item>
        /// <item><description>Each HEADING row shall be preceded by the "HEADING" Data Descriptor.</description></item>
        /// <item><description>Each UNIT row shall be preceded by the "UNIT" Data Descriptor.</description></item>
        /// <item><description>Each TYPE row shall be preceded by the "TYPE" Data Descriptor.</description></item>
        /// <item><description>Each DATA row shall be preceded by the "DATA" Data Descriptor.</description></item>
        /// </list>
        /// </summary>
        private static void Rule3(CsvReader csv, List<RuleError> errors, AgsGroup group)
        {
            List<string> descriptors = new() { "GROUP", "HEADING", "TYPE", "UNIT", "DATA" };

            if (descriptors.Any(d => d.Contains(csv.GetField(0))))
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "3",
                RuleId = 300,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Does not start with a valid data descriptor.",
            });
        }

        /// <summary>
        /// Within each GROUP, the DATA items are contained in data FIELDs. Each data FIELD contains a single 
        /// data VARIABLE in each row. Each DATA row of a data file will contain one or more data FIELDs.
        /// The GROUP row contains only one DATA item, the GROUP name, in addition to the Data Descriptor(Rule 3).
        /// </summary>
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
                    RuleName = "4a",
                    RuleId = 410,
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "GROUP row has more than one field.",
                });
                return;
            }

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "4a",
                RuleId = 411,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "GROUP row is malformed.",
            });
        }

        /// <summary>
        /// All other rows in the GROUP have a number of DATA items defined by the HEADING row.
        /// </summary>
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
                    RuleName = "4b",
                    RuleId = 420,
                    RowNumber = csv.Parser.RawRow,
                    Group = group.Name,
                    Message = "HEADING row missing.",
                });
                return;
            }

            var test = group.ReturnDescriptor(AgsDescriptor.HEADING);
            var headings = group.Columns.Select(c => c.Heading);

            if (headings.Count() == csv.Parser.Record.Length + 1) // +1 to account for index column
                return;

            errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "4b",
                RuleId = 421,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Number of fields does not match the HEADING row.",
            });
        }

        /// <summary>
        /// DATA DESCRIPTORS, GROUP names, data field HEADINGs, data field UNITs, data field TYPEs, and 
        /// data VARIABLEs shall be enclosed in double quotes("..."). Any quotes within a data item must be 
        /// defined with a second quote e.g. "he said ""hello""".
        /// </summary>
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
                    RuleName = "5",
                    RuleId = 500,
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
                    RuleName = "5",
                    RuleId = 501,
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
                RuleName = "5",
                RuleId = 502,
                RowNumber = csv.Parser.RawRow,
                Group = group.Name,
                Message = "Contains fields that are not enclosed in double quotes.",
            });
        }

        /// <summary>
        /// The DATA DESCRIPTORS, GROUP names, data field HEADINGs, data field UNITs, data 
        /// field TYPEs, and data VARIABLEs in each line of the data file shall be separated by a 
        /// comma(,). No carriage returns(ASCII character 13) or line feeds(ASCII character 10) are 
        /// allowed in or between data VARIABLEs within a DATA row.
        /// </summary>
        private static void Rule6()
        {

        }
    }
}

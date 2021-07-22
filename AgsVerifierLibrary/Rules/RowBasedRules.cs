using AgsVerifierLibrary.Models;
using CsvHelper;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    class RowBasedRules
    {
        public static void CheckRow(CsvReader csv, List<RuleErrorModel> errors, AgsGroupModel group, DataFrame df)
        {
            string groupName = group.Name;
            string[] headings = group.Columns.Select(c => c.Heading).ToArray();

            Rule1(csv, errors);
            Rule2a(csv, errors, groupName);
            Rule2c(csv, errors, groupName);
            Rule3(csv, errors, groupName);
            Rule4a(csv, errors, groupName);
            Rule4b(csv, errors, groupName, headings);
            Rule5(csv, errors, groupName);
            Rule6(csv, errors, groupName);
            Rule19(csv, errors, groupName);
            Rule19a(csv, errors, groupName);
            //Rule19b_1(csv, errors, groupName, df); // Add to group checks
        }

        private static void Rule1(CsvReader csv, List<RuleErrorModel> errors)
        {
            if (Encoding.UTF8.GetByteCount(csv.Parser.RawRecord) == csv.Parser.RawRecord.Length)
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "1",
                RowNumber = csv.Parser.RawRow,
                Message = "Has Non-ASCII character(s).",
            });
        }

        private static void Rule2a(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.Parser.RawRecord.EndsWith("\r\n"))
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "2a",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "Is not terminated by <CR> and <LF> characters.",
            }
            );
        }

        private static void Rule2c(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.GetField(0) != "HEADING")
                return;

            if (csv.Parser.Record.GroupBy(r => r).Count() == csv.Parser.Record.Length)
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "2c",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "HEADER row has duplicate fields.",
            }
            );
        }

        private static void Rule3(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            List<string> descriptors = new() { "GROUP", "HEADING", "TYPE", "UNIT", "DATA" };

            if (descriptors.Any(d => d.Contains(csv.GetField(0))))
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "3",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "Does not start with a valid data descriptor.",
            }
            );
        }

        private static void Rule4a(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.GetField(0) != "GROUP")
                return;

            if (csv.Parser.Record.Length == 2)
                return;

            if (csv.Parser.Record.Length > 2)
            {
                errors.Add(new RuleErrorModel()
                {
                    Status = "Fail",
                    RuleId = "4a",
                    RowNumber = csv.Parser.RawRow,
                    Group = groupName,
                    Message = "GROUP row has more than one field.",
                });
                return;
            }

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "4a",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "GROUP row is malformed.",
            }
            );
        }

        private static void Rule4b(CsvReader csv, List<RuleErrorModel> errors, string groupName, string[] headings)
        {
            if (csv.Parser.RawRecord == "\r\n" || csv.Parser.RawRecord == "\n")
                return;

            List<string> descriptors = new() { "TYPE", "UNIT", "DATA" };

            if (descriptors.Any(d => d.Contains(csv.GetField(0))) == false)
                return;

            if (headings == null)
            {
                errors.Add(new RuleErrorModel()
                {
                    Status = "Fail",
                    RuleId = "4b",
                    RowNumber = csv.Parser.RawRow,
                    Group = groupName,
                    Message = "HEADING row missing.",
                }
                );
                return;
            }

            if (headings.Length == csv.Parser.Record.Length)
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "4b",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "Number of fields does not match the HEADING row.",
            }
            );
        }

        private static void Rule5(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.Parser.RawRecord == "\r\n" || csv.Parser.RawRecord == "\n")
                return;

            var rawSplit = Regex.Split(csv.Parser.RawRecord.TrimEnd(), @",(?=(?:""[^""]*?(?:[^""]*)*))|,(?=[^"", ]+(?:, |$))");

            bool containsOrphanQuotes = csv.Parser.Record.Count(r => r.Contains('"')) / 2 > 0;

            if (containsOrphanQuotes)
            {
                errors.Add(new RuleErrorModel()
                {
                    Status = "Fail",
                    RuleId = "5",
                    RowNumber = csv.Parser.RawRow,
                    Group = groupName,
                    Message = "Contains quotes within a data field. All such quotes should be enclosed by a second quote.",
                });
            }

            bool containsOnlySpaces = csv.Parser.Record.Where(r => Regex.IsMatch(r, @"^\s*$") && string.IsNullOrEmpty(r) == false).Count() > 0;

            if (containsOnlySpaces)
            {
                errors.Add(new RuleErrorModel()
                {
                    Status = "Fail",
                    RuleId = "5",
                    RowNumber = csv.Parser.RawRow,
                    Group = groupName,
                    Message = "Contains only white space characters.",
                });
            }

            bool allStartsWithDoubleQuote = rawSplit.All(s => s.StartsWith('"'));
            bool allEndsWithDoubleQuote = rawSplit.All(s => s.EndsWith('"'));

            if (allStartsWithDoubleQuote && allEndsWithDoubleQuote)
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "5",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "Contains fields that are not enclosed in double quotes.",
            }
            );
        }

        private static void Rule6(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            return;
        }

        private static void Rule19(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.GetField(0) != "GROUP")
                return;

            if (csv.GetField(1).Length == 4 && csv.GetField(1).All(c => char.IsUpper(c)))
                return;

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "19",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "GROUP name should consist of four uppercase letters.",
            }
            );
        }

        private static void Rule19a(CsvReader csv, List<RuleErrorModel> errors, string groupName)
        {
            if (csv.GetField(0) != "HEADING")
                return;

            if (csv.Parser.Record.Length > 1)
            {
                csv.Parser.Record[1..]
                    .Where(r => Regex.IsMatch(r, @"[^A-Z0-9_]"))
                    .ToList()
                    .ForEach(field => errors.Add(
                        new RuleErrorModel()
                        {
                            Status = "Fail",
                            RuleId = "19a",
                            RowNumber = csv.Parser.RawRow,
                            Group = groupName,
                            Field = field,
                            Message = $"HEADING {field} should consist of only uppercase letters, numbers, and an underscore character.",
                        })
                        );

                csv.Parser.Record[1..]
                    .Where(r => r.Length > 9)
                    .ToList()
                    .ForEach(field => errors.Add(
                        new RuleErrorModel()
                        {
                            Status = "Fail",
                            RuleId = "19a",
                            RowNumber = csv.Parser.RawRow,
                            Group = groupName,
                            Field = field,
                            Message = $"HEADING {field} is more than 9 characters in length.",
                        })
                        );

                return;
            }

            errors.Add(new RuleErrorModel()
            {
                Status = "Fail",
                RuleId = "19a",
                RowNumber = csv.Parser.RawRow,
                Group = groupName,
                Message = "HEADING row does not have any fields.",
            }
            );
        }

        private static void Rule19b_1(CsvReader csv, List<RuleErrorModel> errors, string groupName, DataFrame df)
        {
            if (csv.GetField(0) != "HEADING")
                return;

            if (csv.Parser.Record.Length < 2)
                return;

            foreach (var field in csv.Parser.Record[1..])
            {
                if (field.Contains('_') == false)
                {
                    errors.Add(
                        new RuleErrorModel()
                        {
                            Status = "Fail",
                            RuleId = "19b_1",
                            RowNumber = csv.Parser.RawRow,
                            Group = groupName,
                            Field = field,
                            Message = $"HEADING {field} should consist of group name and field name separated by \"_\".",
                        });

                    return;
                }

                string[] splitHeading = field.Split('_');

                if (splitHeading[0].Length != 4 || splitHeading[1].Length > 4) //!= 4) or (len(item.split('_')[1]) > 4
                {
                    errors.Add(
                        new RuleErrorModel()
                        {
                            Status = "Fail",
                            RuleId = "19b_1",
                            RowNumber = csv.Parser.RawRow,
                            Group = groupName,
                            Field = field,
                            Message = $"HEADING {field} should consist of a 4 character group name and a field name of up to 4 characters.",
                        });
                }

                bool groupNameExists = df.Columns["DICT_GRP"].Cast<string>().Distinct().Any(s => s == splitHeading[0]);

                if (groupNameExists == false)
                {
                    errors.Add(
                        new RuleErrorModel()
                        {
                            Status = "Fail",
                            RuleId = "19b_1",
                            RowNumber = csv.Parser.RawRow,
                            Group = groupName,
                            Field = field,
                            Message = $"HEADING {field} group name not present in the standard dictionary.",
                        });
                }
            }
        }
    }
}

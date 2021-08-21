using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    public class PerGroupRules
    {
        private static readonly Regex _regexAgsHeadingField = new(@"[^A-Z0-9_]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly string[] _parentGroupExceptions = new string[] { "PROJ", "TRAN", "ABBR", "DICT", "UNIT", "TYPE", "LOCA", "FILE", "LBSG", "PREM", "STND" };

        private readonly AgsContainer _ags;
        private readonly AgsContainer _stdDictionary;
        private readonly List<RuleError> _errors;

        public PerGroupRules(AgsContainer ags, List<RuleError> errors, AgsContainer stdDictionary)
        {
            _ags = ags;
            _stdDictionary = stdDictionary;
            _errors = errors;
        }

        public void Process()
        {
            foreach (AgsGroup group in _ags.Groups)
            {
                Rule2(group);
                Rule2b(group);
                Rule7(group);
                Rule9(group);
                Rule10a(group);
                Rule10b(group);
                Rule10c(group);
                Rule11c(group);
                Rule18a(group);
                Rule19(group);
                Rule19a(group);
                Rule19b(group);
            }
        }

        /// <summary>
        /// Each data file shall contain one or more data GROUPs. Each data GROUP shall comprise a number of
        /// GROUP HEADER rows and must have one or more DATA rows.
        /// </summary>
        private void Rule2(AgsGroup group)
        {
            if (group.RowCount > 0)
                return;

            _errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "2",
                RuleId = 200,
                Group = group.Name,
                Message = $"No DATA rows in the {group.Name} table.",
            });
        }

        /// <summary>
        /// The GROUP HEADER rows fully define the data presented within the DATA rows for that group (Rule 8). As a 
        /// minimum, the GROUP HEADER rows comprise GROUP, HEADING, UNIT and TYPE rows presented in that order.
        /// </summary>
        private void Rule2b(AgsGroup group)
        {
            List<string> descriptors = new() { "HEADING", "UNIT", "TYPE" };

            foreach (var descriptor in descriptors)
            {
                if (group.GetGroupDescriptorRowNumber((AgsDescriptor)Enum.Parse(typeof(AgsDescriptor), descriptor)) == 0)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "2b",
                        RuleId = 220,
                        Group = group.Name,
                        RowNumber = group.GroupRow,
                        Message = $"{descriptor} row missing from the {group.Name} group.",
                    });
                }
            }

            bool orderTestA = group.HeadingRow < group.UnitRow && group.UnitRow > group.TypeRow;
            bool orderTestB = group.HeadingRow > group.UnitRow && group.HeadingRow < group.TypeRow;
            bool orderTestC = group.HeadingRow > group.UnitRow && group.UnitRow > group.TypeRow;
            bool orderTestD = group.HeadingRow < group.UnitRow && group.HeadingRow > group.TypeRow;

            if (orderTestA || orderTestB)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "2b",
                    RuleId = 221,
                    Group = group.Name,
                    RowNumber = group.UnitRow,
                    Message = $"UNIT row is misplaced. It should be immediately below the HEADING row.",
                });
            }

            if (orderTestC || orderTestD)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "2b",
                    RuleId = 222,
                    Group = group.Name,
                    RowNumber = group.TypeRow,
                    Message = $"TYPE row is misplaced. It should be immediately below the UNIT row.",
                });
            }
        }

        /// <summary>
        /// The order of data FIELDs in each line within a GROUP is defined at the start of each GROUP in the HEADING row.
        /// HEADINGs shall be in the order described in the AGS FORMAT DATA DICTIONARY.
        /// </summary>
        private void Rule7(AgsGroup group)
        {
            var dictHeadings = _stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_TYPE", AgsDescriptor.HEADING).AllOf("DICT_HDNG");
            var groupHeadings = group.Columns.Select(c => c.Heading);

            var intersectDictWithFile = dictHeadings.Intersect(groupHeadings).ToArray();
            var intersectFileWithDict = groupHeadings.Intersect(dictHeadings).ToArray();

            if (intersectDictWithFile.SequenceEqual(intersectFileWithDict))
                return;

            for (int i = 0; i < intersectDictWithFile.Length; i++)
            {
                if (intersectDictWithFile[i] == intersectFileWithDict[i])
                    continue;

                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "7",
                    RuleId = 700,
                    Group = group.Name,
                    RowNumber = group.HeadingRow,
                    Message = $"Standard dictionary defined headings not in order starting from {intersectFileWithDict[i]}. Expected order: ...{string.Join('|', intersectDictWithFile[i..])}",
                });
                return;
            }
        }

        /// <summary>
        /// Data HEADING and GROUP names shall be taken from the AGS FORMAT DATA DICTIONARY. In cases where there is no suitable entry,
        /// a user-defined GROUP and/or HEADING may be used in accordance with Rule 18. Any user-defined HEADINGs shall be included at
        /// the end of the HEADING row after the standard HEADINGs in the order defined in the DICT group (see Rule 18a).
        /// </summary>
        private void Rule9(AgsGroup group)
        {
            var stdDictTableFilteredByGroup = _stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(group.Name);
            var stdDictTableName = stdDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.GROUP);
            var stdDictTableHeadings = stdDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.HEADING).AllOf("DICT_HDNG");

            if (stdDictTableName.Any() == false || stdDictTableHeadings.Any() == false)
            {
                if (_ags["DICT"] is null && _errors.Any(e => e.RuleId == 1800) == false)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "18",
                        RuleId = 1800,
                        Group = "DICT",
                        Message = "DICT table not found. See error log under Rule 9 for a list of non-standard headings that need to be defined in a DICT table.",
                    });
                }
            }

            var fileDictTableFilteredByGroup = _ags["DICT"]?["DICT_GRP"].FilterRowsBy(group.Name);
            var fileDictTableName = fileDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.GROUP);

            if (stdDictTableName.Any() == false && fileDictTableName.Any() == false)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "9",
                    RuleId = 900,
                    Group = group.Name,
                    RowNumber = group.HeadingRow,
                    Message = $"GROUP name not defined within either the AGS FORMAT DATA DICTIONARY or DICT table.",
                });
                return;
            }

            var fileDictTableHeadings = fileDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.HEADING).AllOf("DICT_HDNG");

            var combinedDictTableHeadings = stdDictTableHeadings.Concat(fileDictTableHeadings ?? Array.Empty<dynamic>()).Distinct();

            var groupHeadings = group.ReturnDescriptor(AgsDescriptor.HEADING);

            foreach (var groupHeading in groupHeadings)
            {
                if (groupHeadings.Count(h => h == groupHeading) > 1)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "9",
                        RuleId = 901,
                        Group = group.Name,
                        RowNumber = group.HeadingRow,
                        Message = $"Duplicate GROUP HEADING found in field  {groupHeading} found.",
                    });
                }

                if (combinedDictTableHeadings.Contains(groupHeading))
                    continue;

                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "9",
                    RuleId = 902,
                    Group = group.Name,
                    RowNumber = group.HeadingRow,
                    Message = $"{groupHeading} not found in DICT table or the standard AGS4 dictionary.",
                });
            }
        }

        /// <summary>
        /// In every GROUP, certain HEADINGs are defined as KEY. There shall not be more than one row of data in each GROUP with the
        /// same combination of KEY field entries. KEY fields must appear in each GROUP, but may contain null data(see Rule 12).
        /// </summary>
        private void Rule10a(AgsGroup group)
        {
            var keyColumns = group.ColumnsByStatus(AgsStatus.KEY);

            var keyHeadings = keyColumns.ReturnDescriptor(AgsDescriptor.HEADING);

            var dictKeyHeadings = _stdDictionary["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AndBy("DICT_STAT", AgsStatus.KEY).AllOf("DICT_HDNG");

            var differences = dictKeyHeadings.Except(keyHeadings);

            foreach (var diff in differences)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "10a",
                    RuleId = 1010,
                    Group = group.Name,
                    RowNumber = group.HeadingRow,
                    Message = $"KEY field {diff} not found.",
                });
            }

            var duplicateKeys = group.Rows.GroupBy(r => r.ToStringByStatus(AgsStatus.KEY)).Where(g => g.Count() > 1);

            if (duplicateKeys.Any())
            {
                foreach (var duplicateKey in duplicateKeys)
                {
                    foreach (var row in duplicateKey)
                    {
                        _errors.Add(new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "10a",
                            RuleId = 1011,
                            Group = group.Name,
                            RowNumber = row.Index,
                            Message = $"Duplicate key field combination: {duplicateKey.Key}",
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Some HEADINGs are marked as REQUIRED.REQUIRED fields must appear in the data GROUPs where they are indicated in the AGS FORMAT DATA DICTIONARY.
        /// These fields require data entry and cannot be null(i.e.left blank or empty).
        /// </summary>
        private void Rule10b(AgsGroup group)
        {
            var requiredColumns = group.ColumnsByStatus(AgsStatus.REQUIRED);
            var requiredHeadings = requiredColumns.ReturnDescriptor(AgsDescriptor.HEADING);
            var groupHeadings = group.ReturnDescriptor(AgsDescriptor.HEADING);

            foreach (var requiredHeading in requiredHeadings)
            {
                if (groupHeadings.Any(i => i == requiredHeading) == false)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "10b",
                        RuleId = 1020,
                        Group = group.Name,
                        RowNumber = group.HeadingRow,
                        Message = $"REQUIRED field {requiredHeading} not found.",
                    });
                }
            }

            var requiredRows = group.Rows.Where(r => r.ToStringByStatus(AgsStatus.REQUIRED).Contains("???"));

            foreach (var requiredRow in requiredRows)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "10b",
                    RuleId = 1021,
                    Group = group.Name,
                    RowNumber = requiredRow.Index,
                    Message = $"REQUIRED field(s) containing empty values: ...{requiredRow.ToStringByStatus(AgsStatus.REQUIRED)}",
                });
            }
        }

        /// <summary>
        /// Links are made between data rows in GROUPs by the KEY fields.
        /// Every entry made in the KEY fields in any GROUP must have an equivalent entry in its PARENT GROUP.
        /// The PARENT GROUP must be included within the data file.
        /// </summary>
        private void Rule10c(AgsGroup group)
        {
            if (_parentGroupExceptions.Contains(group.Name)) // TODO work out how to indicate exceptions within parent group model ref??
                return;

            if (group.ParentGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "10c",
                    RuleId = 1030,
                    Group = group.Name,
                    RowNumber = group.GroupRow,
                    Message = $"Parent group left blank in dictionary.",
                });
                return;
            }

            if (_ags.ReturnGroupNames().Contains(group.ParentGroup.Name) == false)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "10c",
                    RuleId = 1031,
                    Group = group.Name,
                    RowNumber = group.GroupRow,
                    Message = $"Could not find parent group {group.ParentGroup.Name}.",
                });
                return;
            }

            var parentKeyColumns = group.ParentGroup.GetColumnsOfStatus(AgsStatus.KEY);

            if (parentKeyColumns.Any() == false)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "10c",
                    RuleId = 1032,
                    Group = group.Name,
                    RowNumber = group.GroupRow,
                    Message = $"Could not check parent entries since group definitions not found in standard dictionary or DICT table.",
                });
                return;
            }

            foreach (var row in group.Rows)
            {
                foreach (var parentKeyColumn in parentKeyColumns)
                {
                    if (parentKeyColumn.Data.Contains(row[parentKeyColumn.Heading]) == false)
                    {
                        _errors.Add(new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "10c",
                            RuleId = 1033,
                            Group = group.Name,
                            RowNumber = row.Index,
                            Message = $"Parent key entry for line not found in {group.ParentGroup.Name}: {row.ToStringByMask(parentKeyColumns.ReturnDescriptor(AgsDescriptor.HEADING))}",
                        });
                    }
                }
            }
        }

        /// <summary>
        ///  Any heading of data TYPE 'Record Link' included in a data file shall cross-reference to the KEY FIELDs
        ///  of data rows in the GROUP referred to by the heading contents.
        /// </summary>
        private void Rule11c(AgsGroup group)
        {
            if (_errors.Contains("11a") == false || _errors.Contains("11b") == false)
                return;

            var rlColumns = group.ColumnsByType(AgsDataType.RL);

            if (rlColumns is null)
                return;

            AgsGroup tranGroup = _ags["TRAN"];
            string delimiter = tranGroup["TRAN_DLIM"][0].ToString();
            string concatenator = tranGroup["TRAN_RCON"][0].ToString();

            foreach (var rlColumn in rlColumns)
            {
                string colRef = rlColumn.Heading;

                foreach (var row in group.Rows)
                {
                    List<string> recordLinks = new();

                    if (row.Contains(colRef, concatenator))
                        recordLinks.AddRange(row[colRef].ToString().Split(concatenator));

                    else
                        recordLinks.Add(row[colRef].ToString());

                    for (int j = 0; j < recordLinks.Count; j++)
                    {
                        string linkedGroupName = recordLinks[j].Split(delimiter).First();

                        var linkedGroupRecords = _ags[linkedGroupName].ColumnsByStatus(AgsStatus.KEY).DelimitedKeyRows(delimiter);

                        if (row.Contains(colRef, delimiter) == false)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11c",
                                RuleId = 1130,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\", \"{delimiter}\" should be used as delimiter.",
                            });
                            continue;
                        }

                        int count = linkedGroupRecords.Count(l => l.Contains(row[colRef].ToString()[4..]));

                        if (count == 0)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11c",
                                RuleId = 1131,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\". No such record found.",
                            });
                            continue;
                        }

                        else if (count > 1)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11c",
                                RuleId = 1132,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\". Link refers to more than one record.",
                            });
                            continue;
                        }

                        var splitRecordLink = recordLinks[row.Index].Split(delimiter);
                        AgsGroup linkedGroup = _ags[splitRecordLink[0]];

                        if (linkedGroup is null)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11",
                                RuleId = 1133,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\". Link refers to group \"{splitRecordLink[0]}\" which was not found.",
                            });
                            continue;
                        }

                        var linkedGroupKeyColumns = linkedGroup.GetColumnsOfStatus(AgsStatus.KEY);

                        if (linkedGroupKeyColumns.Count() < splitRecordLink[1..].Length)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11",
                                RuleId = 1134,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\". Link reference has too many delimited values compared to the number of cross-reference group KEY fields.",
                            });
                        }

                        if (linkedGroupKeyColumns.Count() > splitRecordLink[1..].Length)
                        {
                            _errors.Add(new RuleError()
                            {
                                Status = "Fail",
                                RuleName = "11",
                                RuleId = 1135,
                                Group = group.Name,
                                RowNumber = row.Index,
                                Message = $"Invalid record link: \"{row[colRef]}\". Link reference has too few delimited values compared to the number of cross-reference group KEY fields.",
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The order in which the user - defined HEADINGs are listed in the DICT GROUP shall define the order in which these HEADINGS
        /// are appended to an existing GROUP or appear in a user-defined GROUP.
        /// This order also defines the sequence in which such HEADINGS are used in a heading of data TYPE 'Record Link'(Rule 11).
        /// </summary>
        private void Rule18a(AgsGroup group)
        {
            var dictHeadings = _ags["DICT"] is null ? Array.Empty<string>() : _ags["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AllOf("DICT_HDNG");
            var groupHeadings = group.Columns.Select(c => c.Heading).ToList();

            var intersectDictWithFile = dictHeadings.Intersect(groupHeadings).ToArray();
            var intersectFileWithDict = groupHeadings.Intersect(dictHeadings).ToArray();

            if (intersectDictWithFile.SequenceEqual(intersectFileWithDict))
                return;

            for (int i = 0; i < intersectDictWithFile.Length; i++)
            {
                if (intersectDictWithFile[i] == intersectFileWithDict[i])
                    continue;

                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "18a",
                    RuleId = 1810,
                    Group = group.Name,
                    RowNumber = group.HeadingRow,
                    Message = $"User defined headings not in order starting from {intersectFileWithDict[i]}. Expected order: ...{string.Join('|', intersectDictWithFile[i..])}",
                });
                return;
            }
        }

        /// <summary>
        /// A GROUP name shall not be more than 4 characters long and shall consist of uppercase letters and numbers only.
        /// </summary>
        private void Rule19(AgsGroup group)
        {
            if (group.Name.Length == 4 && group.Name.All(c => char.IsUpper(c)))
                return;

            _errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "19",
                RuleId = 1900,
                RowNumber = group.GroupRow,
                Group = group.Name,
                Message = "GROUP name should consist of four uppercase letters.",
            }
            );
        }

        /// <summary>
        /// A HEADING name shall not be more than 9 characters long and shall consist of uppercase letters, numbers 
        /// or the underscore character only.
        /// </summary>
        private void Rule19a(AgsGroup group)
        {
            var headings = group.Columns.Where(c => c.Heading != "HEADING").Select(c => c.Heading);

            if (headings.Any())
            {
                headings
                    .Where(r => _regexAgsHeadingField.IsMatch(r))
                    .ToList()
                    .ForEach(heading => _errors.Add(
                        new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "19a",
                            RuleId = 1910,
                            RowNumber = group.HeadingRow,
                            Group = group.Name,
                            Field = heading,
                            Message = $"HEADING {heading} should consist of only uppercase letters, numbers, and an underscore character.",
                        }));

                headings
                    .Where(r => r.Length > 9)
                    .ToList()
                    .ForEach(heading => _errors.Add(
                        new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "19a",
                            RuleId = 1911,
                            RowNumber = group.HeadingRow,
                            Group = group.Name,
                            Field = heading,
                            Message = $"HEADING {heading} is more than 9 characters in length.",
                        }));

                return;
            }

            _errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "19a",
                RuleId = 1912,
                RowNumber = group.HeadingRow,
                Group = group.Name,
                Message = "HEADING row does not have any fields.",
            }
            );
        }

        /// <summary>
        /// HEADING names shall start with the GROUP name followed by an underscore character.e.g. "NGRP_HED1"
        /// Where a HEADING refers to an existing HEADING within another GROUP, the HEADING name added to the group shall bear the same name. e.g.
        /// "CMPG_TESN" in the "CMPT" GROUP.
        /// </summary>
        private void Rule19b(AgsGroup group)
        {
            var headings = group.ReturnDescriptor(AgsDescriptor.HEADING);

            foreach (var heading in headings)
            {
                if (heading.Contains('_') == false)
                {
                    _errors.Add(
                        new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "19b",
                            RuleId = 1920,
                            RowNumber = group.HeadingRow,
                            Group = group.Name,
                            Field = heading,
                            Message = $"Heading {heading} should consist of group name and field name separated by an \"_\" underscore.",
                        });
                    continue;
                }

                string[] splitHeading = heading.Split('_');

                if (splitHeading[0].Length != 4 || splitHeading[1].Length > 4)
                {
                    _errors.Add(
                        new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "19b",
                            RuleId = 1921,
                            RowNumber = group.HeadingRow,
                            Group = group.Name,
                            Field = heading,
                            Message = $"Heading {heading} should consist of a 4 character group name and a field name of up to 4 characters.",
                        });
                }

                string[] exclusions = new string[] { "SPEC", "TEST" };

                if (exclusions.Contains(splitHeading[0]))
                    continue;

                var stdGroupDictRows = _stdDictionary["DICT"]["DICT_TYPE"].FilterRowsBy(AgsDescriptor.GROUP).AllOf("DICT_GRP");
                var rootGroup = _ags[splitHeading[0]];

                if (stdGroupDictRows.Contains(splitHeading[0]) == false && rootGroup is null)
                {
                    _errors.Add(
                        new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "19b",
                            RuleId = 1922,
                            RowNumber = group.HeadingRow,
                            Group = group.Name,
                            Field = heading,
                            Message = $"Group {splitHeading[0]} referred to in {heading} could not be found in either the standard dictionary or the DICT table.",
                        });
                }

                var stdDictHeadings = _stdDictionary["DICT"]?["DICT_HDNG"].Data;
                var fileDictHeadings = _ags["DICT"]?["DICT_HDNG"]?.Data;

                var allDictHeadings = (stdDictHeadings ?? Enumerable.Empty<dynamic>()).Concat(fileDictHeadings ?? Enumerable.Empty<dynamic>()).Distinct();

                if (allDictHeadings.Any(s => s.Contains(heading)) == false)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "19b",
                        RuleId = 1923,
                        RowNumber = group.HeadingRow,
                        Group = group.Name,
                        Field = heading,
                        Message = $"Definition for {heading} not found under group {splitHeading[0]}. Either rename heading or add definition under correct group.",
                    });
                }
            }
        }
    }
}

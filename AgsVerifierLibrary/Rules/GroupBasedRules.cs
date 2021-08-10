using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    public class GroupBasedRules
    {
        private static readonly Regex _regexAgsHeadingField = new(@"[^A-Z0-9_]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly string[] _parentGroupExceptions = new string[] { "PROJ", "TRAN", "ABBR", "DICT", "UNIT", "TYPE", "LOCA", "FILE", "LBSG", "PREM", "STND" };

        private readonly AgsContainer _ags;
        private readonly AgsContainer _stdDictionary;
        private readonly List<RuleError> _errors;

        public GroupBasedRules(AgsContainer ags, List<RuleError> errors, AgsContainer stdDictionary)
        {
            _ags = ags;
            _stdDictionary = stdDictionary;
            _errors = errors;
            CheckGroups();
        }

        private void CheckGroups()
        {
            Rule11(); // Covered by other rules
            Rule12(); // Covered by other rules

            Rule11a(); // TRAN group
            Rule11b(); // TRAN group
            Rule13(); // PROJ group
            Rule14(); // TRAN group
            Rule15(); // UNIT group
            Rule16(); // ABBR group
            //Rule16a(); // ABBR+TRAN group - Called by Rule 16
            Rule17(); // TYPE group
            Rule18(); // DICT group
            Rule20(); // FILE group

            foreach (var group in _ags.Groups)
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
            var fileDictTableFilteredByGroup = _ags["DICT"]["DICT_GRP"].FilterRowsBy(group.Name);

            var stdDictTableName = stdDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.GROUP);
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

            var stdDictTableHeadings = stdDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.HEADING).AllOf("DICT_HDNG");
            var fileDictTableHeadings = fileDictTableFilteredByGroup.AndBy("DICT_TYPE", AgsDescriptor.HEADING).AllOf("DICT_HDNG");

            var combinedDictTableHeadings = stdDictTableHeadings.Concat(fileDictTableHeadings).Distinct();

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

            foreach (var row in group.Rows)
            {
                string str = row.ToStringByStatus(AgsStatus.KEY);

                if (group.Rows.Count(r => r.ToStringByStatus(AgsStatus.KEY) == str) > 1)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "10a",
                        RuleId = 1011,
                        Group = group.Name,
                        RowNumber = (int)row["Index"],
                        Message = $"Duplicate key field combination: {str}",
                    });
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

            foreach (var row in group.Rows)
            {
                string str = row.ToStringByStatus(AgsStatus.REQUIRED);

                if (str.Contains("???"))
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "10b",
                        RuleId = 1021,
                        Group = group.Name,
                        RowNumber = (int)row["Index"],
                        Message = $"REQUIRED field(s) containing empty values: ...{str}",
                    });
                }
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
        ///  HEADINGs defined as a data TYPE of 'Record Link' (RL) can be used to link data rows to entries in GROUPs
        ///  outside of the defined hierarchy (Rule 10c) or DICT group for user defined GROUPs. The GROUP name followed 
        ///  by the KEY FIELDs defining the cross - referenced data row, in the order presented in the AGS4 DATA DICTIONARY.
        /// </summary>
        private void Rule11()
        {

        }

        /// <summary>
        /// Each GROUP/KEY FIELD shall be separated by a delimiter character. This single delimiter character shall 
        /// be defined in TRAN_DLIM. The default being "|" (ASCII character 124).
        /// </summary>
        private void Rule11a()
        {
            AgsGroup group = _ags["TRAN"];
            AgsColumn delimiterColumn = group["TRAN_DLIM"];

            if (delimiterColumn is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "11a",
                    RuleId = 1110,
                    Group = group.Name,
                    RowNumber = group.Rows[0].Index,
                    Message = $"TRAN_DLIM missing.",
                });
            }

            if (delimiterColumn.Data[0] == string.Empty)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "11a",
                    RuleId = 1111,
                    Group = group.Name,
                    RowNumber = group.Rows[0].Index,
                    Message = $"TRAN_DLIM is a null value.",
                });
            }
        }

        /// <summary>
        /// A heading of data TYPE 'Record Link' can refer to more than one combination of GROUP and KEY FIELDs. The 
        /// combination shall be separated by a defined concatenation character.This single concatenation character 
        /// shall be defined in TRAN_RCON.The default being "+" (ASCII character 43).
        /// </summary>
        private void Rule11b()
        {
            AgsGroup group = _ags["TRAN"];
            AgsColumn concatenatorColumn = group["TRAN_RCON"];

            if (concatenatorColumn is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "11b",
                    RuleId = 1120,
                    Group = group.Name,
                    RowNumber = group.Rows[0].Index,
                    Message = $"TRAN_RCON missing.",
                });
            }

            if (concatenatorColumn.Data[0] == string.Empty)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "11b",
                    RuleId = 1121,
                    Group = group.Name,
                    RowNumber = group.Rows[0].Index,
                    Message = $"TRAN_RCON is a null value.",
                });
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
        /// Data does not have to be included against each HEADING unless REQUIRED(Rule 10b). The data FIELD can be null;
        /// a null entry is defined as ""(two quotes together).
        /// </summary>
        private void Rule12()
        {

        }

        /// <summary>
        /// Each data file shall contain the PROJ GROUP which shall contain only one data row and, as a minimum, 
        /// shall contain data under the headings defined as REQUIRED (Rule 10b).
        /// </summary>
        private void Rule13()
        {
            AgsGroup projGroup = _ags["PROJ"];

            if (projGroup.RowCount == 1)
                return;

            else if (projGroup.RowCount == 0)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "13",
                    RuleId = 1300,
                    Group = projGroup.Name,
                    RowNumber = projGroup.GroupRow,
                    Message = "There should be at least one DATA row in the PROJ table.",
                });
            }

            else if (projGroup.RowCount > 0)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "13",
                    RuleId = 1301,
                    Group = projGroup.Name,
                    RowNumber = projGroup.GroupRow,
                    Message = "There should not be more than one DATA row in the PROJ table.",
                });
            }

            _errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "13",
                RuleId = 1302,
                Group = projGroup.Name,
                RowNumber = projGroup.Rows[0].Index,
                Message = "Each AGS data file shall contain the PROJ GROUP.",
            });
        }

        /// <summary>
        /// Each data file shall contain the TRAN GROUP which shall contain only one data row and, 
        /// as a minimum, shall contain data under the headings defined as REQUIRED (Rule 10b).
        /// </summary>
        private void Rule14()
        {
            AgsGroup tranGroup = _ags["TRAN"];

            if (tranGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "14",
                    RuleId = 1400,
                    Group = tranGroup.Name,
                    Message = "Each AGS data file shall contain the TRAN GROUP.",
                });
            }

            if (tranGroup.RowCount == 1)
                return;

            else if (tranGroup.RowCount == 0)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "14",
                    RuleId = 1401,
                    RowNumber = tranGroup.GroupRow,
                    Group = tranGroup.Name,
                    Message = "There should be at least one DATA row in the TRAN table.",
                });
            }

            else if (tranGroup.RowCount > 0)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "14",
                    RuleId = 1402,
                    RowNumber = tranGroup.Rows[0].Index,
                    Group = tranGroup.Name,
                    Message = "There should not be more than one DATA row in the TRAN table.",
                });
            }
        }

        /// <summary>
        ///  Each data file shall contain the UNIT GROUP to list all units used within the data file.
        ///  Every unit of measurement entered in the UNIT row of a GROUP or data entered in a FIELD where the field TYPE
        ///  is defined as "PU"(for example ELRG_RUNI, GCHM_UNIT or MOND_UNIT FIELDs) shall be listed and defined in the UNIT GROUP.
        /// </summary>
        private void Rule15()
        {
            AgsGroup unitGroup = _ags["UNIT"];

            if (unitGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "15",
                    RuleId = 1500,
                    Group = unitGroup.Name,
                    Message = "UNIT table not found.",
                });
            }

            var allGroupUnits = _ags.ReturnAllUnits().Distinct();
            var allPuTypeColumnData = _ags.GetAllColumnsOfType(AgsDataType.PU).MergeData().Distinct();
            var mergedUnits = allGroupUnits.Concat(allPuTypeColumnData);

            var unitUnits = unitGroup["UNIT_UNIT"].Data;

            var missingUnits = mergedUnits.Except(unitUnits);

            if (missingUnits.Any() == false)
                return;

            _errors.Add(new RuleError()
            {
                Status = "Fail",
                RuleName = "15",
                RuleId = 1501,
                RowNumber = unitGroup.GroupRow,
                Group = unitGroup.Name,
                Message = $"Unit(s) \"{string.Join('|', missingUnits)}\" not found in UNIT table.",
            });
        }
        // TODO should i check if ABBR RCON holding the value of the heading matches or else return an error???
        /// <summary>
        ///  Each data file shall contain the ABBR GROUP when abbreviations have been included in the data file.
        ///  The abbreviations listed in the ABBR GROUP shall include definitions for all abbreviations entered
        ///  in a FIELD where the data TYPE is defined as "PA" or any abbreviation needing definition used within
        ///  any other heading data type.
        /// </summary>
        private void Rule16()
        {
            var allPaTypeColumns = _ags.GetAllColumnsOfType(AgsDataType.PA);

            if (allPaTypeColumns is null)
                return;

            AgsGroup abbrGroup = _ags["ABBR"];

            if (abbrGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "16",
                    RuleId = 1600,
                    Group = "ABBR",
                    Message = $"PA field abbreviations identified, however ABBR table not found.",
                });
            }

            if (_errors.Contains(1120) || _errors.Contains(1121))
                return; 

            string concatenator = _ags["TRAN"]["TRAN_RCON"].Data[0];

            var abbrCodes = abbrGroup["ABBR_CODE"].Data.Distinct();

            foreach (var paTypeColumn in allPaTypeColumns)
            {
                foreach (var row in paTypeColumn.Group.Rows)
                {
                    if (row[paTypeColumn.Heading].ToString().Contains(concatenator))
                        Rule16a(paTypeColumn, row, concatenator, abbrCodes);

                    if (abbrCodes.Contains(row[paTypeColumn.Heading]))
                        continue;

                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "16",
                        RuleId = 1601,
                        Group = "ABBR",
                        Field = paTypeColumn.Heading,
                        RowNumber = abbrGroup.GroupRow,
                        Message = $"\"{row[paTypeColumn.Heading]}\" under {paTypeColumn.Heading} in {paTypeColumn.Group.Name} not found in ABBR table.",
                    });
                }
            }
        }

        /// <summary>
        ///  Where multiple abbreviations are required to fully codify a FIELD, the abbreviations shall be separated by a defined
        ///  concatenation character. This single concatenation character shall be defined in TRAN_RCON. The default being "+"
        ///  (ASCII character 43). Each abbreviation used in such combinations shall be listed separately in the ABBR GROUP.
        ///  e.g. "CP+RC" must have entries for both "CP" and "RC" in ABBR GROUP, together with their full definition.
        /// </summary>
        private void Rule16a(AgsColumn column, AgsRow row, string concatenator, IEnumerable<dynamic> abbrCodes)
        {
            var splitValues = row[column.Heading].ToString().Split(concatenator);

            foreach (var splitValue in splitValues)
            {
                if (abbrCodes.Contains(splitValue) == false)
                {
                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "16a",
                        RuleId = 1610,
                        Group = "ABBR",
                        Field = column.Heading,
                        RowNumber = row.Index,
                        Message = $"Concatenated field \"{row[column.Heading]}\" contains \"{splitValue}\" under {column.Heading} in {column.Group.Name} not found in ABBR table.",
                    });
                }
            }
        }

        /// <summary>
        /// Each data file shall contain the TYPE GROUP to define the field TYPEs used within the data file.
        /// Every data type entered in the TYPE row of a GROUP shall be listed and defined in the TYPE GROUP.
        /// </summary>
        private void Rule17()
        {
            AgsGroup typeGroup = _ags["TYPE"];

            if (typeGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "17",
                    RuleId = 1700,
                    Group = "TYPE",
                    Message = "TYPE table not found.",
                });
                return;
            }

            var allTypesInFile = _ags.ReturnAllTypes().Distinct();

            foreach (var typeColumn in allTypesInFile)
            {
                if (_ags["TYPE"]["TYPE_TYPE"].Data.Contains(typeColumn) || typeColumn == "TYPE")
                    continue;

                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "17",
                    RuleId = 1701,
                    Group = "TYPE",
                    RowNumber = typeGroup.GroupRow,
                    Message = $"Data type {typeColumn} not found in TYPE table.",
                });
            }
        }

        /// <summary>
        /// Each data file shall contain the DICT GROUP where non-standard GROUP and HEADING names have been 
        /// included in the data file.
        /// </summary>
        private void Rule18()
        {
            if (_ags["DICT"] is null)
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

        /// <summary>
        /// The order in which the user - defined HEADINGs are listed in the DICT GROUP shall define the order in which these HEADINGS
        /// are appended to an existing GROUP or appear in a user-defined GROUP.
        /// This order also defines the sequence in which such HEADINGS are used in a heading of data TYPE 'Record Link'(Rule 11).
        /// </summary>
        private void Rule18a(AgsGroup group)
        {
            var dictHeadings = _ags["DICT"]["DICT_GRP"].FilterRowsBy(group.Name).AllOf("DICT_HDNG");
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

        // TODO edit so for loop with i uses rows... maybe getgroups with headings ...
        /// <summary>
        /// Additional computer files (e.g. digital images) can be included within a data submission. Each such file shall be defined in a FILE GROUP.
        /// The additional files shall be transferred in a sub-folder named FILE. This FILE sub - folder shall contain additional sub-folders each
        /// named by the FILE_FSET reference. Each FILE_FSET named folder will contain the files listed in the FILE GROUP.
        /// </summary>
        private void Rule20()
        {
            AgsGroup fileGroup = _ags["FILE"];

            var fSetColumns = _ags.GetAllColumnsOfHeading("FILE_FSET", "FILE");

            var anyFsetEntries = fSetColumns.Any(c => c.AllNull == false);

            if (fileGroup is null && anyFsetEntries)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "20",
                    RuleId = 2000,
                    Group = "FILE",
                    Message = $"FILE table not found even though there are FILE_FSET entries in other tables.",
                });
                return;
            }
            else if (fileGroup is null)
                return;

            foreach (var fSetColumn in fSetColumns)
            {
                for (int i = 0; i < fSetColumn.Data.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(fSetColumn.Data[i]))
                        continue;

                    var fileGroupFSetEntries = fileGroup["FILE_FSET"].Data;

                    if (fileGroupFSetEntries.Contains(fSetColumn.Data[i]))
                        continue;

                    _errors.Add(new RuleError()
                    {
                        Status = "Fail",
                        RuleName = "20",
                        RuleId = 2001,
                        Group = fSetColumn.Group.Name,
                        RowNumber = _ags[fSetColumn.Group.Name].Rows[i].Index,
                        Message = $"FILE_FSET entry \"{fSetColumn.Data[i]}\" not found in FILE table.",
                    });
                }
            }

            string baseDir = Path.GetDirectoryName(_ags.FilePath);
            string fileFolderPath = Path.Combine(baseDir, "FILE");

            if (anyFsetEntries && Directory.Exists(Path.Combine(baseDir, "FILE")) == false)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "20",
                    RuleId = 2002,
                    Group = "FILE",
                    Message = $"Folder named \"FILE\" not found. Files defined in the FILE table should be saved in this folder.",
                });
                return;
            }

            var fileGroupFolderNames = fileGroup["FILE_FSET"].ReturnDataDistinctNonBlank();
            var subFolders = Directory.GetDirectories(fileFolderPath).Select(i => Path.GetFileName(i));

            fileGroupFolderNames.Except(subFolders).ToList().ForEach(missingSubFolder =>
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "20",
                    RuleId = 2003,
                    Group = "FILE",
                    Message = $"Sub-folder named \"{Path.Combine("FILE", missingSubFolder)}\" not found even though it is defined in the FILE table.",
                });
            });

            var fileGroupFileNames = fileGroup["FILE_NAME"].ReturnDataDistinctNonBlank();
            var subFolderFiles = Directory.GetDirectories(fileFolderPath).SelectMany(d => Directory.GetFiles(d).Select(f => Path.GetFileName(f)));

            var missingFilesMask = fileGroupFileNames.Except(subFolderFiles);

            var missingSubFiles = fileGroup.Filter("FILE_NAME", missingFilesMask);

            var test = fileGroup["FILE_NAME"].FilterRowsBy("EM-4m3_ML052");

            foreach (var missingSubFile in missingSubFiles)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "20",
                    RuleId = 2004,
                    Group = "FILE",
                    Message = $"File named \"{Path.Combine("FILE", missingSubFile["FILE_FSET"].ToString(), missingSubFile["FILE_NAME"].ToString())}\" not found even though it is defined in the FILE table.",
                });
            };
        }
    }
}

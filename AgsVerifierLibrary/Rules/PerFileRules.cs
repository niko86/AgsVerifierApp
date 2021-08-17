using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgsVerifierLibrary.Rules
{
    public class PerFileRules
    {
        private readonly AgsContainer _ags;
        private readonly AgsContainer _stdDictionary;
        private readonly List<RuleError> _errors;

        public PerFileRules(AgsContainer ags, List<RuleError> errors, AgsContainer stdDictionary)
        {
            _ags = ags;
            _stdDictionary = stdDictionary;
            _errors = errors;
        }

        public void Process()
        {
            Rule8();
            Rule11a(); // TRAN group
            Rule11b(); // TRAN group
            Rule13(); // PROJ group
            Rule14(); // TRAN group
            Rule15(); // UNIT group
            Rule16(); // ABBR group
            Rule17(); // TYPE group
            Rule18(); // DICT group
            Rule20(); // FILE group

            // Rule11(); Covered by other rules
            // Rule12(); Covered by other rules
            //Rule16a(); Called by Rule 16
        }

        // TODO - Add checks on U, DT and T.
        /// <summary>
        ///  Data VARIABLEs shall be presented in the units of measurement and type that are described by 
        ///  the appropriate data field UNIT and data field TYPE defined at the start of the GROUP within 
        ///  the GROUP HEADER rows.
        /// </summary>
        private void Rule8()
        {
            var columns = _ags.GetAllColumnsOfFixedNumericalType();

            foreach (var column in columns)
            {
                for (int i = 0; i < column.Data.Count; i++)
                {
                    if (NumericChecks.NumericTypeIsValid(column.Type, column.Data[i]) == false)
                    {
                        _errors.Add(new RuleError()
                        {
                            Status = "Fail",
                            RuleName = "8",
                            RuleId = 800,
                            Group = column.Group.Name,
                            RowNumber = column.Row(i).Index,
                            Message = $"\"{column.Data[i]}\" under {column.Heading} in {column.Group.Name} table does not match the defined TYPE \"{column.Type}\".",
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

            if (projGroup is null)
            {
                _errors.Add(new RuleError()
                {
                    Status = "Fail",
                    RuleName = "13",
                    RuleId = 1302,
                    Group = projGroup.Name,
                    RowNumber = projGroup.Rows[0].Index,
                    Message = "Each AGS data file shall contain the PROJ GROUP.",
                });
                return;
            }

            else if (projGroup.RowCount < 1)
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

            else if (projGroup.RowCount > 1)
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
                return;
            }

            else if (tranGroup.RowCount < 1)
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

            else if (tranGroup.RowCount > 1)
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

            if (missingUnits.Any())
            {
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

            string[] exclusion = new string[] { string.Empty, null, "HEADING" };

            foreach (var paTypeColumn in allPaTypeColumns)
            {
                var paTypeRows = paTypeColumn.Group.Rows.GroupBy(r => r[paTypeColumn.Heading]);

                var paTypeNotInAbbrGroups = paTypeRows.Where(k => abbrCodes.Contains(k.Key) == false && exclusion.Contains(k.Key) == false);

                if (paTypeNotInAbbrGroups.Any())
                {
                    foreach (var paTypeNotInAbbrGroup in paTypeNotInAbbrGroups)
                    {
                        foreach (var row in paTypeNotInAbbrGroup)
                        {
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

                var paTypeConcatGroups = paTypeRows.Where(k => k.Key.ToString().Contains(concatenator));

                if (paTypeConcatGroups.Any())
                {
                    foreach (var paTypeConcatGroup in paTypeConcatGroups)
                    {
                        var splitValues = paTypeConcatGroup.Key.ToString().Split(concatenator);

                        foreach (var splitValue in splitValues)
                        {
                            if (abbrCodes.Contains(splitValue) == false)
                            {
                                foreach (var row in paTypeConcatGroup)
                                {
                                    _errors.Add(new RuleError()
                                    {
                                        Status = "Fail",
                                        RuleName = "16a",
                                        RuleId = 1610,
                                        Group = "ABBR",
                                        Field = paTypeColumn.Heading,
                                        RowNumber = row.Index,
                                        Message = $"Concatenated field \"{row[paTypeColumn.Heading]}\" contains \"{splitValue}\" under {paTypeColumn.Heading} in {paTypeColumn.Group.Name} not found in ABBR table.",
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        // TODO implement check for XN data type see page 7 in AGS 4.1 standard.
        /// <summary>
        ///  Where multiple abbreviations are required to fully codify a FIELD, the abbreviations shall be separated by a defined
        ///  concatenation character. This single concatenation character shall be defined in TRAN_RCON. The default being "+"
        ///  (ASCII character 43). Each abbreviation used in such combinations shall be listed separately in the ABBR GROUP.
        ///  e.g. "CP+RC" must have entries for both "CP" and "RC" in ABBR GROUP, together with their full definition.
        /// </summary>
        private void Rule16a()
        {

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

            foreach (string typeColumn in allTypesInFile)
            {
                if (typeGroup["TYPE_TYPE"].Contains(typeColumn))
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

        // TODO edit so for loop with i uses rows... maybe getgroups with headings ...
        /// <summary>
        /// Additional computer files (e.g. digital images) can be included within a data submission. Each such file shall be defined in a FILE GROUP.
        /// The additional files shall be transferred in a sub-folder named FILE. This FILE sub - folder shall contain additional sub-folders each
        /// named by the FILE_FSET reference. Each FILE_FSET named folder will contain the files listed in the FILE GROUP.
        /// </summary>
        private void Rule20()
        {
            AgsGroup fileGroup = _ags["FILE"];

            var fSetColumns = _ags.GetAllColumnsOfHeading("FILE_FSET", fileGroup);

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
            else if (fileGroup is null) // Necessary to break out if File table not present but above isnt true
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

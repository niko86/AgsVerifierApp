using AgsVerifierLibrary.Actions;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Rules
{
    public class GroupBasedRules
    {
		private static readonly Regex _regexAgsHeadingField = new(@"[^A-Z0-9_]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly string[] _parentGroupExceptions = new string[] { "PROJ", "TRAN", "ABBR", "DICT", "UNIT", "TYPE", "LOCA", "FILE", "LBSG", "PREM", "STND" };

        private readonly List<AgsGroupModel> _groups;
        private readonly List<AgsGroupModel> _stdDictionary;
        private readonly List<RuleErrorModel> _errors;
        private readonly string _filePath;

        public GroupBasedRules(List<AgsGroupModel> groups, List<AgsGroupModel> stdDictionary, List<RuleErrorModel> errors, string filePath)
        {
			_groups = groups;
            _stdDictionary = stdDictionary;
            _errors = errors;
            _filePath = filePath;
        }

		public void CheckGroups()
		{
			Rule11(); // Covered by other rules
			Rule12(); // Covered by other rules

			Rule11a(); // TRAN group
			Rule11b(); // TRAN group
			Rule13(); // PROJ group
			Rule14(); // TRAN group
			Rule15(); // UNIT group
			Rule16(); // ABBR group
			Rule16a(); // ABBR+TRAN group
			Rule17(); // TYPE group
			Rule18(); // DICT group
			Rule20(); // FILE group

			foreach (var group in _groups)
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

		private void Rule2(AgsGroupModel group)
		{
            if (group.GetColumn("HEADING").Data.Any())
				return;

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "2",
				Group = group.Name,
				Message = $"No DATA rows in the {group.Name} table.",
			});
		}

		private void Rule2b(AgsGroupModel group)
		{
			List<string> descriptors = new() { "HEADING", "UNIT", "TYPE" };

			foreach (var descriptor in descriptors)
			{
				if (group.GetGroupDescriptorRowNumber((Descriptor)Enum.Parse(typeof(Descriptor), descriptor)) == 0)
                {
					_errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "2b",
						Group = group.Name,
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
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "2b",
					Group = group.Name,
					RowNumber = group.UnitRow,
					Message = $"UNIT row is misplaced. It should be immediately below the HEADING row.",
				});
			}

			if (orderTestC || orderTestD)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "2b",
					Group = group.Name,
					RowNumber = group.TypeRow,
					Message = $"TYPE row is misplaced. It should be immediately below the UNIT row.",
				});
			}
		}

		private void Rule7(AgsGroupModel group)
		{
			// The order of data FIELDs in each line within a GROUP is defined at the start of each GROUP in the HEADING row.
			// HEADINGs shall be in the order described in the AGS FORMAT DATA DICTIONARY.
			var dictHeadings = _stdDictionary.GetGroup("DICT").GetRowsByFilter("DICT_GRP", group.Name).AndBy("DICT_TYPE", Descriptor.HEADING.ToString()).ReturnAllValuesOf("DICT_HDNG");
			var groupHeadings = group.Columns.Select(c => c.Heading);

			var intersectDictWithFile = dictHeadings.Intersect(groupHeadings).ToArray();
			var intersectFileWithDict = groupHeadings.Intersect(dictHeadings).ToArray();

			if (intersectDictWithFile.SequenceEqual(intersectFileWithDict))
				return;

			for (int i = 0; i < intersectDictWithFile.Length; i++)
			{
				if (intersectDictWithFile[i] == intersectFileWithDict[i])
					continue;

				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "7",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Headings not in order starting from {intersectFileWithDict[i]}. Expected order: ...{string.Join('|', intersectDictWithFile[i..])}",
				});
				return;
			}
		}

		private void Rule9(AgsGroupModel group)
		{
			// Data HEADING and GROUP names shall be taken from the AGS FORMAT DATA DICTIONARY. In cases where there is no suitable entry,
			// a user-defined GROUP and/or HEADING may be used in accordance with Rule 18. Any user-defined HEADINGs shall be included at
			// the end of the HEADING row after the standard HEADINGs in the order defined in the DICT group (see Rule 18a).

			var stdDictTableFilteredByGroup = _stdDictionary.GetGroup("DICT").GetRowsByFilter("DICT_GRP", group.Name);

			var fileDictTableFilteredByGroup = _groups.GetGroup("DICT").GetRowsByFilter("DICT_GRP", group.Name);

            if (stdDictTableFilteredByGroup is null && fileDictTableFilteredByGroup is null)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "9",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"GROUP name not defined within either the AGS FORMAT DATA DICTIONARY or DICT table.",
				});
				return;
			}

			var stdDictTableHeadingRows = stdDictTableFilteredByGroup.AndBy("DICT_TYPE", Descriptor.HEADING).ReturnAllValuesOf("DICT_HDNG");
			var fileDictTableHeadingRows = fileDictTableFilteredByGroup.AndBy("DICT_TYPE", Descriptor.HEADING).ReturnAllValuesOf("DICT_HDNG");
			var combinedDictTableHeadingRows = stdDictTableHeadingRows.Concat(fileDictTableHeadingRows).Distinct();

			var groupHeadings = group.Columns.Select(c => c.Heading).ToList();

			var intersectDictWithFile = combinedDictTableHeadingRows.Intersect(groupHeadings).ToArray();
			var intersectFileWithDict = groupHeadings.Intersect(combinedDictTableHeadingRows).ToArray();

			if (intersectDictWithFile.SequenceEqual(intersectFileWithDict))
				return;

			for (int i = 0; i < intersectDictWithFile.Length; i++)
			{
				if (intersectDictWithFile[i] == intersectFileWithDict[i])
					continue;

				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "9",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Headings not in order starting from {intersectFileWithDict[i]}. Expected order: ...{string.Join('|', intersectDictWithFile[i..])}",
				});
				return;
			}
		}

		private void Rule10a(AgsGroupModel group)
		{
			// Add code to add statuses to custom fields using local dict
			var keyHeadings = group.Columns.ByStatus(Status.KEY).ReturnDescriptor(Descriptor.HEADING);

			var groupHeadings = group.Columns.ReturnDescriptor(Descriptor.HEADING);

			List<string> duplicateHeadings = new();

            foreach (var keyHeading in keyHeadings)
            {
				if (groupHeadings.Any(i => i == keyHeading) == false) 
				{
					_errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "10a",
						Group = group.Name,
						RowNumber = group.HeadingRow,
						Message = $"KEY field {keyHeading} not found.",
					});
				}

                else if (groupHeadings.Count(i => i == keyHeading) > 1)
                {
					duplicateHeadings.Add(keyHeading);
				}
            }

			if (duplicateHeadings.Count > 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10a",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Duplicate KEY field combination: ...{string.Join('|', duplicateHeadings)}",
				});
			}
		}

		private void Rule10b(AgsGroupModel group)
		{
			// Some HEADINGs are marked as REQUIRED.REQUIRED fields must appear in the data GROUPs where they are indicated in the AGS FORMAT DATA DICTIONARY.
			// These fields require data entry and cannot be null(i.e.left blank or empty).

			var requiredHeadings = group.Columns.ByStatus(Status.REQUIRED).ReturnDescriptor(Descriptor.HEADING);

			var groupHeadings = group.Columns.ReturnDescriptor(Descriptor.HEADING);

			List<string> requiredHeadingsWithBlanks = new();

			foreach (var requiredHeading in requiredHeadings)
			{
				if (groupHeadings.Any(i => i == requiredHeading) == false)
				{
					_errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "10b",
						Group = group.Name,
						RowNumber = group.HeadingRow,
						Message = $"REQUIRED field {requiredHeading} not found.",
					});
				}

				else if (group.GetColumn(requiredHeading).Data.Any(i => string.IsNullOrWhiteSpace(i)))
				{
					requiredHeadingsWithBlanks.Add(requiredHeading);
				}
			}

			if (requiredHeadingsWithBlanks.Count > 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10b",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"REQUIRED field(s) containing empty values: ...{string.Join('|', requiredHeadingsWithBlanks)}",
				});
			}
		}
		//todo
		private void Rule10c(AgsGroupModel group)
		{
			// Links are made between data rows in GROUPs by the KEY fields.
			// Every entry made in the KEY fields in any GROUP must have an equivalent entry in its PARENT GROUP.
			// The PARENT GROUP must be included within the data file.

			if (_parentGroupExceptions.Contains(group.Name))
				return;

			string parentGroupName = group.ParentGroup;

			if (parentGroupName == string.Empty)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10c",
					Group = group.Name,
					Message = $"Parent group left blank in dictionary.",
				});
				return;
			}

			if (_groups.ReturnGroupNames().Contains(parentGroupName) == false)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10c",
					Group = group.Name,
					Message = $"Could not find parent group {parentGroupName}.",
				});
				return;
			}

			var parentDictKeyHeadings = HelperFunctions.MergedDictColumnByStatus(_stdDictionary, _groups, "key", group.Name, "DICT_HDNG");

			if (parentDictKeyHeadings.Any() == false)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10c",
					Group = group.Name,
					Message = $"Could not check parent entries since group definitions not found in standard dictionary or DICT table.",
				});
				return;
			}

			var childDictKeyHeadings = HelperFunctions.MergedDictColumnByStatus(_stdDictionary, _groups, "key", group.Name, "DICT_HDNG");

            foreach (var childKeyHeading in childDictKeyHeadings)
            {
				
			}

			// SequenceEqual to compare two lists sequencing.

			AgsGroupModel parentGroup = _groups.GetGroup(parentGroupName);

			// TODO bug check as false positives coming through on LBST and SHBT
			//Rule10c_missingKeys(parentGroup, mergedDictKeyHeadings, true);
			//Rule10c_missingKeys(group, mergedDictKeyHeadings, false);

			// TODO THIS NEEDS TO BE FOR DATA IN THE GROUP MUST HAVE ENTRY IN PARENT GROUP!!!!!!!!
			// NEED FOR LOOP TO GO THROUGH CHILD Data for each column AND CHECK IF in parent data column 
		}

		private void Rule10c_missingKeys(AgsGroupModel group, List<string> keyHeadings, bool parent)
        {
			var groupKeyHeadings = group.Columns.ByStatus(Status.KEY).ReturnDescriptor(Descriptor.HEADING);

			var missingKeyHeadings = keyHeadings.Except(groupKeyHeadings);

			string parentChild = parent ? "parent" : "child";

			if (missingKeyHeadings.Any())
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "10c",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Could not check KEY field entries due to missing key fields in {parentChild} group {group.Name}. Check error log under Rule 10a.",
				});
			}
		}

		private void Rule11()
		{
			//  HEADINGs defined as a data TYPE of 'Record Link' (RL) can be used to link data rows to entries in GROUPs
			//  outside of the defined hierarchy (Rule 10c) or DICT group for user defined GROUPs.
			//  The GROUP name followed by the KEY FIELDs defining the cross - referenced data row, in the order presented in the AGS4 DATA DICTIONARY.
		}

		private void Rule11a()
		{
			AgsGroupModel group = _groups.GetGroup("TRAN");
			AgsColumnModel delimiterColumn = group.GetColumn("TRAN_DLIM");

			if (delimiterColumn is null)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "11a",
					Group = group.Name,
					RowNumber = group.FirstDataRow,
					Message = $"TRAN_DLIM missing.",
				});
			}

			if (delimiterColumn.Data[0] == string.Empty)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "11a",
					Group = group.Name,
					RowNumber = group.FirstDataRow,
					Message = $"TRAN_DLIM is a null value.",
				});
			}
		}

		private void Rule11b()
		{
			AgsGroupModel group = _groups.GetGroup("TRAN");
			AgsColumnModel concatenatorColumn = group.GetColumn("TRAN_RCON");

			if (concatenatorColumn is null)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "11b",
					Group = group.Name,
					RowNumber = group.FirstDataRow,
					Message = $"TRAN_RCON missing.",
				});
			}

			if (concatenatorColumn.Data[0] == string.Empty)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "11b",
					Group = group.Name,
					RowNumber = group.FirstDataRow,
					Message = $"TRAN_RCON is a null value.",
				});
			}
		}

		private void Rule11c(AgsGroupModel group)
		{
			//  Any heading of data TYPE 'Record Link' included in a data file shall cross-reference to the KEY FIELDs
			//  of data rows in the GROUP referred to by the heading contents.

			var errorIds = _errors.Select(e => e.RuleId);

			if (errorIds.Contains("11a") == false || errorIds.Contains("11b") == false)
				return;

			var rlColumns = group.Columns.ByType(DataType.RL);

			if (rlColumns is null)
				return;

			AgsGroupModel tranGroup = _groups.GetGroup("TRAN");
			string delimiter = tranGroup.GetColumn("TRAN_DLIM").Data.FirstOrDefault();
			string concatenator = tranGroup.GetColumn("TRAN_RCON").Data.FirstOrDefault();

			foreach (var rlColumn in rlColumns)
            {
				for (int i = 0; i < rlColumn.Data.Count; i++)
				{
					List<string> recordLinks = new();

					if (rlColumn.Data[i].Contains(concatenator))
						recordLinks.AddRange(rlColumn.Data[i].Split(concatenator));

					else
						recordLinks.Add(rlColumn.Data[i]);

					for (int j = 0; j < recordLinks.Count; j++)
					{
						string linkedGroupName = recordLinks[j].Split(delimiter).First();

						var linkedGroupRecords = _groups.GetGroup(linkedGroupName).Columns.ByStatus(Status.KEY).OrderBy(i => i.Index).ReturnRows(delimiter);


						if (rlColumn.Data[i].Contains(delimiter) == false)
						{
							_errors.Add(new RuleErrorModel()
							{
								Status = "Fail",
								RuleId = "11c",
								Group = group.Name,
								RowNumber = group.FirstDataRow,
								Message = $"Invalid record link: \"{rlColumn.Data[i]}\", \"{delimiter}\" should be used as delimiter.",
							});
							return;
						}

						int count = linkedGroupRecords.Count(l => l.Contains(rlColumn.Data[i][4..]));

						if (count == 0)
						{
							_errors.Add(new RuleErrorModel()
							{
								Status = "Fail",
								RuleId = "11c",
								Group = group.Name,
								RowNumber = group.FirstDataRow + i,
								Message = $"Invalid record link: \"{rlColumn.Data[i]}\". No such record found.",
							});
							return;
						}

						else if (count > 1)
						{
							_errors.Add(new RuleErrorModel()
							{
								Status = "Fail",
								RuleId = "11c",
								Group = group.Name,
								RowNumber = group.FirstDataRow + i,
								Message = $"Invalid record link: \"{rlColumn.Data[i]}\". Link refers to more than one record.",
							});
							return;
						}
					}
				}
			}
        }

        private void Rule12()
		{

		}

		private void Rule13()
		{
			AgsGroupModel projGroup = _groups.GetGroup("PROJ");

			if (projGroup.DataFrame.Rows.Count == 1)
				return;

			else if (projGroup.DataFrame.Rows.Count == 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "13",
					RowNumber = projGroup.GroupRow,
					Group = projGroup.Name,
					Message = "There should be at least one DATA row in the PROJ table.",
				});
			}

			else if (projGroup.DataFrame.Rows.Count > 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "13",
					RowNumber = projGroup.GroupRow,
					Group = projGroup.Name,
					Message = "There should not be more than one DATA row in the PROJ table.",
				});
			}

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "13",
				RowNumber = projGroup.FirstDataRow,
				Group = projGroup.Name,
				Message = "Each AGS data file shall contain the PROJ GROUP.",
			});
		}
		
		private void Rule14()
		{
			AgsGroupModel tranGroup = _groups.GetGroup("TRAN");

            if (tranGroup is null)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "14",
					RowNumber = tranGroup.FirstDataRow,
					Group = tranGroup.Name,
					Message = "Each AGS data file shall contain the TRAN GROUP.",
				});
			}

			if (tranGroup.DataFrame.Rows.Count == 1)
				return;

			else if (tranGroup.DataFrame.Rows.Count == 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "14",
					RowNumber = tranGroup.GroupRow,
					Group = tranGroup.Name,
					Message = "There should be at least one DATA row in the TRAN table.",
				});
			}

			else if (tranGroup.DataFrame.Rows.Count > 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "14",
					RowNumber = tranGroup.FirstDataRow,
					Group = tranGroup.Name,
					Message = "There should not be more than one DATA row in the TRAN table.",
				});
			}
		}

		private void Rule15()
		{
			//  Each data file shall contain the UNIT GROUP to list all units used within the data file.
			//  Every unit of measurement entered in the UNIT row of a GROUP or data entered in a FIELD where the field TYPE
			//  is defined as "PU"(for example ELRG_RUNI, GCHM_UNIT or MOND_UNIT FIELDs) shall be listed and defined in the UNIT GROUP.

			AgsGroupModel unitGroup = _groups.GetGroup("UNIT");

			if (unitGroup is null)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "15",
					Group = unitGroup.Name,
					Message = "UNIT table not found.",
				});
			}

			var allGroupUnits = _groups.ReturnAllUnits().Distinct();
			var allPuTypeColumnData = _groups.GetAllColumnsOfType(DataType.PU).MergeData().Distinct();
			var mergedUnits = allGroupUnits.Concat(allPuTypeColumnData);
			
			var unitUnits = unitGroup.GetColumn("UNIT_UNIT").Data;

			var missingUnits = mergedUnits.Except(unitUnits);

			if (missingUnits.Any() == false)
				return;

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "15",
				RowNumber = unitGroup.GroupRow,
				Group = unitGroup.Name,
				Message = $"Unit(s) \"{string.Join('|', missingUnits)}\" not found in UNIT table.", 
			});
		}
		// TODO
		private void Rule16()
		{
			//  Each data file shall contain the ABBR GROUP when abbreviations have been included in the data file.
			//  The abbreviations listed in the ABBR GROUP shall include definitions for all abbreviations entered
			//  in a FIELD where the data TYPE is defined as "PA" or any abbreviation needing definition used within
			//  any other heading data type.

			var allPaTypeColumns = _groups.GetAllColumnsOfType(DataType.PA);

			if (allPaTypeColumns is null)
				return;

			AgsGroupModel abbrGroup = _groups.GetGroup("ABBR");

            if (abbrGroup is null)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "16",
					Group = "ABBR",
					Message = $"PA field abbreviations identified, however ABBR table not found.",
				});
			}

			var abbrCodes = abbrGroup.GetColumn("ABBR_CODE").Data.Distinct().ToList();

			// TODO fix the loop
            foreach (var paTypeColumn in allPaTypeColumns)
            {
				for (int i = 0; i < abbrCodes.Count; i++)
				{
					if (paTypeColumn.Data.Distinct().Contains(abbrCodes[i]) == false)
					{
						_errors.Add(new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "16",
							Group = "ABBR",
							RowNumber = abbrGroup.FirstDataRow + i,
							Message = $"\"{abbrCodes[i]}\" under {paTypeColumn.Heading} in {paTypeColumn.Group} not found in ABBR table",
						});
					}
				}
			}
		}

		private void Rule16a()
        {
			//  Where multiple abbreviations are required to fully codify a FIELD, the abbreviations shall be separated by a defined
			//  concatenation character. This single concatenation character shall be defined in TRAN_RCON. The default being "+"
			//  (ASCII character 43). Each abbreviation used in such combinations shall be listed separately in the ABBR GROUP.
			//  e.g. "CP+RC" must have entries for both "CP" and "RC" in ABBR GROUP, together with their full definition.

			var raisedErrorIds = _errors.Select(e => e.RuleId);

			if (raisedErrorIds.Contains("14"))
				return; // No TRAN table no way to concatenator. Or would

			AgsGroupModel tranGroup = _groups.GetGroup("TRAN");


			AgsColumnModel tranRconColumn = tranGroup.GetColumn("TRAN_RCON");


		}

		private void Rule17()
		{
			//Each data file shall contain the TYPE GROUP to define the field TYPEs used within the data file.
			//Every data type entered in the TYPE row of a GROUP shall be listed and defined in the TYPE GROUP.
			AgsGroupModel typeGroup = _groups.GetGroup("TYPE");

			if (typeGroup is null)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "17",
					Group = "TYPE",
					Message = "TYPE table not found.",
				});
				return;
			}

			var typeColumns = _groups.Select(g => g.Columns.Select(t => t.Type)).SelectMany(i => i).Distinct();

            foreach (var typeColumn in typeColumns)
            {
				if (_groups.GetGroup("TYPE").GetColumn("TYPE_TYPE").Data.Contains(typeColumn) || typeColumn == "TYPE")
					continue;

				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "17",
					Group = "TYPE",
					Message = $"Data type {typeColumn} not found in TYPE table.",
				});
			}
		}

		private void Rule18()
		{
			if (_groups.GetGroup("DICT") is null)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "18",
					Group = "DICT",
					Message = "DICT table not found. See error log under Rule 9 for a list of non-standard headings that need to be defined in a DICT table.",
				});
			}
		}
		//TODO
		private void Rule18a(AgsGroupModel group)
        {
			// MODIFIED FROM RULE 7
			// The order in which the user - defined HEADINGs are listed in the DICT GROUP shall define the order in which these HEADINGS
			// are appended to an existing GROUP or appear in a user-defined GROUP.
			// This order also defines the sequence in which such HEADINGS are used in a heading of data TYPE 'Record Link'(Rule 11).

			var dictHeadings = _groups.GetGroup("DICT").DataFrame.FilterColumnToList("DICT_GRP", group.Name, "DICT_HDNG");
			var groupHeadings = group.Columns.Select(c => c.Heading).ToList();

			var intersectDictWithFile = dictHeadings.Intersect(groupHeadings).ToArray();
			var intersectFileWithDict = groupHeadings.Intersect(dictHeadings).ToArray();

			if (intersectDictWithFile.SequenceEqual(intersectFileWithDict))
				return;

			for (int i = 0; i < intersectDictWithFile.Length; i++)
			{
				if (intersectDictWithFile[i] == intersectFileWithDict[i])
					continue;
				//TODO handle 'Record Link'
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "18a",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Headings not in order starting from {intersectFileWithDict[i]}. Expected order: ...{string.Join('|', intersectDictWithFile[i..])}",
				});
				return;
			}
		}

		private void Rule19(AgsGroupModel group)
		{
			if (group.Name.Length == 4 && group.Name.All(c => char.IsUpper(c)))
				return;

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "19",
				RowNumber = group.GroupRow,
				Group = group.Name,
				Message = "GROUP name should consist of four uppercase letters.",
			}
			);
		}

		private void Rule19a(AgsGroupModel group)
		{
			var headings = group.Columns.Where(c => c.Heading != "HEADING").Select(c => c.Heading);

			if (headings.Any())
			{
				headings
					.Where(r => _regexAgsHeadingField.IsMatch(r))
					.ToList()
					.ForEach(heading => _errors.Add(
						new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "19a",
							RowNumber = group.HeadingRow,
							Group = group.Name,
							Field = heading,
							Message = $"HEADING {heading} should consist of only uppercase letters, numbers, and an underscore character.",
						})
						);

				headings
					.Where(r => r.Length > 9)
					.ToList()
					.ForEach(heading => _errors.Add(
						new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "19a",
							RowNumber = group.HeadingRow,
							Group = group.Name,
							Field = heading,
							Message = $"HEADING {heading} is more than 9 characters in length.",
						})
						);

				return;
			}

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "19a",
				RowNumber = group.HeadingRow,
				Group = group.Name,
				Message = "HEADING row does not have any fields.",
			}
			);
		}
		// TODO
		private void Rule19b(AgsGroupModel group)
		{
			
			//elif heading not in ref_headings_list_1 and heading in ref_headings_list_2:
			//msg = f'Definition for {heading} not found under group {ref_group_name}. Either rename heading or add definition under correct group.'
			//		line_number = line_numbers[group]['HEADING']
			//		add_error_msg(ags_errors, 'Rule 19b', line_number, group, msg)

			var headings = group.Columns.Where(c => c.Heading != "HEADING").Select(c => c.Heading);

			foreach (var heading in headings)
			{
				if (heading.Contains('_') == false)
				{
					_errors.Add(
						new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "19b",
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
						new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "19b",
							RowNumber = group.HeadingRow,
							Group = group.Name,
							Field = heading,
							Message = $"Heading {heading} should consist of a 4 character group name and a field name of up to 4 characters.",
						});
				}

				var stdDictHeadings = _stdDictionary.GetGroup("DICT")?.GetColumn("DICT_HDNG").Data;
				var agsDictHeadings = _groups.GetGroup("DICT")?.GetColumn("DICT_HDNG")?.Data;

				var allDictHeadings = (stdDictHeadings ?? Enumerable.Empty<string>()).Concat(agsDictHeadings?? Enumerable.Empty<string>()).Distinct();

				if (allDictHeadings.Any(s => s.Contains(heading)) == false)
				{
					_errors.Add(
						new RuleErrorModel()
						{
							Status = "Fail",
							RuleId = "19b",
							RowNumber = group.HeadingRow,
							Group = group.Name,
							Field = heading,
							Message = $"Group name {splitHeading[0]} of heading {heading} not present in the standard or AGS file data dictionaries.",
						});
				}
			}
		}

		private void Rule20()
		{
			// Additional computer files (e.g. digital images) can be included within a data submission. Each such file shall be defined in a FILE GROUP.
			// The additional files shall be transferred in a sub-folder named FILE. This FILE sub - folder shall contain additional sub-folders each
			// named by the FILE_FSET reference. Each FILE_FSET named folder will contain the files listed in the FILE GROUP.

			AgsGroupModel fileGroup = _groups.GetGroup("FILE");

			var fSetColumns = _groups.GetAllColumnsOfHeading("FILE_FSET", "FILE");

            if (fileGroup is null && fSetColumns.Any())
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "20",
					Group = "FILE",
					Message = $"FILE table not found even though there are FILE_FSET entries in other tables.",
				});
			}

            foreach (var fSetColumn in fSetColumns)
            {
                for (int i = 0; i < fSetColumn.Data.Count; i++)
                {
					if (string.IsNullOrWhiteSpace(fSetColumn.Data[i]))
						continue;

					var fileGroupFSetEntries = fileGroup.GetColumn("FILE_FSET").Data;

					if (fileGroupFSetEntries.Contains(fSetColumn.Data[i]))
						continue;

					_errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "20",
						Group = fSetColumn.Group,
						RowNumber = _groups.GetGroup(fSetColumn.Group).FirstDataRow + i,
						Message = $"FILE_FSET entry \"{fSetColumn.Data[i]}\" not found in FILE table.",
					});
				}
            }

			string baseDir = Path.GetDirectoryName(_filePath);
			string fileFolderPath = Path.Combine(baseDir, "FILE");

			if (Directory.Exists(Path.Combine(baseDir, "FILE")) == false)
            {
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "20",
					Group = "FILE",
					Message = $"Folder named \"FILE\" not found. Files defined in the FILE table should be saved in this folder.",
				});
				return;
			}

			var fileGroupFolderNames = fileGroup.GetColumn("FILE_FSET").ReturnDataDistinctNonBlank();
			var subFolders = Directory.GetDirectories(fileFolderPath).Select(i => Path.GetFileName(i));

			fileGroupFolderNames.Except(subFolders).ToList().ForEach(missingSubFolder =>
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "20",
					Group = "FILE",
					Message = $"Sub-folder named \"{Path.Combine("FILE", missingSubFolder)}\" not found even though it is defined in the FILE table.",
				});
			});

			var fileGroupFileNames = fileGroup.GetColumn("FILE_NAME").ReturnDataDistinctNonBlank();
			var subFolderFiles = Directory.GetDirectories(fileFolderPath).SelectMany(d => Directory.GetFiles(d).Select(f => Path.GetFileName(f)));
			var refDict = fileGroup.GetRows();

			fileGroupFileNames.Except(subFolderFiles).ToList().ForEach(missingSubFile =>
			{
				var temp = refDict.AndBy("FILE_NAME", missingSubFile).ReturnFirstValueOf("FILE_FSET");
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "20",
					Group = "FILE",
					Message = $"File named \"{Path.Combine("FILE", temp, missingSubFile)}\" not found even though it is defined in the FILE table.",
				});
			});
		}
	}
}

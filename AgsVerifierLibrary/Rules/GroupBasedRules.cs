using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgsVerifierLibrary.Rules
{
    public class GroupBasedRules
    {
		private static readonly Regex _regexAgsHeadingField = new(@"[^A-Z0-9_]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly Type _groupType = typeof(AgsGroupModel);
		private static readonly Dictionary<string, string> _descriptors = new()
		{
			{ "HEADING", "HeadingRow"},
			{ "UNIT", "UnitRow" },
			{ "TYPE", "TypeRow" },
		};

        private readonly List<AgsGroupModel> _groups;
        private readonly List<AgsGroupModel> _stdDictionary;
        private readonly List<RuleErrorModel> _errors;
        private readonly List<string> _stdDictHeadings;
        private readonly List<string> _agsDictHeadings;
        private readonly List<string> _allDictHeadings;
        private readonly List<string> _agsTypeTypes;

        public GroupBasedRules(List<AgsGroupModel> groups, List<AgsGroupModel> stdDictionary, List<RuleErrorModel> errors)
        {
			_groups = groups;
            _stdDictionary = stdDictionary;
            _errors = errors;

			_stdDictHeadings = stdDictionary.FirstOrDefault(d => d.Name == "DICT").Columns.FirstOrDefault(c => c.Heading == "DICT_HDNG").Data;
			_agsDictHeadings = groups.FirstOrDefault(g => g.Name == "DICT").Columns.FirstOrDefault(c => c.Heading == "DICT_HDNG").Data;
			_allDictHeadings = (_stdDictHeadings ?? Enumerable.Empty<string>()).Concat(_agsDictHeadings ?? Enumerable.Empty<string>()).Distinct().ToList();

			_agsTypeTypes = groups.FirstOrDefault(g => g.Name == "TYPE").Columns.FirstOrDefault(c => c.Heading == "TYPE_TYPE").Data;
		}

		public void CheckGroups()
		{
			Rule17(); // Reliant only on TYPE group
			Rule18(); // Reliant only on DICT group

			foreach (var group in _groups)
			{
				Rule2(group);
				Rule2b(group);
				Rule7(group);
				Rule9(group);
				Rule10a(group);
				Rule10b(group);
				Rule10c(group);
				Rule11(group);
				Rule11c(group);
				Rule12();
				Rule13(group);
				Rule14(group);
				Rule15(group);
				Rule16(group);
				Rule18a(group);
				Rule19(group);
				Rule19a(group);
				Rule19b(group);
				Rule20(group);
			}
		}

		private void Rule2(AgsGroupModel group)
		{
			if (group.DataFrame.FilterRowsByColumn("HEADING", "DATA").Any())
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
			foreach (var key in _descriptors.Keys)
            {
				// Gets HEADING UNIT TYPE row integers using reflection checks if not 0
				if ((int)_groupType.GetProperty(_descriptors[key]).GetValue(group) == 0)
                {
					_errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "2b",
						Group = group.Name,
						Message = $"{key} row missing from the {group.Name} group.",
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
			var dictHeadings = _stdDictionary.FirstOrDefault(t => t.Name == "DICT").DataFrame.FilterColumnToList("DICT_GRP", group.Name, "DICT_HDNG");
			var agsHeadings = group.Columns.Select(c => c.Heading).ToList();

			var intersectDictToAgs = dictHeadings.Intersect(agsHeadings).ToArray();
			var intersectAgsToDict = agsHeadings.Intersect(dictHeadings).ToArray();

			if (intersectDictToAgs.SequenceEqual(intersectAgsToDict))
				return;

			for (int i = 0; i < intersectDictToAgs.Length; i++)
			{
				if (intersectDictToAgs[i] == intersectAgsToDict[i])
					continue;

				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "7",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Headings not in order starting from {intersectAgsToDict[i]}. Expected order: ...{string.Join('|', intersectDictToAgs[i..])}",
				});
				return;
			}
		}

		private void Rule9(AgsGroupModel group)
		{
			var agsDictHeadings = _groups.FirstOrDefault(g => g.Name == "DICT").DataFrame.FilterColumnToList("DICT_GRP", group.Name, "DICT_HDNG");
			var agsHeadings = group.Columns.Select(c => c.Heading).ToList();

			var intersectDictToAgs = agsDictHeadings.Intersect(agsHeadings).ToArray();
			var intersectAgsToDict = agsHeadings.Intersect(agsDictHeadings).ToArray();

			if (intersectDictToAgs.SequenceEqual(intersectAgsToDict))
				return;

			for (int i = 0; i < intersectDictToAgs.Length; i++)
			{
				if (intersectDictToAgs[i] == intersectAgsToDict[i])
					continue;

				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "9",
					Group = group.Name,
					RowNumber = group.HeadingRow,
					Message = $"Headings not in order starting from {intersectAgsToDict[i]}. Expected order: ...{string.Join('|', intersectDictToAgs[i..])}",
				});
				return;
			}
		}

		private void Rule10a(AgsGroupModel group)
		{

		}

		private void Rule10b(AgsGroupModel group)
		{

		}

		private void Rule10c(AgsGroupModel group)
		{

		}

		private void Rule11(AgsGroupModel group)
		{

		}

		private void Rule11c(AgsGroupModel group)
		{

		}

		private void Rule12()
		{

		}

		private void Rule13(AgsGroupModel group)
		{
            if (group.Name is not "PROJ")
				return;

			if (group.DataFrame.Rows.Count == 1)
				return;

			else if (group.DataFrame.Rows.Count == 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "13",
					RowNumber = group.GroupRow,
					Group = group.Name,
					Message = "There should be at least one DATA row in the PROJ table.",
				});
			}

			else if (group.DataFrame.Rows.Count > 0)
			{
				_errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "13",
					RowNumber = group.GroupRow,
					Group = group.Name,
					Message = "There should not be more than one DATA row in the PROJ table.",
				});
			}

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "13",
				RowNumber = group.FirstDataRow,
				Group = group.Name,
				Message = "Each AGS data file shall contain the PROJ GROUP.",
			});
		}
		
		private void Rule14(AgsGroupModel group)
		{

		}

		private void Rule15(AgsGroupModel group)
		{

		}

		private void Rule16(AgsGroupModel group)
		{

		}

		private void Rule17()
		{
			//Each data file shall contain the TYPE GROUP to define the field TYPEs used within the data file.
			//Every data type entered in the TYPE row of a GROUP shall be listed and defined in the TYPE GROUP.
			AgsGroupModel typeGroup = _groups.FirstOrDefault(g => g.Name == "TYPE");

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
				if (_agsTypeTypes.Contains(typeColumn) || typeColumn == "TYPE")
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
			if (_agsDictHeadings.Count > 0)
				return;

			_errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "18",
				Group = "DICT",
				Message = "DICT table not found. See error log under Rule 9 for a list of non-standard headings that need to be defined in a DICT table.",
			});
		}

		private void Rule18a(AgsGroupModel group)
        {

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

		private void Rule19b(AgsGroupModel group)
		{
			// TODO
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

				if (_allDictHeadings.Any(s => s.Contains(heading)) == false)
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

		private void Rule20(AgsGroupModel group)
		{

		}
	}
}

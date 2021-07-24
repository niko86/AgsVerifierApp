using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Rules
{
    public static class GroupBasedRules
    {
		private static readonly Type groupType = typeof(AgsGroupModel);
		private static readonly Dictionary<string, string> _descriptors = new()
		{
			{ "HEADING", "HeadingRow"},
			{ "UNIT", "UnitRow" },
			{ "TYPE", "TypeRow" },
		};

		public static void CheckGroups(List<AgsGroupModel> groups, List<RuleErrorModel> errors)
		{
			foreach (var group in groups)
			{
				Rule2(group, errors);
				Rule2b(group, errors);
				Rule7(group, errors);
				Rule9(group, errors);
				Rule10a(group, errors);
				Rule10b(group, errors);
				Rule10c(group, errors);
				Rule11(group, errors);
				Rule11c(group, errors);
				Rule12();
				Rule13(group, errors);
				Rule14(group, errors);
				Rule15(group, errors);
				Rule16(group, errors);
				Rule17(group, errors);
				Rule18(group, errors);
				Rule19b_2(group, errors);
				Rule19c(group, errors);
				Rule20(group, errors);
			}
		}

		private static void Rule2(AgsGroupModel group, List<RuleErrorModel> errors)
		{
			int dataCount = group.DataFrame.FilterRowsByColumn("HEADING", "DATA").Count();

			if (dataCount > 0)
				return;

			errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "2",
				Group = group.Name,
				Message = $"No DATA rows in the {group.Name} group.",
			});
		}

		private static void Rule2b(AgsGroupModel group, List<RuleErrorModel> errors)
		{
			// Have to trust group is present
            foreach (var key in _descriptors.Keys)
            {
				// Gets HEADING UNIT TYPE row integers using reflection checks if not 0
				if ((int)groupType.GetProperty(_descriptors[key]).GetValue(group) == 0)
                {
					errors.Add(new RuleErrorModel()
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
				errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "2b",
					Group = group.Name,
					Message = $"UNIT row is misplaced. It should be immediately below the HEADING row.",
				});
			}

			if (orderTestC || orderTestD)
			{
				errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "2b",
					Group = group.Name,
					Message = $"TYPE row is misplaced. It should be immediately below the UNIT row.",
				});
			}
		}

		private static void Rule7(AgsGroupModel group, List<RuleErrorModel> errors)
		{
			
		}

		private static void Rule9(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule10a(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule10b(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule10c(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule11(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule11c(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule12()
		{
			return;
		}

		private static void Rule13(AgsGroupModel group, List<RuleErrorModel> errors)
		{
            if (group.Name is not "PROJ")
				return;

			if (group.DataFrame.Rows.Count == 1)
				return;

			if (group.DataFrame.Rows.Count == 0)
			{
				errors.Add(new RuleErrorModel()
				{
					Status = "Fail",
					RuleId = "13",
					RowNumber = group.GroupRow,
					Group = group.Name,
					Message = "There should be at least one DATA row in the PROJ table.",
				});
			}

			errors.Add(new RuleErrorModel()
			{
				Status = "Fail",
				RuleId = "13",
				RowNumber = group.FirstDataRow,
				Group = group.Name,
				Message = "There should not be more than one DATA row in the PROJ table.",
			});
		}

		private static void Rule14(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule15(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule16(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule17(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule18(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule19b_2(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule19c(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}

		private static void Rule20(AgsGroupModel group, List<RuleErrorModel> errors)
		{

		}
	}
}

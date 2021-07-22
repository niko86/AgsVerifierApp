using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Rules
{
    class GroupBasedRules
    {
		public static void CheckGroups(List<AgsGroupModel> groups, List<RuleErrorModel> errors)
		{
			var group = groups.First(g => g.Name == "TEST");
			//foreach (var group in groups)
			//{
			Rule2(group, errors);
			Rule2b(group, errors);
			//}
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
			var df = group.DataFrame;

			List<string> descriptors = new() { "HEADING", "UNIT", "TYPE" };

			for (int i = 1; i <= 2; i++) //Loop through second half of the list.
			{
				int descriptorCount = df.FilterRowsByColumn("HEADING", descriptors[i]).Count();

				if (descriptorCount == 0)
				{
					errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "2b",
						Group = group.Name,
						Message = $"{descriptors[i]} row missing from the {group.Name} group.",
					});
				}

				if (df.Rows[i - 1][0].ToString() != descriptors[i])
				{
					errors.Add(new RuleErrorModel()
					{
						Status = "Fail",
						RuleId = "2b",
						Group = group.Name,
						Message = $"{descriptors[i]} row is misplaced. It should be immediately below the {descriptors[i - 1]} row.",
					});
				}
			}
		}
	}
}

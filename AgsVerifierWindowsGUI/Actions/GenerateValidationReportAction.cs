using AgsVerifierLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class GenerateValidationReportAction
    {
        public static string Run(List<RuleError> errors, TimeSpan elapsed)
        {
            StringBuilder sb = new();

            var groupedErrors = errors.OrderBy(i => i.RuleId).GroupBy(k => k.RuleName);

            sb.AppendLine($"{errors.Count} errors identified:");
            sb.AppendLine();

            foreach (var groupedError in groupedErrors)
            {
                sb.AppendLine($"{groupedError.First().Status}: Rule {groupedError.First().RuleName}");

                foreach (var error in groupedError)
                {
                    sb.AppendLine($"  Line {error.RowNumber}: {error.Group} -  {error.Message}");
                }

                sb.AppendLine();
            }
            sb.AppendLine(new string('-', 140));
            sb.AppendLine($"Finished: Process took {elapsed.TotalSeconds:N3} seconds");

            return sb.ToString();
        }
    }
}

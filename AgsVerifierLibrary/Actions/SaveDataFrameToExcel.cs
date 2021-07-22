using AgsVerifierLibrary.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Actions
{
    public class SaveDataFrameToExcel
    {
        public SaveDataFrameToExcel()
        {

        }

		public void Run(List<AgsGroupModel> groups, string basePath)
		{
            //using ExcelPackage excelPackage = new();
            //foreach (var group in groups)
            //{
            //    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(group.Name);
            //    ExcelTextFormat etf = new()
            //    {
            //        Culture = CultureInfo.InvariantCulture,
            //        TextQualifier = '"',
            //        Delimiter = ',',
            //        EOL = "\r\n",
            //    };

            //    worksheet.Cells[1, 1].LoadFromText(groupToCsv.Run(group).ToString(), etf);

            //    var datePosition = new List<int>();

            //    for (var i = 1; i <= group.Columns.Select(c => c.Type).Count(); i++)
            //        // Row 3 is where the Types are positioned
            //        if (worksheet.Cells[3, i].GetValue<string>() == "DT")
            //            datePosition.Add(i);
            //    foreach (var position in datePosition)
            //    {
            //        // Row 2 is where the Units are positioned
            //        worksheet.Column(position).Style.Numberformat.Format = worksheet.Cells[2, position].GetValue<string>();
            //    }
            //}

            ////Save your file
            //FileInfo fileInfo = new FileInfo(Path.Combine(basePath, "test.xlsx"));
            //excelPackage.SaveAs(fileInfo);
        }

	}
}

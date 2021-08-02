using Microsoft.Data.Analysis;
using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
	public class AgsGroupModel
	{
		public string Name { get; set; }
		public int GroupRow { get; set; }
		public int HeadingRow { get; set; }
		public int UnitRow { get; set; }
		public int TypeRow { get; set; }
		public int FirstDataRow { get; set; }
		public string ParentGroup { get; set; }
		public List<AgsColumnModel> Columns { get; set; }
		public DataFrame DataFrame { get; set; }

		public AgsColumnModel this[int rowIndex]
		{
			get => Columns[rowIndex];
			set => Columns[rowIndex] = value;
		}
	}
}

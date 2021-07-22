using Microsoft.Data.Analysis;
using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
	public class AgsGroupModel
	{
		// make a header class to store headers and the unit type data etc. 
		// HOW does the dataframe put back following changes? IS IT NEEDED?
		public int Index { get; set; }
		public string Name { get; set; }
		public List<AGSColumn> Columns { get; set; } = new();
		public DataFrame DataFrame { get; set; }
	}
}

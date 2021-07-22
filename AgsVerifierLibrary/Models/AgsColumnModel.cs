using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
	public class AGSColumn
	{
		public int Index { get; set; }
		public string Heading { get; set; }
		public string Type { get; set; }
		public string Unit { get; set; }
		public List<string> DataColumn { get; set; } = new();
		public StatusEnum Status { get; set; }
	}
}

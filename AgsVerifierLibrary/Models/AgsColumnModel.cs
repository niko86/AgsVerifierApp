using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
	public class AGSColumnModel
	{
		public int Index { get; set; }
		public string Heading { get; set; }
		public string Type { get; set; }
		public string Unit { get; set; }
		public List<string> Data { get; set; } = new();
		public StatusEnum Status { get; set; }
	}
}

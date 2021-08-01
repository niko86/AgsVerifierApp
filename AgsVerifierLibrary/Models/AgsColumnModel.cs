using System.Collections;
using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
	public class AgsColumnModel
	{
		public int Index { get; set; }
		public string Heading { get; set; }
		public string Type { get; set; }
		public string Unit { get; set; }
		public List<string> Data { get; private set; } = new();
		public string Status { get; set; }
		public string Group { get; set; }

		public string this[int rowIndex]
		{
			get => Data[rowIndex];
			set => Data[rowIndex] = value;
		}
	}
}

namespace AgsVerifierLibrary.Models
{
	public class RuleErrorModel
	{
		public string Status { get; set; }
		public string RuleId { get; set; }
		public int RowNumber { get; set; }
		public string Message { get; set; }
		public string Group { get; set; }
		public string Field { get; set; }
    }
}